# Análise do Sistema de Eventos - AlbionOnlineSniffer

## 🎯 **Visão Geral do Sistema**

O sistema de eventos do AlbionOnlineSniffer é uma arquitetura robusta e escalável que processa pacotes UDP do Albion Online, converte-os em eventos do jogo, e os distribui para múltiplos consumidores através de um sistema de despacho centralizado.

## 🔄 **Fluxo Completo dos Eventos**

### **1. Captura de Pacotes UDP**
```
📡 Pacote UDP (Porta 5050) 
    ↓
🔧 PacketCaptureService.ExtractUdpPacket()
    ↓
📦 UdpPacket extraído
    ↓
🚀 OnUdpPayloadCaptured event disparado
```

### **2. Processamento do Protocolo**
```
📦 Payload UDP
    ↓
🔧 Protocol16Deserializer.ReceivePacket()
    ↓
🌐 IPhotonReceiver.ReceivePacket()
    ↓
🎮 Handlers específicos processam o pacote
```

### **3. Geração de Eventos**
```
🎮 Handler processa pacote
    ↓
✨ Cria evento do jogo (ex: NewCharacterEvent)
    ↓
📢 Chama eventDispatcher.DispatchEvent()
    ↓
🚀 Evento é disparado para todos os handlers registrados
```

### **4. Distribuição de Eventos**
```
🚀 EventDispatcher.DispatchEvent()
    ↓
📋 Handlers específicos (por tipo de evento)
    ↓
🌍 Handlers globais (recebem todos os eventos)
    ↓
📤 EventToQueueBridge (publica na fila)
    ↓
🎯 Pipeline de eventos (processamento assíncrono)
```

## 🏗️ **Arquitetura dos Componentes**

### **1. PacketCaptureService**
- **Responsabilidade**: Captura pacotes UDP da rede
- **Eventos**: Dispara `OnUdpPayloadCaptured` quando um pacote é capturado
- **Métodos**: `ExtractUdpPacket()` - Extrai UDP de qualquer estrutura de pacote

### **2. CapturePipeline**
- **Responsabilidade**: Conecta captura UDP ao desserializador
- **Eventos**: Escuta `OnUdpPayloadCaptured` e chama `Protocol16Deserializer.ReceivePacket()`
- **Métricas**: Monitora performance da captura

### **3. Protocol16Deserializer**
- **Responsabilidade**: Interface entre captura e processamento
- **Dependência**: `IPhotonReceiver` (Albion.Network)
- **Método**: `ReceivePacket(byte[] payload)` - Encaminha payload para o receiver

### **4. EventHandlers**
- **Responsabilidade**: Processam pacotes específicos e geram eventos
- **Exemplos**: 
  - `NewCharacterEventHandler` → `NewCharacterEvent`
  - `HealthUpdateEventHandler` → `HealthUpdateEvent`
  - `MoveEventHandler` → `MoveEvent`
- **Padrão**: Herdam de `EventPacketHandler<T>` e chamam `eventDispatcher.DispatchEvent()`

### **5. EventDispatcher**
- **Responsabilidade**: Sistema centralizado de despacho de eventos
- **Funcionalidades**:
  - Handlers específicos por tipo de evento
  - Handlers globais (recebem todos os eventos)
  - Execução paralela de handlers
  - Logging e monitoramento

### **6. EventToQueueBridge**
- **Responsabilidade**: Ponte entre eventos e sistema de filas
- **Funcionalidades**:
  - Registra handler global no EventDispatcher
  - Converte eventos em mensagens para filas
  - Tópicos padronizados: `albion.event.{eventType}`
  - Extrai informações de posição quando disponível

## 🔧 **Configuração e Inicialização**

### **Program.cs - Configuração Principal**
```csharp
// 1. Registrar serviços Core (inclui EventDispatcher)
Core.DependencyProvider.RegisterServices(services);

// 2. Registrar serviços de captura
Capture.DependencyProvider.RegisterServices(services, configuration);

// 3. Registrar pipeline de captura
services.AddSingleton<App.Services.CapturePipeline>();

// 4. Conectar EventDispatcher ao Pipeline
eventDispatcher.RegisterGlobalHandler(async (eventData) =>
{
    await pipeline.EnqueueAsync(eventTypeName, eventData);
});

// 5. Conectar EventDispatcher ao Queue Bridge
serviceProvider.GetRequiredService<EventToQueueBridge>();
```

### **DependencyProvider.Core - Registro de Handlers**
```csharp
// Handlers são registrados automaticamente via DI
// Cada handler recebe EventDispatcher como dependência
// Handlers processam pacotes e disparam eventos
```

## 📊 **Tipos de Eventos Suportados**

### **Eventos de Jogadores**
- `NewCharacterEvent` - Novo jogador detectado
- `MoveEvent` - Movimento de jogador
- `HealthUpdateEvent` - Atualização de vida
- `CharacterEquipmentChangedEvent` - Equipamento alterado

### **Eventos de Mundo**
- `ChangeClusterEvent` - Mudança de cluster
- `NewHarvestableEvent` - Novo recurso coletável
- `NewMobEvent` - Novo mob spawnado
- `NewDungeonEvent` - Nova dungeon encontrada

### **Eventos de Sistema**
- `LoadClusterObjectsEvent` - Objetos do cluster carregados
- `KeySyncEvent` - Sincronização de chaves
- `FlaggingFinishedEvent` - Flagging finalizado

## 🚀 **Como os Eventos são Processados**

### **1. Captura e Extração**
```csharp
// PacketCaptureService extrai UDP de qualquer estrutura
var udpPacket = ExtractUdpPacket(packet);
if (udpPacket != null)
{
    ProcessUdpPacket(udpPacket);
    OnUdpPayloadCaptured?.Invoke(payload); // Evento disparado
}
```

### **2. Processamento do Protocolo**
```csharp
// CapturePipeline escuta o evento
_udpCaptureService.OnUdpPayloadCaptured += OnPacket;

private void OnPacket(byte[] packetData)
{
    _deserializer.ReceivePacket(packetData); // Encaminha para Albion.Network
}
```

### **3. Geração de Eventos**
```csharp
// Handler processa pacote e gera evento
protected override async Task OnActionAsync(NewCharacterEvent value)
{
    // Processa dados do pacote
    playerHandler.AddPlayer(value.Id, value.Name, ...);
    
    // Dispara evento para o EventDispatcher
    await eventDispatcher.DispatchEvent(value);
}
```

### **4. Distribuição**
```csharp
// EventDispatcher executa todos os handlers
public async Task DispatchEvent(object gameEvent)
{
    var eventType = gameEvent.GetType().Name;
    
    // Handlers específicos
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
    
    await Task.WhenAll(tasks); // Execução paralela
}
```

### **5. Publicação na Fila**
```csharp
// EventToQueueBridge escuta todos os eventos
_eventDispatcher.RegisterGlobalHandler(OnEventAsync);

private async Task OnEventAsync(object gameEvent)
{
    var topic = $"albion.event.{eventTypeFormatted.ToLowerInvariant()}";
    var message = new { EventType, Timestamp, Position, Data = gameEvent };
    
    await _publisher.PublishAsync(topic, message);
}
```

## 🔍 **Pontos de Monitoramento**

### **1. Logs de Captura**
```
📡 PACOTE UDP CAPTURADO: {Length} bytes
📊 CAPTURE MÉTRICAS: {Packets} pacotes válidos, {Rate} B/s
```

### **2. Logs de Eventos**
```
🎯 EVENTO RECEBIDO: {EventType} em {Timestamp}
📤 PUBLICANDO: {EventType} -> {Topic}
✅ Evento publicado na fila: {EventType} -> {Topic}
```

### **3. Logs de Pipeline**
```
🚀 Pipeline obtido: {PipelineType}
🔗 Pipeline conectado ao EventDispatcher
🔧 Configuração de handlers: {HandlerCount} handlers registrados
```

## 🎯 **Vantagens da Arquitetura**

### **1. Desacoplamento**
- Captura independente do processamento
- Handlers independentes do despacho
- Sistema de filas independente dos eventos

### **2. Escalabilidade**
- Handlers executam em paralelo
- Sistema de filas para processamento assíncrono
- Fácil adicionar novos consumidores de eventos

### **3. Manutenibilidade**
- Cada componente tem responsabilidade única
- Fácil de testar individualmente
- Logging centralizado e detalhado

### **4. Extensibilidade**
- Fácil adicionar novos tipos de eventos
- Fácil adicionar novos handlers
- Fácil adicionar novos consumidores

## 🔮 **Possíveis Melhorias**

### **1. Métricas Avançadas**
- Contadores de eventos por tipo
- Latência de processamento
- Taxa de eventos por segundo

### **2. Filtros de Eventos**
- Filtros por tipo de evento
- Filtros por posição geográfica
- Filtros por relevância

### **3. Cache de Eventos**
- Cache de eventos recentes
- Histórico de eventos
- Compressão de dados

## 📝 **Conclusão**

O sistema de eventos do AlbionOnlineSniffer é uma arquitetura bem projetada que:

✅ **Captura eficientemente** pacotes UDP da rede  
✅ **Processa robustamente** diferentes tipos de pacotes  
✅ **Gera eventos estruturados** para cada ação do jogo  
✅ **Distribui eventos** para múltiplos consumidores  
✅ **Publica eventos** em sistema de filas para processamento assíncrono  
✅ **Monitora e loga** todo o processo para debugging  

A arquitetura é **modular, escalável e extensível**, permitindo fácil manutenção e adição de novas funcionalidades. O sistema está preparado para uso em produção e pode ser facilmente adaptado para diferentes cenários de uso. 🎯
