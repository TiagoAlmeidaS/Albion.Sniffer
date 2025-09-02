# 🔍 Discovery Service - Dashboard Web

## 📊 Visão Geral

O **Discovery Service Dashboard Web** é uma interface moderna e em tempo real para monitorar e analisar os pacotes descobertos pelo sniffer do Albion Online. O dashboard oferece visualizações interativas, estatísticas detalhadas e atualizações automáticas.

## 🎯 Funcionalidades

### 📈 **Estatísticas em Tempo Real**
- **Total de pacotes** processados
- **Pacotes visíveis** vs **escondidos** com percentuais
- **Tempo de execução** do serviço
- **Última atualização** com timestamp

### 🏆 **Top 10 Pacotes Mais Frequentes**
- **Código** do pacote
- **Tipo** do pacote
- **Contagem** de ocorrências
- **Status** (visível/escondido)
- **Última ocorrência** (timestamp)

### 📊 **Top 5 Tipos de Pacotes**
- **Tipo** do pacote
- **Contagem** total por tipo

## 🚀 Como Usar

### 1. **Acessar o Dashboard**
```
http://localhost:5000/discovery.html
```

### 2. **Navegação**
- **Dashboard Principal**: `http://localhost:5000/` - Visão geral do sniffer
- **Discovery Dashboard**: `http://localhost:5000/discovery.html` - Estatísticas do Discovery

### 3. **Atualizações Automáticas**
- As estatísticas são atualizadas **automaticamente a cada 2 segundos**
- Botão **"Atualizar"** para refresh manual
- Interface responsiva para desktop e mobile

## 🔧 **API Endpoints**

### **Estatísticas Gerais**
```http
GET /api/discovery/stats
```
**Resposta:**
```json
{
  "timestamp": "2024-01-15T14:30:45.123Z",
  "totalPackets": 1247,
  "visiblePackets": 892,
  "hiddenPackets": 355,
  "visiblePercentage": 71.5,
  "hiddenPercentage": 28.5,
  "uptime": "00:05:23",
  "topPackets": [...],
  "topTypes": [...]
}
```

### **Top Pacotes**
```http
GET /api/discovery/top-packets?limit=10
```
**Resposta:**
```json
[
  {
    "code": 123,
    "type": "MoveEvent",
    "count": 156,
    "isHidden": false,
    "lastSeen": "2024-01-15T14:30:45.123Z"
  }
]
```

### **Top Tipos**
```http
GET /api/discovery/top-types?limit=5
```
**Resposta:**
```json
[
  {
    "type": "MoveEvent",
    "count": 156
  }
]
```

## 🎨 **Interface do Dashboard**

### **Cabeçalho**
- **Logo** do Discovery Service
- **Links de navegação** entre Dashboard e Discovery
- **Status** de conexão em tempo real

### **Cards de Estatísticas**
- **4 cards principais** com métricas essenciais
- **Ícones coloridos** para identificação rápida
- **Valores formatados** (K, M para números grandes)
- **Percentuais** calculados automaticamente

### **Tabela de Top Pacotes**
- **Tabela responsiva** com scroll horizontal
- **Badges coloridos** para status (visível/escondido)
- **Formatação de tempo** legível
- **Ordenação** por frequência

### **Lista de Top Tipos**
- **Lista compacta** com contadores
- **Design limpo** e fácil de ler
- **Atualização** em tempo real

## 🔄 **Arquitetura Técnica**

### **Backend**
- **`DiscoveryStatistics`**: Coleta e processa estatísticas
- **`DiscoveryWebStatisticsService`**: Expõe dados para web
- **API REST**: Endpoints para dados JSON
- **Cache em memória**: Performance otimizada

### **Frontend**
- **HTML5 + CSS3**: Interface moderna
- **JavaScript Vanilla**: Sem dependências externas
- **Fetch API**: Comunicação com backend
- **Auto-refresh**: Atualizações automáticas

### **Fluxo de Dados**
```
DiscoveryService → DiscoveryStatistics → DiscoveryWebStatisticsService → API → Dashboard
```

## 🛠️ **Configuração**

### **Pacotes Escondidos**
Os pacotes escondidos são definidos no `DiscoveryService.cs`:
```csharp
List<int> pacotesEscondidos = [
    1, 11, 19, 2, 3, 6, 21, 29, 35, 39, 40, 46, 47, 90, 91, 123, 209, 280, 319, 246, 359, 387, 514, 525, 526, 593
];
```

### **Intervalo de Atualização**
- **Backend**: 2 segundos (configurável)
- **Frontend**: 2 segundos (sincronizado)
- **Console**: 5 segundos (separado)

## 🎯 **Cenários de Uso**

### **1. Monitoramento de Descoberta**
- Identificar novos tipos de pacotes
- Monitorar frequência de pacotes específicos
- Detectar anomalias no tráfego

### **2. Análise de Performance**
- Verificar taxa de pacotes visíveis vs escondidos
- Monitorar tempo de execução
- Analisar padrões de tráfego

### **3. Debugging e Troubleshooting**
- Identificar pacotes problemáticos
- Verificar se filtros estão funcionando
- Monitorar saúde do sistema

### **4. Pesquisa e Desenvolvimento**
- Descobrir novos eventos do jogo
- Analisar protocolo de comunicação
- Desenvolver novos handlers

## 📱 **Responsividade**

### **Desktop**
- **Layout em grid** com 2 colunas
- **Tabelas completas** com todas as colunas
- **Hover effects** e animações

### **Mobile**
- **Layout em coluna única**
- **Tabelas com scroll** horizontal
- **Touch-friendly** interface

## 🚨 **Troubleshooting**

### **Dashboard não carrega**
- Verifique se o projeto Web está rodando
- Confirme se as APIs estão respondendo
- Verifique o console do navegador

### **Dados não atualizam**
- Verifique se o `DiscoveryService` está ativo
- Confirme se há pacotes sendo processados
- Verifique logs do backend

### **Performance lenta**
- Reduza o intervalo de atualização
- Limite o número de pacotes exibidos
- Verifique recursos do sistema

## 🔮 **Futuras Melhorias**

### **Funcionalidades Planejadas**
- **Filtros avançados** por tipo/código
- **Gráficos em tempo real** (Chart.js)
- **Exportação de dados** (CSV/JSON)
- **Alertas configuráveis**
- **Histórico de estatísticas**

### **Integrações**
- **SignalR** para updates em tempo real
- **WebSockets** para streaming
- **Prometheus** para métricas
- **Grafana** para dashboards avançados

## 📝 **Logs e Monitoramento**

### **Logs do Backend**
- **DiscoveryService**: Logs de debug (silenciosos)
- **DiscoveryWebStatisticsService**: Logs de inicialização
- **API**: Logs de requisições

### **Logs do Frontend**
- **Console**: Erros e warnings
- **Network**: Requisições HTTP
- **Performance**: Métricas de carregamento

## 🎉 **Conclusão**

O **Discovery Service Dashboard Web** oferece uma interface moderna e intuitiva para monitorar o sistema de descoberta de pacotes do Albion Online. Com atualizações em tempo real, visualizações claras e arquitetura robusta, é uma ferramenta essencial para desenvolvedores e pesquisadores.

---

**Desenvolvido com ❤️ para a comunidade Albion Online**
