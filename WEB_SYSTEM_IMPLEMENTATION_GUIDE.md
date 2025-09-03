# Guia de Implementação do Sistema Web - Albion.Sniffer

## Visão Geral

Este documento descreve a implementação completa do sistema web para análise geral de dados interceptados (UDP e eventos de discovery) no Albion.Sniffer, conforme especificado na documentação fornecida.

## Componentes Implementados

### 1. Backend - Serviços e Controllers

#### UDPEventIntegrationService (Melhorado)
- **Localização**: `src/AlbionOnlineSniffer.Core/Services/UDPEventIntegrationService.cs`
- **Funcionalidades**:
  - Extração detalhada de informações de eventos (opcode, parâmetros, timestamp)
  - Logs detalhados para depuração
  - Categorização automática de eventos
  - Integração com EventDispatcher

#### EventFilterService (Expandido)
- **Localização**: `src/AlbionOnlineSniffer.Web/Services/EventFilterService.cs`
- **Funcionalidades**:
  - Filtros avançados por range de opcode
  - Busca por termo em payloads
  - Estatísticas por categoria
  - Distribuição de opcodes
  - Métricas de performance

#### EventsController (Novo)
- **Localização**: `src/AlbionOnlineSniffer.Web/Controllers/EventsController.cs`
- **Endpoints**:
  - `GET /api/events/filters` - Configuração de filtros
  - `POST /api/events/filters` - Atualizar filtros
  - `GET /api/events/stats/{type}` - Estatísticas por tipo
  - `GET /api/events/advanced-stats/{type}` - Estatísticas avançadas
  - `GET /api/events/opcode-distribution/{type}` - Distribuição de opcodes
  - `GET /api/events/performance` - Métricas de performance
  - `POST /api/events/test-event` - Teste de eventos
  - `GET /api/events/data-status` - Status de preenchimento de dados
  - `GET /api/events/debug` - Informações de debug

#### DebugService (Novo)
- **Localização**: `src/AlbionOnlineSniffer.Web/Services/DebugService.cs`
- **Funcionalidades**:
  - Monitoramento de dados não preenchidos
  - Logs detalhados de eventos
  - Estatísticas de debug
  - Recomendações automáticas

### 2. Frontend - Dashboard Melhorado

#### events.html (Atualizado)
- **Localização**: `src/AlbionOnlineSniffer.Web/wwwroot/events.html`
- **Funcionalidades**:
  - Filtros avançados (opcode range, busca por termo)
  - Visualização de payloads detalhados
  - Gráficos de distribuição de opcodes (Chart.js)
  - Exportação de dados (CSV)
  - Modal para detalhes de eventos
  - Estatísticas avançadas
  - Interface responsiva e moderna

## Como Usar

### 1. Inicialização

```bash
# Navegar para o diretório do projeto
cd src/AlbionOnlineSniffer.Web

# Executar o projeto
dotnet run
```

### 2. Acesso ao Dashboard

- **URL**: `http://localhost:5000/events.html`
- **Funcionalidades disponíveis**:
  - Visualização em tempo real de eventos UDP e Discovery
  - Filtros por tipo, categoria, opcode range
  - Gráficos de distribuição de opcodes
  - Exportação de dados

### 3. Teste de Integração

#### Teste de Evento Simulado
```bash
curl -X POST http://localhost:5000/api/events/test-event \
  -H "Content-Type: application/json" \
  -d '{
    "eventType": "TestEvent",
    "opcode": "354",
    "paramCount": 3,
    "category": "Test"
  }'
```

#### Verificação de Status de Dados
```bash
curl http://localhost:5000/api/events/data-status
```

### 4. Filtros Avançados

#### Por Range de Opcode
- **UDP**: Filtra eventos por range de opcode (ex: 100-500)
- **Discovery**: Filtra packets por range de opcode

#### Por Termo de Busca
- Busca em nomes de eventos
- Busca em payloads
- Busca em tipos de eventos

#### Por Categoria
- Fishing, Dungeons, Harvestables, Mobs, Players, Loot, System

### 5. Visualizações

#### Gráfico de Distribuição de Opcodes
- Mostra os 20 opcodes mais frequentes
- Alterna entre UDP e Discovery
- Atualização automática a cada 3 segundos

#### Estatísticas em Tempo Real
- Contadores de eventos/packets
- Taxa de sucesso/erro
- Métricas de performance

## Depuração de Dados Não Preenchidos

### 1. Logs Detalhados

O sistema agora inclui logs detalhados que mostram:
- Opcode extraído do evento
- Número de parâmetros
- Status de preenchimento
- Categoria detectada

### 2. Endpoints de Debug

- `GET /api/events/debug` - Informações completas do sistema
- `GET /api/events/data-status` - Status de preenchimento com recomendações

### 3. Recomendações Automáticas

O sistema gera recomendações automáticas baseadas no status dos dados:
- Se nenhum dado é detectado
- Se apenas UDP ou Discovery está funcionando
- Se dados estão sendo capturados corretamente

## Estrutura de Dados

### Evento UDP
```json
{
  "eventType": "PlayerMoveEvent",
  "eventName": "PlayerMoveEvent",
  "opcode": "354",
  "category": "Players",
  "count": 150,
  "isSuccessful": true,
  "lastSeen": "2024-01-15T10:30:00Z",
  "details": {
    "Opcode": "354",
    "ParamCount": 3,
    "EventName": "PlayerMoveEvent",
    "Timestamp": "2024-01-15T10:30:00Z",
    "HasParameters": true
  }
}
```

### Packet Discovery
```json
{
  "code": "354",
  "type": "PlayerMove",
  "count": 75,
  "isHidden": false,
  "lastSeen": "2024-01-15T10:30:00Z",
  "category": "Players"
}
```

## Melhorias Implementadas

### 1. Análise Geral (Não Específica)
- Sistema focado em análise geral de tráfego
- Categorização automática para facilitar interpretação
- Suporte a todos os tipos de eventos, não apenas pesca

### 2. Filtros Avançados
- Range de opcodes
- Busca por termo
- Filtros por categoria
- Contagem mínima

### 3. Visualizações
- Gráficos de distribuição
- Estatísticas em tempo real
- Métricas de performance

### 4. Depuração
- Logs detalhados
- Monitoramento de dados vazios
- Recomendações automáticas
- Endpoints de teste

### 5. Exportação
- Dados em CSV
- Filtros aplicados
- Timestamp de exportação

## Próximos Passos

### Melhorias Futuras
1. **Persistência**: Integrar SQLite com EF Core para histórico
2. **WebSockets**: Substituir polling por SignalR
3. **Segurança**: Adicionar autenticação JWT
4. **Integração Externa**: Exportar para ElasticSearch/Grafana

### Monitoramento
- Verificar logs do console para eventos registrados
- Usar endpoint `/api/events/data-status` para diagnóstico
- Testar com eventos simulados via `/api/events/test-event`

## Conclusão

O sistema web foi implementado com sucesso, fornecendo:
- ✅ Análise geral de dados interceptados
- ✅ Filtros avançados e visualizações
- ✅ Depuração de dados não preenchidos
- ✅ Interface moderna e responsiva
- ✅ Logs detalhados para troubleshooting
- ✅ Exportação de dados
- ✅ Gráficos analíticos

O sistema está pronto para uso e pode ser facilmente expandido conforme necessário.