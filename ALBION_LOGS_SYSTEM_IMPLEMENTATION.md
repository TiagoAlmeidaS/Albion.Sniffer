# 🚀 Sistema de Logs Personalizado - AlbionOnlineSniffer

## 📋 Visão Geral

Implementei um sistema de logs personalizado para captura de rede e eventos do Albion, que será usado pela interface web para exibir informações em tempo real. O sistema é completamente separado dos logs tradicionais do .NET.

## 🏗️ Arquitetura

### **1. AlbionEventLogger (Core)**
- **Localização**: `src/AlbionOnlineSniffer.Core/Services/AlbionEventLogger.cs`
- **Responsabilidade**: Sistema central de logs personalizado
- **Funcionalidades**:
  - Logs gerais com níveis (Debug, Information, Warning, Error, Critical)
  - Logs de captura de rede (pacotes UDP, erros, dispositivos)
  - Logs de eventos (processados, enviados para fila)
  - Métricas e estatísticas em tempo real
  - Limpeza automática de logs antigos

### **2. Interface IAlbionEventLogger**
- **Localização**: `src/AlbionOnlineSniffer.Core/Interfaces/IAlbionEventLogger.cs`
- **Responsabilidade**: Contrato para o sistema de logs
- **Métodos**:
  - `AddLog()` - Logs gerais
  - `LogUdpPacketCapture()` - Captura de pacotes UDP
  - `LogCaptureError()` - Erros de captura
  - `LogNetworkDevice()` - Dispositivos de rede
  - `LogEventProcessed()` - Eventos processados
  - `LogEventQueued()` - Eventos enviados para fila

### **3. AlbionLogsApiService**
- **Localização**: `src/AlbionOnlineSniffer.Core/Services/AlbionLogsApiService.cs`
- **Responsabilidade**: Exposição dos logs via API para interface web
- **Endpoints**:
  - `GetLogs()` - Logs gerais com filtros
  - `GetNetworkLogs()` - Logs de captura de rede
  - `GetEventLogs()` - Logs de eventos
  - `GetLogStatistics()` - Estatísticas dos logs
  - `GetLogsSummary()` - Resumo em tempo real

## 🔗 Integração nas Camadas

### **1. Camada de Captura (Capture)**
- **PacketCaptureService**: Registra logs de pacotes UDP capturados
- **PacketCaptureMonitor**: Integrado com o sistema de logs
- **Logs capturados**:
  - Pacotes UDP com payload e metadados
  - Erros de captura e dispositivos
  - Status de dispositivos de rede

### **2. Camada Core**
- **EventDispatcher**: Registra logs de eventos processados
- **AlbionNetworkHandlerManager**: Integrado com o sistema de logs
- **Logs capturados**:
  - Eventos do jogo processados
  - Sucesso/falha no processamento
  - Dados dos eventos

### **3. Camada de Fila (Queue)**
- **RabbitMqPublisher**: Registra logs de eventos enviados para fila
- **Logs capturados**:
  - Eventos enviados para RabbitMQ
  - Sucesso/falha na publicação
  - Detalhes da fila

## 📊 Modelos de Dados

### **LogEntry**
```csharp
public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public LogLevel Level { get; set; }
    public string Message { get; set; }
    public string Category { get; set; }
    public object? Data { get; set; }
}
```

### **NetworkCaptureLog**
```csharp
public class NetworkCaptureLog
{
    public DateTime Timestamp { get; set; }
    public NetworkCaptureType Type { get; set; }
    public string? SourceIp { get; set; }
    public int? SourcePort { get; set; }
    public string? DestinationIp { get; set; }
    public int? DestinationPort { get; set; }
    public int? PayloadSize { get; set; }
    public string? PayloadPreview { get; set; }
    public string? HexPreview { get; set; }
    public string? Error { get; set; }
    public string? Context { get; set; }
    public string? DeviceName { get; set; }
    public string? DeviceType { get; set; }
    public bool? IsValid { get; set; }
}
```

### **EventLog**
```csharp
public class EventLog
{
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; }
    public object? EventData { get; set; }
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? DataPreview { get; set; }
    public string? QueueName { get; set; }
    public string? Action { get; set; }
}
```

## ⚙️ Configuração

### **Dependency Injection**
```csharp
// Core
services.AddSingleton<IAlbionEventLogger, AlbionEventLogger>();
services.AddSingleton<AlbionEventLogger>();
services.AddSingleton<AlbionLogsApiService>();

// Capture
services.AddSingleton<IPacketCaptureService>(provider =>
{
    var eventLogger = provider.GetService<IAlbionEventLogger>();
    return new PacketCaptureService(5050, eventLogger);
});

// Queue
services.AddSingleton<IQueuePublisher>(provider =>
{
    var eventLogger = provider.GetService<IAlbionEventLogger>();
    return new RabbitMqPublisher(connectionString, exchange, eventLogger);
});
```

## 🎯 Funcionalidades da Interface Web

### **1. Monitoramento de Rede em Tempo Real**
- Pacotes UDP capturados
- Tamanho e preview do payload
- Portas origem/destino
- Status dos dispositivos de rede

### **2. Rastreamento de Eventos**
- Eventos do jogo processados
- Eventos enviados para fila
- Sucesso/falha no processamento
- Dados dos eventos

### **3. Métricas e Estatísticas**
- Total de logs por categoria
- Logs da última hora
- Última atividade
- Performance em tempo real

### **4. Filtros e Busca**
- Por tipo de log
- Por nível de severidade
- Por categoria
- Por período de tempo

## 🔄 Fluxo de Dados

```
[Captura de Rede] → [AlbionEventLogger] → [Interface Web]
         ↓
[Processamento de Eventos] → [AlbionEventLogger] → [Interface Web]
         ↓
[Envio para Fila] → [AlbionEventLogger] → [Interface Web]
```

## 💡 Benefícios

### **1. Separação de Responsabilidades**
- Logs do sistema vs. logs do Albion
- Console limpo para logs críticos
- Interface web para logs de negócio

### **2. Performance**
- Logs em memória com filas concorrentes
- Limpeza automática de logs antigos
- Sem impacto na captura de pacotes

### **3. Flexibilidade**
- Filtros por tipo e categoria
- Estatísticas em tempo real
- API para integração com frontend

### **4. Manutenibilidade**
- Sistema centralizado de logs
- Interface bem definida
- Fácil extensão para novos tipos de log

## 🚀 Próximos Passos

1. **Implementar Interface Web**
   - Dashboard em tempo real
   - Gráficos de métricas
   - Filtros avançados

2. **Persistência de Logs**
   - Banco de dados para histórico
   - Backup automático
   - Retenção configurável

3. **Alertas e Notificações**
   - Alertas de erro
   - Notificações de eventos importantes
   - Webhooks para sistemas externos

4. **Análise Avançada**
   - Machine Learning para detecção de anomalias
   - Relatórios automáticos
   - Exportação de dados

---

**Status**: ✅ Sistema implementado e integrado
**Próxima etapa**: Desenvolver interface web para consumir os logs
