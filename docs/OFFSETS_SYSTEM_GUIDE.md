# 🔍 Guia do Sistema de Offsets - Albion Sniffer

## 📋 **Visão Geral**

O sistema de **Offsets** é um componente fundamental do Albion Sniffer que permite **mapear e extrair dados específicos** dos pacotes de rede do jogo Albion Online. Os offsets funcionam como um "mapa" que indica **onde** encontrar cada tipo de informação dentro dos pacotes UDP capturados.

## 🎯 **O que são os Offsets?**

### **Definição Técnica:**
- **Offsets** são **índices numéricos** (bytes) que indicam a posição de dados específicos dentro dos pacotes de rede
- Cada offset representa uma **chave** no dicionário de parâmetros recebidos do jogo
- Os offsets são **configuráveis** e podem mudar entre diferentes versões do jogo

### **Analogia:**
Imagine um **formulário** onde cada campo tem um número:
```
Formulário: [0: Nome] [1: Idade] [2: Email] [3: Telefone]
Offsets:   [0, 1, 2, 3] → Extrai: Nome, Idade, Email, Telefone
```

## 🔧 **Como Funciona o Sistema**

### **1. Captura de Pacotes:**
```
📡 Pacote UDP capturado
    ↓
🔧 PacketCaptureService extrai dados
    ↓
🌐 Albion.Network processa protocolo
    ↓
📦 Dicionário de parâmetros criado
```

### **2. Aplicação de Offsets:**
```
📊 Parâmetros: {0: "João", 1: 25, 2: "joao@email.com"}
🔍 Offsets: [0, 1, 2]
    ↓
✅ Extração: Nome="João", Idade=25, Email="joao@email.com"
```

### **3. Mapeamento de Eventos:**
```csharp
// Exemplo: NewCharacterEvent
public class NewCharacterEvent : BaseEvent
{
    private readonly byte[] offsets;
    
    public NewCharacterEvent(Dictionary<byte, object> parameters)
    {
        // Carrega offsets do arquivo JSON
        var packetOffsets = PacketOffsetsLoader.GlobalPacketOffsets;
        offsets = packetOffsets?.NewCharacter ?? new byte[] { 0, 1, 2, 3, 4, 5 };
        
        // Extrai dados usando os offsets
        Id = SafeParameterExtractor.GetInt32(parameters, offsets[0]);
        Name = SafeParameterExtractor.GetString(parameters, offsets[1]);
        GuildName = SafeParameterExtractor.GetString(parameters, offsets[2]);
        // ... outros campos
    }
}
```

## 📁 **Estrutura dos Arquivos de Offsets**

### **Arquivo Principal: `offsets.json`**
```json
{
    "NewCharacter": [0, 1, 8, 51, 53, 16, 20, 22, 23, 40, 43],
    "Move": [0, 1],
    "NewHarvestableObject": [0, 5, 7, 8, 10, 11],
    "KeySync": [0]
}
```

### **Modelo C#: `PacketOffsets.cs`**
```csharp
public class PacketOffsets
{
    public byte[] NewCharacter { get; set; }
    public byte[] Move { get; set; }
    public byte[] NewHarvestableObject { get; set; }
    public byte[] KeySync { get; set; }
    // ... outros eventos
}
```

## 🔍 **Tipos de Dados Mapeados**

### **Eventos de Jogador:**
- **NewCharacter**: [0, 1, 8, 51, 53, 16, 20, 22, 23, 40, 43]
  - `0` = ID do personagem
  - `1` = Nome do personagem
  - `8` = Nome da guild
  - `51` = Nome da aliança
  - `53` = Facção
  - `16, 20, 22, 23, 40, 43` = Outros dados (posição, equipamentos)

### **Eventos de Movimento:**
- **Move**: [0, 1]
  - `0` = ID do jogador
  - `1` = Dados de movimento (posição, velocidade, flags)

### **Eventos de Objetos:**
- **NewHarvestableObject**: [0, 5, 7, 8, 10, 11]
  - `0` = ID do objeto
  - `5` = Tipo do objeto
  - `7` = Posição X
  - `8` = Posição Y
  - `10` = Tier
  - `11` = Cargas restantes

## 🚨 **Pacotes Já Decifrados?**

### **❌ NÃO!** Os pacotes **NÃO** estão decifrados:
- Os pacotes UDP são **capturados em bruto** da rede
- O **Albion.Network** faz a **descriptografia inicial** do protocolo Photon
- Os **offsets** são aplicados **após** a descriptografia do protocolo
- Os dados ainda podem estar **criptografados** (ex: posições com XOR)

### **Fluxo de Descriptografia:**
```
📡 Pacote UDP Bruto
    ↓
🔓 Albion.Network (descriptografia Photon)
    ↓
📦 Dicionário de parâmetros
    ↓
🔍 Aplicação de offsets
    ↓
🔓 Descriptografia adicional (se necessário)
    ↓
✅ Dados finais utilizáveis
```

## 🎯 **É Possível Aprender Novos Offsets?**

### **✅ SIM!** É possível descobrir novos offsets através de:

#### **1. Análise de Pacotes:**
- **Capturar** pacotes durante diferentes ações do jogo
- **Comparar** parâmetros entre eventos similares
- **Identificar** padrões nos dados recebidos

#### **2. Engenharia Reversa:**
- **Analisar** o código do jogo (se disponível)
- **Monitorar** mudanças entre versões
- **Testar** diferentes cenários de jogo

#### **3. Ferramentas de Debug:**
```csharp
// Exemplo de debug para descobrir offsets
public void DebugParameters(Dictionary<byte, object> parameters)
{
    foreach (var kvp in parameters.OrderBy(x => x.Key))
    {
        Console.WriteLine($"Offset {kvp.Key}: {kvp.Value} ({kvp.Value?.GetType().Name})");
    }
}
```

### **4. Estratégias de Descoberta:**
- **Logging** de todos os parâmetros recebidos
- **Comparação** entre diferentes versões do jogo
- **Análise** de eventos específicos (ex: crafting, combate)
- **Monitoramento** de mudanças de estado

## 🗺️ **Mapeamento de Serviços**

### **✅ SIM!** É possível criar um mapeamento completo:

#### **1. Mapeamento Automático:**
```csharp
public class OffsetDiscoveryService
{
    public Dictionary<string, Dictionary<byte, object>> DiscoverOffsets()
    {
        var discoveredOffsets = new Dictionary<string, Dictionary<byte, object>>();
        
        // Capturar eventos durante gameplay
        // Analisar padrões nos parâmetros
        // Gerar mapeamento automático
        
        return discoveredOffsets;
    }
}
```

#### **2. Validação de Offsets:**
```csharp
public class OffsetValidator
{
    public bool ValidateOffsets(byte[] offsets, Dictionary<byte, object> parameters)
    {
        foreach (var offset in offsets)
        {
            if (!parameters.ContainsKey(offset))
            {
                return false; // Offset inválido
            }
        }
        return true;
    }
}
```

#### **3. Geração de Configurações:**
```csharp
public class OffsetConfigGenerator
{
    public string GenerateOffsetsJson(Dictionary<string, byte[]> discoveredOffsets)
    {
        // Gerar arquivo offsets.json atualizado
        // Baseado nos offsets descobertos
        return JsonSerializer.Serialize(discoveredOffsets, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
    }
}
```

## 🔧 **Implementação Atual**

### **Serviços Principais:**

#### **1. PacketOffsetsLoader:**
- Carrega offsets do arquivo JSON
- Gerencia offsets globais
- Fallback para offsets padrão

#### **2. PacketOffsetsProvider:**
- Provedor estático para acesso aos offsets
- Configuração via dependency injection
- Cache de offsets para performance

#### **3. SafeParameterExtractor:**
- Extração segura de parâmetros
- Prevenção de KeyNotFoundException
- Conversão de tipos com fallback

### **Configuração:**
```csharp
// Program.cs
services.AddSingleton<PacketOffsets>(provider =>
{
    var offsetsLoader = provider.GetRequiredService<PacketOffsetsLoader>();
    var offsetsPath = "src/AlbionOnlineSniffer.Core/Data/jsons/offsets.json";
    return offsetsLoader.LoadOffsets(offsetsPath);
});

// Configurar provider estático
PacketOffsetsProvider.Configure(serviceProvider);
```

## 📊 **Vantagens do Sistema de Offsets**

### **✅ Flexibilidade:**
- **Configurável** sem recompilação
- **Atualizável** via arquivo JSON
- **Compatível** com diferentes versões

### **✅ Robustez:**
- **Fallback** para offsets padrão
- **Validação** de parâmetros
- **Tratamento** de erros elegante

### **✅ Manutenibilidade:**
- **Centralizado** em um local
- **Documentado** e estruturado
- **Testável** e validável

## 🚀 **Próximos Passos**

### **1. Descoberta Automática:**
- Implementar serviço de descoberta de offsets
- Logging automático de parâmetros
- Geração de configurações

### **2. Validação:**
- Validação automática de offsets
- Detecção de offsets obsoletos
- Sugestões de atualização

### **3. Documentação:**
- Mapeamento completo de todos os offsets
- Exemplos de uso para cada evento
- Guias de troubleshooting

## 📝 **Conclusão**

O sistema de **Offsets** é uma solução elegante e flexível para mapear dados dos pacotes de rede do Albion Online. Ele permite:

- **Extração precisa** de dados específicos
- **Configuração flexível** via arquivos JSON
- **Descoberta contínua** de novos offsets
- **Manutenção simplificada** do sistema

Com as ferramentas e estratégias adequadas, é possível criar um mapeamento completo e atualizado dos offsets, mantendo o sistema sempre funcional e compatível com as mudanças do jogo.
