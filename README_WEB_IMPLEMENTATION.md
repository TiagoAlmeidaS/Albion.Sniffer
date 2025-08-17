# Albion Sniffer Web - Implementação com Repositórios In-Memory

## 🎯 Visão Geral

Esta implementação transforma o Albion Sniffer em um **live viewer web** com armazenamento totalmente em memória, eliminando a necessidade de banco de dados e fornecendo visualização em tempo real dos dados capturados.

## 🏗️ Arquitetura

```
[ Albion Online ] → [ SharpPcap ] → [ Protocol16Deserializer ] → [ EventDispatcher ]
                                                                    ↓
[ SignalR Hub ] ← [ InboundMessageService ] ← [ InMemoryRepositories ] ← [ EventStreamService ]
     ↓
[ Blazor UI ] ← [ Health Checks ] ← [ Metrics Service ]
```

## 📦 Componentes Principais

### 1. Repositórios em Memória
- **`BoundedInMemoryRepository<T>`**: Repositório com limite configurável e comportamento FIFO
- **`IInMemoryRepository<T>`**: Interface base para todos os repositórios
- **Configuração**: Limites configuráveis via `appsettings.json`

### 2. Modelos de Dados
- **`Packet`**: Pacotes UDP capturados com metadados
- **`Event`**: Eventos de jogo parseados
- **`LogEntry`**: Entradas de log com níveis e categorias
- **`Session`**: Agrupamento de eventos relacionados

### 3. Serviços
- **`InboundMessageService`**: Processa mensagens recebidas
- **`SessionManager`**: Gerencia sessões de eventos
- **`MetricsService`**: Coleta métricas de performance
- **`HealthCheckService`**: Verifica saúde da aplicação

### 4. SignalR Hub
- **`SnifferHub`**: Comunicação em tempo real com clientes
- **Métodos**: `GetPackets`, `GetEvents`, `GetLogs`, `GetSessions`
- **Eventos**: `packet:new`, `gameEvent:new`, `log:new`

## 🚀 Como Executar

### 1. Pré-requisitos
```bash
# .NET 8.0 SDK
dotnet --version  # Deve ser 8.0 ou superior

# Dependências do projeto
dotnet restore
```

### 2. Configuração
Edite `appsettings.json`:
```json
{
  "InMemoryRepositories": {
    "MaxPackets": 10000,      // Máximo de pacotes em memória
    "MaxEvents": 10000,       // Máximo de eventos em memória
    "MaxLogs": 5000,          // Máximo de logs em memória
    "MaxSessions": 1000       // Máximo de sessões em memória
  }
}
```

### 3. Execução
```bash
cd src/AlbionOnlineSniffer.Web
dotnet run
```

### 4. Acesso
- **Web UI**: http://localhost:5000
- **Health Check**: http://localhost:5000/healthz
- **Métricas Prometheus**: http://localhost:5000/metrics
- **API**: http://localhost:5000/api/*

## 📊 Endpoints Disponíveis

### Health Checks
- `GET /healthz` - Status geral da aplicação
- `GET /healthz/repositories` - Saúde dos repositórios
- `GET /healthz/memory` - Uso de memória
- `GET /healthz/performance` - Performance e latência

### Métricas
- `GET /metrics` - Formato Prometheus
- `GET /api/metrics` - Formato JSON

### Dados
- `GET /api/packets?skip=0&take=100` - Pacotes paginados
- `GET /api/events?skip=0&take=100` - Eventos paginados
- `GET /api/logs?skip=0&take=100` - Logs paginados
- `GET /api/sessions?skip=0&take=100` - Sessões paginadas

### Controle
- `POST /api/repositories/clear` - Limpa todos os repositórios
- `POST /api/metrics/reset` - Reseta métricas

## 🎨 Interface Web

### Páginas Principais
1. **Dashboard**: Visão geral com métricas e status
2. **Packets**: Lista de pacotes capturados com hex viewer
3. **Events**: Eventos de jogo parseados
4. **Logs**: Console de logs em tempo real
5. **Sessions**: Agrupamento por sessão/correlation ID

### Componentes
- **HexViewer**: Visualizador hexadecimal para payloads binários
- **PacketList**: Lista paginada de pacotes com filtros
- **EventViewer**: Visualizador de eventos com detalhes
- **LogConsole**: Console de logs com níveis de severidade
- **MetricsDashboard**: Dashboard de métricas em tempo real

## 📈 Métricas e Monitoramento

### Métricas Coletadas
- **Contadores**: Pacotes recebidos, eventos processados, logs criados
- **Gauges**: Uso de memória, tamanho dos repositórios
- **Histogramas**: Latência de processamento (P50, P95, P99)
- **Taxas**: Pacotes/segundo, eventos/segundo

### Health Checks
- **Repositories**: Verifica funcionamento dos repositórios
- **Memory**: Monitora uso de memória
- **Performance**: Verifica latências e taxas
- **Error Rate**: Monitora taxa de erros

## 🔧 Configuração Avançada

### Tamanhos dos Repositórios
```json
{
  "InMemoryRepositories": {
    "MaxPackets": 10000,    // Para capturas intensivas
    "MaxEvents": 10000,     // Para muitos eventos
    "MaxLogs": 5000,        // Para logs detalhados
    "MaxSessions": 1000     // Para muitas sessões
  }
}
```

### Logging
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "AlbionOnlineSniffer.Web": "Debug"
    }
  }
}
```

### Performance
```json
{
  "HealthChecks": {
    "Interval": "00:00:30",    // Verificação a cada 30s
    "Timeout": "00:00:10"      // Timeout de 10s
  }
}
```

## 🧪 Testes

### Testes de Performance
```bash
# Teste de carga com ab (Apache Bench)
ab -n 1000 -c 10 http://localhost:5000/api/metrics

# Teste de latência
curl -w "@curl-format.txt" -o /dev/null -s http://localhost:5000/healthz
```

### Testes de Funcionalidade
```bash
# Verificar health
curl http://localhost:5000/healthz

# Verificar métricas
curl http://localhost:5000/metrics

# Verificar dados
curl http://localhost:5000/api/packets?take=10
```

## 🚨 Troubleshooting

### Problemas Comuns

#### 1. Erro de Memória
```
System.OutOfMemoryException
```
**Solução**: Reduza os limites dos repositórios em `appsettings.json`

#### 2. Latência Alta
```
packet_processing_latency_p99 > 250ms
```
**Solução**: Verifique se há gargalos no processamento ou muitos dados

#### 3. Repositórios Vazios
```
packets_current: 0, packets_total: 0
```
**Solução**: Verifique se a captura está funcionando e se os eventos estão sendo processados

### Logs de Debug
```bash
# Habilitar logs detalhados
export Logging__LogLevel__AlbionOnlineSniffer.Web=Debug
dotnet run
```

## 🔮 Próximos Passos

### 1. Implementações Pendentes
- [ ] Queue Consumer para RabbitMQ/Redis
- [ ] Páginas Blazor completas
- [ ] Filtros avançados
- [ ] Exportação de dados

### 2. Melhorias Futuras
- [ ] Cache distribuído (Redis)
- [ ] Compressão de dados
- [ ] Análise de padrões
- [ ] Alertas automáticos

### 3. Integrações
- [ ] Grafana para dashboards
- [ ] Prometheus para métricas
- [ ] Elasticsearch para logs
- [ ] Kibana para visualização

## 📚 Referências

- [SignalR Documentation](https://docs.microsoft.com/en-us/aspnet/core/signalr)
- [Blazor Server](https://docs.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/server)
- [Health Checks](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
- [Prometheus Metrics](https://prometheus.io/docs/concepts/metric_types/)

## 🤝 Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature
3. Implemente as mudanças
4. Adicione testes
5. Submeta um Pull Request

## 📄 Licença

Este projeto está sob a mesma licença do projeto principal Albion Sniffer.