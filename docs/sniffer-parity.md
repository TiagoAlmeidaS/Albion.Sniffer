# Albion Sniffer - Paridade App Console ↔ Web Blazor

## 📊 Matriz de Funcionalidades

| Módulo/Service | App Console | Web Blazor | Status | Observações |
|----------------|-------------|------------|---------|-------------|
| **Captura (SharpPcap)** | ✅ Captura UDP:5050 | ❌ Não captura | ✅ Implementado | Web consome da fila |
| **Parser (Protocol16Deserializer)** | ✅ Decodifica protocol 16 | ⚠️ Opcional | ✅ Implementado | Web pode ter parser minimal |
| **PacketProcessor** | ✅ Normaliza pacotes | ⚠️ Aplica normalização | 🔄 Pendente | Implementar normalização |
| **Publisher (RabbitMQ/Redis)** | ✅ Publica eventos | ❌ Apenas consome | 🔄 Pendente | Implementar consumer |
| **Logs** | ✅ Serilog/Loki | ⚠️ Lista in-memory | 🔄 Pendente | Implementar buffer limitado |
| **Persistência (DB)** | ✅ EF Core (DB real) | ❌ InMemoryRepository | 🔄 Pendente | Substituir por repositórios em memória |
| **UI** | ✅ CLI | ⚠️ Blazor Server | 🔄 Pendente | Implementar páginas: /queue, /packets, /logs, /sessions |
| **Replay** | ❌ N/A | ⚠️ Opcional | 🔄 Pendente | Re-publicar para exchange de replay |

## 🎯 Objetivo

Mapear funcionalidades App ↔ Web, implementar paridade mínima, mas com armazenamento somente em memória para pacotes, eventos, logs e mensagens da fila.
O foco é visualização em tempo real (live viewer), sem persistência em disco.

## 🏗️ Arquitetura Web (com InMemory)

```
[ RabbitMQ/Redis ] --> [ QueueConsumer (BackgroundService) ] 
                           -> [Normalizer/Parser] 
                           -> [InMemoryRepositories] 
                           -> [SignalR Hub] --> [Blazor UI]
                                             \-> [Prometheus Metrics]
```

## 📦 Repositórios em Memória

### Interface Base
```csharp
public interface IInMemoryRepository<T>
{
    void Add(T item);
    IReadOnlyList<T> GetAll();
    int Count { get; }
    void Clear();
}
```

### Implementação com Limite FIFO
```csharp
public class BoundedInMemoryRepository<T> : IInMemoryRepository<T>
{
    private readonly int _maxItems;
    private readonly LinkedList<T> _items = new();
    private readonly object _lock = new();

    public BoundedInMemoryRepository(int maxItems = 5000)
    {
        _maxItems = maxItems;
    }

    public void Add(T item)
    {
        lock (_lock)
        {
            if (_items.Count >= _maxItems)
                _items.RemoveFirst();
            _items.AddLast(item);
        }
    }

    public IReadOnlyList<T> GetAll()
    {
        lock (_lock)
        {
            return _items.ToList();
        }
    }

    public int Count => _items.Count;
    
    public void Clear()
    {
        lock (_lock)
        {
            _items.Clear();
        }
    }
}
```

## 🔄 Queue Consumer

### BackgroundService para Consumo
```csharp
public class QueueConsumerService : BackgroundService
{
    private readonly IInMemoryRepository<Packet> _packets;
    private readonly IInMemoryRepository<Event> _events;
    private readonly IInMemoryRepository<LogEntry> _logs;
    private readonly IHubContext<SnifferHub> _hub;
    private readonly ILogger<QueueConsumerService> _logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Consumir da fila e distribuir para repositórios
        // Emitir via SignalR
    }
}
```

## 🎨 Blazor + SignalR UI

### Páginas Principais
- **/queue** → Stream de mensagens da fila
- **/packets** → Lista de pacotes interceptados
- **/logs** → Console em tempo real
- **/sessions** → Agrupar por correlation/session id
- **HexViewer** → Para payloads binários

### Componentes
- **PacketList** → Lista paginada de pacotes
- **EventViewer** → Visualizador de eventos
- **LogConsole** → Console de logs em tempo real
- **HexViewer** → Visualizador hexadecimal
- **MetricsDashboard** → Dashboard de métricas

## 📊 Observabilidade

### Health Checks
- **/healthz** → Status geral da aplicação
- **/healthz/queue** → Status da conexão com fila
- **/healthz/repositories** → Status dos repositórios

### Métricas Prometheus
- **/metrics** → Métricas em formato Prometheus
- Counters: mensagens recebidas, descartadas (limite FIFO)
- Gauges: tamanho dos repositórios
- Histograms: latência de processamento

## 🚀 Critérios de Aceite

- [ ] Dados são exibidos em tempo real na UI
- [ ] Histórico é limitado (ex.: últimos 10k eventos), sem uso de DB
- [ ] Mensagens aparecem na UI em <250ms p99 após chegar na fila
- [ ] HexViewer funcional para payloads binários
- [ ] Métricas e health checks disponíveis
- [ ] Filtros básicos implementados
- [ ] Agrupamento por sessão/correlation ID

## 📝 Próximos Passos

1. **Implementar repositórios em memória**
2. **Criar QueueConsumerService**
3. **Implementar páginas Blazor**
4. **Adicionar health checks e métricas**
5. **Testes de integração**
6. **Documentação de uso**

## 🔧 Configuração

### appsettings.json
```json
{
  "InMemoryRepositories": {
    "MaxPackets": 10000,
    "MaxEvents": 10000,
    "MaxLogs": 5000
  },
  "Queue": {
    "Type": "RabbitMQ", // ou "Redis"
    "ConnectionString": "amqp://localhost:5672",
    "QueueName": "albion-sniffer"
  }
}
```

## 📚 Referências

- [Albion.Network](https://github.com/ao-org/albion-online-data-project) - Framework base para eventos
- [SignalR](https://docs.microsoft.com/en-us/aspnet/core/signalr) - Comunicação em tempo real
- [Blazor Server](https://docs.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/server) - UI framework