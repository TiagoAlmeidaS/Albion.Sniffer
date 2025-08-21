# 🔍 Análise do Fluxo de Pacotes - Albion Sniffer

## 📋 **Visão Geral**

Este documento analisa o **fluxo completo** de processamento de pacotes no Albion Sniffer, desde a **interceptação** na rede até o **mapeamento de offsets** e **geração de eventos**. O objetivo é identificar os **pontos ideais** para interceptação e envio de dados para filas de descoberta.

## 🔄 **Fluxo Completo de Processamento**

### **1. Interceptação de Pacotes UDP**
```
🌐 Rede (Porta 5050)
    ↓
📡 PacketCaptureService.Device_OnPacketArrival()
    ↓
🔧 ExtractUdpPacket() - Extrai UDP de qualquer estrutura
    ↓
📦 ProcessUdpPacket() - Processa payload UDP
    ↓
🚀 OnUdpPayloadCaptured event disparado
```

**Localização:** `src/AlbionOnlineSniffer.Capture/PacketCaptureService.cs:120-140`

**Código Chave:**
```csharp
private void Device_OnPacketArrival(object sender, PacketCapture e)
{
    var rawPacket = e.GetPacket();
    var packet = Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);
    
    var udpPacket = ExtractUdpPacket(packet);
    if (udpPacket != null)
    {
        ProcessUdpPacket(udpPacket); // ✅ PONTO DE INTERCEPTAÇÃO 1
    }
}

private void ProcessUdpPacket(UdpPacket udpPacket)
{
    var payload = udpPacket.PayloadData;
    if (payload != null && payload.Length > 0)
    {
        // ✅ PONTO DE INTERCEPTAÇÃO 2 - Payload UDP bruto
        OnUdpPayloadCaptured?.Invoke(payload);
    }
}
```

### **2. Pipeline de Captura**
```
🚀 OnUdpPayloadCaptured event
    ↓
🔧 CapturePipeline.OnPacket()
    ↓
🔧 Protocol16Deserializer.ReceivePacket()
    ↓
🌐 IPhotonReceiver.ReceivePacket() (Albion.Network)
```

**Localização:** `src/AlbionOnlineSniffer.App/Services/CapturePipeline.cs:50-60`

**Código Chave:**
```csharp
private void OnPacket(byte[] packetData)
{
    try
    {
        _logger.LogInformation("📡 PACOTE UDP CAPTURADO: {Length} bytes", packetData?.Length ?? 0);
        // ✅ PONTO DE INTERCEPTAÇÃO 3 - Antes do parsing
        _deserializer.ReceivePacket(packetData);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Erro ao processar pacote UDP");
    }
}
```

### **3. Deserialização do Protocolo**
```
🌐 IPhotonReceiver.ReceivePacket()
    ↓
🔧 Albion.Network.ReceiverBuilder
    ↓
🎮 Handlers específicos processam o pacote
    ↓
✨ Eventos do jogo são criados
```

**Localização:** `src/AlbionOnlineSniffer.Core/Services/Protocol16Deserializer.cs:30-40`

**Código Chave:**
```csharp
public void ReceivePacket(byte[] payload)
{
    try
    {
        _logger.LogDebug("Recebendo pacote UDP de {PayloadLength} bytes", payload.Length);
        
        // ✅ PONTO DE INTERCEPTAÇÃO 4 - Antes do Albion.Network
        _photonReceiver.ReceivePacket(payload);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Erro ao processar pacote UDP: {Message}", ex.Message);
    }
}
```

### **4. Processamento por Handlers**
```
🎮 Handler específico (ex: NewCharacterEventHandler)
    ↓
🔍 Aplicação de offsets aos parâmetros
    ↓
✨ Criação de eventos do jogo
    ↓
🚀 EventDispatcher.DispatchEvent()
```

**Localização:** `src/AlbionOnlineSniffer.Core/Handlers/NewCharacterEventHandler.cs:50-80`

**Código Chave:**
```csharp
protected override async Task OnActionAsync(NewCharacterEvent value)
{
    // 🔍 OFFSETS APLICADOS AQUI - PONTO DE INTERCEPTAÇÃO 5
    // value.Id, value.Name, value.GuildName, etc. já foram extraídos usando offsets
    
    // ✅ PONTO DE INTERCEPTAÇÃO 6 - Evento criado com dados mapeados
    var playerSpottedV1 = new PlayerSpottedV1
    {
        EventId = Guid.NewGuid().ToString("n"),
        ObservedAt = DateTimeOffset.UtcNow,
        PlayerId = value.Id,        // ← Offset 0 aplicado
        PlayerName = value.Name,    // ← Offset 1 aplicado
        GuildName = value.GuildName, // ← Offset 2 aplicado
        // ... outros campos
    };
    
    // ✅ PONTO DE INTERCEPTAÇÃO 7 - Antes do dispatch
    await eventDispatcher.DispatchEvent(playerSpottedV1);
}
```

### **5. Despacho de Eventos**
```
🚀 EventDispatcher.DispatchEvent()
    ↓
📋 Handlers específicos por tipo
    ↓
🌍 Handlers globais
    ↓
📤 EventToQueueBridge (publica na fila)
```

**Localização:** `src/AlbionOnlineSniffer.Core/Services/EventDispatcher.cs:50-80`

**Código Chave:**
```csharp
public async Task DispatchEvent(object gameEvent)
{
    try
    {
        var eventType = gameEvent.GetType().Name;
        
        // ✅ PONTO DE INTERCEPTAÇÃO 8 - Evento sendo despachado
        
        // Handlers específicos para o tipo de evento
        if (_eventHandlers.ContainsKey(eventType))
        {
            foreach (var handler in _eventHandlers[eventType])
            {
                tasks.Add(handler(gameEvent));
            }
        }
        
        // Handlers globais
        foreach (var handler in _globalHandlers)
        {
            tasks.Add(handler(gameEvent));
        }
        
        await Task.WhenAll(tasks);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Erro ao disparar evento: {EventType}", eventType);
    }
}
```

## 🎯 **Pontos Ideais para Interceptação**

### **🔴 PONTO 1: Payload UDP Bruto (Recomendado)**
**Localização:** `PacketCaptureService.ProcessUdpPacket()`

**Vantagens:**
- ✅ **Dados brutos** - Sem processamento
- ✅ **Completo** - Todo o payload UDP
- ✅ **Cedo no fluxo** - Antes de qualquer parsing
- ✅ **Simples** - Apenas bytes

**Dados Disponíveis:**
```json
{
    "EventName": "UDP_PAYLOAD_RAW",
    "Parameters": {
        "Payload": "bytes[]",
        "Length": "int",
        "SourcePort": "int",
        "DestinationPort": "int",
        "Timestamp": "DateTime"
    }
}
```

### **🟡 PONTO 2: Antes do Parsing Albion.Network**
**Localização:** `Protocol16Deserializer.ReceivePacket()`

**Vantagens:**
- ✅ **Antes da descriptografia** - Dados ainda criptografados
- ✅ **Payload completo** - Sem perda de dados
- ✅ **Fácil interceptação** - Método simples

**Dados Disponíveis:**
```json
{
    "EventName": "BEFORE_ALBION_NETWORK",
    "Parameters": {
        "Payload": "bytes[]",
        "Length": "int",
        "Timestamp": "DateTime"
    }
}
```

### **🟢 PONTO 3: Evento com Offsets Aplicados**
**Localização:** `NewCharacterEventHandler.OnActionAsync()`

**Vantagens:**
- ✅ **Dados mapeados** - Offsets já aplicados
- ✅ **Estruturado** - Objetos C# tipados
- ✅ **Contexto completo** - Evento + dados

**Dados Disponíveis:**
```json
{
    "EventName": "NewCharacterEvent",
    "Parameters": {
        "Id": "int",
        "Name": "string",
        "GuildName": "string",
        "AllianceName": "string",
        "PositionBytes": "bytes[]",
        "Items": "float[]"
    },
    "Offsets": [0, 1, 8, 51, 53, 16, 20, 22, 23, 40, 43],
    "Timestamp": "DateTime"
}
```

### **🔵 PONTO 4: Antes do Dispatch**
**Localização:** `EventDispatcher.DispatchEvent()`

**Vantagens:**
- ✅ **Evento final** - Dados processados
- ✅ **Tipo conhecido** - Evento específico
- ✅ **Antes da distribuição** - Controle total

## 🚀 **Implementação Recomendada**

### **Estratégia 1: Interceptação no PacketCaptureService (PONTO 1)**

```csharp
// ✅ MODIFICAÇÃO MÍNIMA - Adicionar evento
public class PacketCaptureService : IPacketCaptureService
{
    // ✅ NOVO EVENTO para descoberta
    public event Action<DiscoveryPacketData>? OnDiscoveryPacketCaptured;
    
    private void ProcessUdpPacket(UdpPacket udpPacket)
    {
        var payload = udpPacket.PayloadData;
        if (payload != null && payload.Length > 0)
        {
            // ✅ INTERCEPTAÇÃO PARA DESCOBERTA
            var discoveryData = new DiscoveryPacketData
            {
                EventName = "UDP_PAYLOAD_RAW",
                Parameters = new Dictionary<string, object>
                {
                    ["Payload"] = payload,
                    ["Length"] = payload.Length,
                    ["SourcePort"] = udpPacket.SourcePort,
                    ["DestinationPort"] = udpPacket.DestinationPort,
                    ["Timestamp"] = DateTime.UtcNow
                }
            };
            
            OnDiscoveryPacketCaptured?.Invoke(discoveryData);
            
            // Fluxo normal continua
            OnUdpPayloadCaptured?.Invoke(payload);
        }
    }
}

// ✅ NOVO MODELO para dados de descoberta
public class DiscoveryPacketData
{
    public string EventName { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
```

### **Estratégia 2: Interceptação no EventDispatcher (PONTO 4)**

```csharp
// ✅ MODIFICAÇÃO MÍNIMA - Adicionar evento
public class EventDispatcher
{
    // ✅ NOVO EVENTO para descoberta
    public event Action<DiscoveryEventData>? OnDiscoveryEventDispatched;
    
    public async Task DispatchEvent(object gameEvent)
    {
        try
        {
            var eventType = gameEvent.GetType().Name;
            
            // ✅ INTERCEPTAÇÃO PARA DESCOBERTA
            var discoveryData = new DiscoveryEventData
            {
                EventName = eventType,
                Parameters = ExtractEventParameters(gameEvent),
                Timestamp = DateTime.UtcNow
            };
            
            OnDiscoveryEventDispatched?.Invoke(discoveryData);
            
            // Fluxo normal continua
            // ... resto do código
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao disparar evento: {EventType}", eventType);
        }
    }
    
    private Dictionary<string, object> ExtractEventParameters(object gameEvent)
    {
        // Extrair propriedades do evento via reflection
        var parameters = new Dictionary<string, object>();
        var properties = gameEvent.GetType().GetProperties();
        
        foreach (var prop in properties)
        {
            try
            {
                var value = prop.GetValue(gameEvent);
                parameters[prop.Name] = value ?? "null";
            }
            catch
            {
                parameters[prop.Name] = "error";
            }
        }
        
        return parameters;
    }
}

// ✅ NOVO MODELO para dados de descoberta
public class DiscoveryEventData
{
    public string EventName { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
```

## 📊 **Comparação das Estratégias**

| Critério | PONTO 1 (UDP Bruto) | PONTO 2 (Antes Albion.Network) | PONTO 3 (Offsets Aplicados) | PONTO 4 (Antes Dispatch) |
|----------|---------------------|--------------------------------|------------------------------|---------------------------|
| **Dados** | ✅ Completos | ✅ Completos | ✅ Mapeados | ✅ Processados |
| **Complexidade** | 🟢 Baixa | 🟢 Baixa | 🟡 Média | 🟡 Média |
| **Timing** | 🟢 Muito cedo | 🟡 Cedo | 🟡 Médio | 🔴 Tarde |
| **Criptografia** | ✅ Preservada | ✅ Preservada | ❌ Descriptografada | ❌ Descriptografada |
| **Estrutura** | ❌ Bruta | ❌ Bruta | ✅ Estruturada | ✅ Estruturada |
| **Offsets** | ❌ Não aplicados | ❌ Não aplicados | ✅ Aplicados | ✅ Aplicados |

## 🎯 **Recomendação Final**

### **🟢 PONTO 1 (UDP Bruto) - RECOMENDADO**

**Justificativa:**
1. **Dados completos** - Sem perda de informação
2. **Criptografia preservada** - Dados originais
3. **Implementação simples** - Mínima modificação
4. **Flexibilidade máxima** - Permite qualquer tipo de análise

**Implementação:**
```csharp
// 1. Adicionar evento no PacketCaptureService
public event Action<DiscoveryPacketData>? OnDiscoveryPacketCaptured;

// 2. Conectar ao sistema de filas
_capture.OnDiscoveryPacketCaptured += async (discoveryData) =>
{
    await _discoveryQueue.PublishAsync("albion.discovery.raw", discoveryData);
};

// 3. Dados enviados para fila
{
    "EventName": "UDP_PAYLOAD_RAW",
    "Parameters": {
        "Payload": "bytes[]",
        "Length": "int",
        "Timestamp": "DateTime"
    }
}
```

### **🟡 PONTO 4 (Antes Dispatch) - ALTERNATIVA**

**Justificativa:**
1. **Dados processados** - Estruturados e tipados
2. **Offsets aplicados** - Mapeamento completo
3. **Contexto rico** - Eventos específicos do jogo

**Implementação:**
```csharp
// 1. Adicionar evento no EventDispatcher
public event Action<DiscoveryEventData>? OnDiscoveryEventDispatched;

// 2. Conectar ao sistema de filas
_eventDispatcher.OnDiscoveryEventDispatched += async (discoveryData) =>
{
    await _discoveryQueue.PublishAsync("albion.discovery.events", discoveryData);
};

// 3. Dados enviados para fila
{
    "EventName": "NewCharacterEvent",
    "Parameters": {
        "Id": "int",
        "Name": "string",
        "GuildName": "string"
    },
    "Offsets": [0, 1, 8, 51, 53, 16, 20, 22, 23, 40, 43]
}
```

## 📝 **Conclusão**

O **PONTO 1 (UDP Bruto)** é a **melhor opção** para interceptação e envio para filas de descoberta porque:

- ✅ **Preserva dados originais** - Sem perda de informação
- ✅ **Implementação simples** - Mínima modificação no código
- ✅ **Flexibilidade máxima** - Permite qualquer tipo de análise
- ✅ **Criptografia preservada** - Dados exatamente como recebidos
- ✅ **Performance** - Sem overhead de processamento

Com essa abordagem, você pode implementar um sistema de descoberta que:
1. **Captura** todos os pacotes UDP brutos
2. **Envia** para fila `albion.discovery.raw`
3. **Processa** em projeto separado para análise
4. **Descobre** novos offsets automaticamente
5. **Gera** configurações atualizadas

O sistema atual já está bem estruturado para essa interceptação, requerendo apenas a adição de um evento adicional no `PacketCaptureService`.
