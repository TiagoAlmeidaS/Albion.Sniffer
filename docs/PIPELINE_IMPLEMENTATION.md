# 🚀 Fase 4 - Pipeline Assíncrono - Implementação

## 📋 Visão Geral

Este documento detalha a implementação da **Fase 4 - Pipeline Assíncrono** do Albion.Sniffer, que implementa um sistema de processamento de eventos baseado em Channels com enrichers configuráveis e políticas de resiliência.

## 🎯 Objetivos

- **Evitar quedas** com oscilação de fila
- **Melhorar latência** através de processamento assíncrono
- **Implementar enrichers** configuráveis para transformação de eventos
- **Adicionar resiliência** com Polly retry policies e circuit breaker
- **Zero perda de eventos** mesmo sob pressão

## 🏗️ Arquitetura do Pipeline

```
[Captura UDP] → [Parser] → [EventDispatcher] → [Pipeline] → [Enrichers] → [Publishers]
                                    ↓
                            [Channel Buffer]
                                    ↓
                            [Worker Tasks]
                                    ↓
                            [Enricher Chain]
                                    ↓
                            [Retry/Circuit Breaker]
                                    ↓
                            [Queue Publishers]
```

## 🔧 Componentes Principais

### **1. EventPipeline (Core)**
- **Responsabilidade**: Orquestrar o fluxo de eventos através de Channels
- **Configuração**: Buffer size, worker count, backpressure handling
- **Métricas**: Throughput, latency, drop count

### **2. IEventEnricher (Chain)**
- **Responsabilidade**: Transformar/enriquecer eventos antes da publicação
- **Tipos**: TierColor, ProfileFilter, ProximityAlert, Composite
- **Configuração**: Via profiles e options

### **3. PipelineWorker (Background)**
- **Responsabilidade**: Processar eventos da fila de Channels
- **Configuração**: Worker count, cancellation, error handling
- **Métricas**: Processing time, error rate

### **4. Resilience Policies (Polly)**
- **Retry**: Tentativas automáticas em caso de falha
- **Circuit Breaker**: Abrir circuito em caso de falhas consecutivas
- **Timeout**: Timeout configurável para operações

## 📦 Implementação

### **Estrutura de Arquivos**
```
src/AlbionOnlineSniffer.Core/
├── Pipeline/
│   ├── IEventPipeline.cs
│   ├── EventPipeline.cs
│   ├── PipelineConfiguration.cs
│   └── PipelineMetrics.cs
├── Enrichers/
│   ├── IEventEnricher.cs
│   ├── TierColorEnricher.cs
│   ├── ProfileFilterEnricher.cs
│   ├── ProximityAlertEnricher.cs
│   └── CompositeEventEnricher.cs
└── Workers/
    ├── IPipelineWorker.cs
    └── PipelineWorker.cs
```

### **Dependências NuGet**
```xml
<PackageReference Include="Polly" Version="8.2.0" />
<PackageReference Include="Polly.Extensions.Http" Version="3.0.0" />
<PackageReference Include="Microsoft.Extensions.Hosting.Abstractions" Version="8.0.0" />
```

## ⚙️ Configuração

### **PipelineSettings**
```json
{
  "Pipeline": {
    "BufferSize": 10000,
    "WorkerCount": 4,
    "MaxConcurrency": 8,
    "RetryAttempts": 3,
    "RetryDelayMs": 1000,
    "CircuitBreakerThreshold": 5,
    "CircuitBreakerDurationMs": 30000,
    "TimeoutMs": 5000
  }
}
```

### **Enricher Configuration**
```json
{
  "Profiles": {
    "default": {
      "Enrichers": ["TierColor", "ProfileFilter"],
      "TierPalette": "classic"
    },
    "zvz": {
      "Enrichers": ["TierColor", "ProfileFilter", "ProximityAlert"],
      "TierPalette": "vibrant"
    }
  }
}
```

## 🔄 Fluxo de Processamento

### **1. Captura e Parsing**
```csharp
// EventDispatcher registra handler para pipeline
eventDispatcher.RegisterHandler("*", async (eventType, eventData) =>
{
    await _pipeline.EnqueueAsync(eventType, eventData);
});
```

### **2. Enqueue no Pipeline**
```csharp
public async Task EnqueueAsync(string eventType, object eventData)
{
    if (!_channel.Writer.TryWrite(new PipelineItem(eventType, eventData)))
    {
        // Buffer cheio - aplicar backpressure
        _metrics.IncrementDroppedEvents();
        await _channel.Writer.WriteAsync(new PipelineItem(eventType, eventData));
    }
}
```

### **3. Processamento por Workers**
```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    await foreach (var item in _channel.Reader.ReadAllAsync(stoppingToken))
    {
        try
        {
            await ProcessEventAsync(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing event {EventType}", item.EventType);
            _metrics.IncrementErrorCount();
        }
    }
}
```

### **4. Chain de Enrichers**
```csharp
public async Task<object> EnrichAsync(object eventData, ProfileOptions profile)
{
    var enriched = eventData;
    foreach (var enricher in _enrichers)
    {
                enriched = await enricher.EnrichAsync(enriched, profile);
    }
    return enriched;
}
```

### **5. Publicação com Resiliência**
```csharp
var policy = Policy
    .Handle<Exception>()
    .WaitAndRetryAsync(3, retryAttempt => 
        TimeSpan.FromMilliseconds(1000 * Math.Pow(2, retryAttempt)))
    .WrapAsync(Policy.TimeoutAsync(5000));

await policy.ExecuteAsync(async () =>
{
    await _publisher.PublishAsync(topic, enrichedEvent);
});
```

## 📊 Métricas e Monitoramento

### **PipelineMetrics**
- **Throughput**: Eventos/segundo processados
- **Latency**: Tempo médio de processamento
- **Buffer Usage**: Uso atual do buffer
- **Drop Count**: Eventos descartados por buffer cheio
- **Error Rate**: Taxa de erros de processamento

### **Health Checks**
- **Pipeline Health**: Status do pipeline e workers
- **Buffer Health**: Uso do buffer (warning se >80%)
- **Worker Health**: Status dos workers ativos

## 🧪 Testes

### **Testes Unitários**
- **EventPipeline**: Testar enqueue/dequeue, backpressure
- **Enrichers**: Testar transformações individuais
- **PipelineWorker**: Testar processamento e error handling
- **Resilience**: Testar retry policies e circuit breaker

### **Testes de Integração**
- **Pipeline End-to-End**: Captura → Pipeline → Publicação
- **Performance**: Throughput sob carga
- **Resilience**: Comportamento com falhas simuladas

## 🚀 Critérios de Aceite

- [ ] Pipeline processa eventos sem perda (buffer configurável)
- [ ] Enrichers aplicam transformações conforme profile
- [ ] Retry policies funcionam em caso de falha
- [ ] Circuit breaker abre em caso de falhas consecutivas
- [ ] Métricas expostas para monitoramento
- [ ] Health checks reportam status do pipeline
- [ ] Performance: throughput >1000 eventos/segundo
- [ ] Latência: <100ms p99 para processamento

## 📝 Próximos Passos

1. **Implementar IEventPipeline e EventPipeline**
2. **Criar sistema de Enrichers**
3. **Implementar PipelineWorker**
4. **Adicionar políticas de resiliência (Polly)**
5. **Integrar com EventDispatcher existente**
6. **Adicionar métricas e health checks**
7. **Testes unitários e de integração**
8. **Documentação de uso**

## 🔗 Referências

- [Channels](https://docs.microsoft.com/en-us/dotnet/core/extensions/channels) - Sistema de filas assíncronas
- [Polly](https://github.com/App-vNext/Polly) - Biblioteca de resiliência
- [Background Services](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services) - Serviços em background
