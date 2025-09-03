# 🎯 Sistema de Categorização e Filtros de Eventos

## 📊 Visão Geral

O **Sistema de Categorização e Filtros de Eventos** é uma solução avançada para organizar, filtrar e analisar eventos do Albion Sniffer. O sistema oferece categorização automática, filtros dinâmicos e uma interface web moderna para monitoramento em tempo real.

## 🏗️ Arquitetura do Sistema

### 📂 **Componentes Principais**

1. **EventCategoryService** - Gerencia categorias e filtros
2. **UDPStatistics** - Estatísticas em tempo real para eventos UDP
3. **EventFilterService** - Serviço web para gerenciar filtros
4. **Event Dashboard** - Interface web com filtros dinâmicos

### 🎨 **Categorias de Eventos**

#### 🎣 **Fishing (Pesca)**
- **Eventos:** StartFishingEvent, FishingBiteEvent, FishingFinishEvent, FishingMiniGameUpdateEvent, NewFishingZoneEvent
- **Cor:** #06b6d4 (Ciano)
- **Ícone:** 🎣

#### 🏰 **Dungeons (Dungeons)**
- **Eventos:** NewDungeonEvent, WispGateOpenedEvent, NewGatedWispEvent
- **Cor:** #8b5cf6 (Roxo)
- **Ícone:** 🏰

#### 🌿 **Harvestables (Recursos)**
- **Eventos:** NewHarvestableEvent, NewHarvestablesListEvent, HarvestableChangeStateEvent
- **Cor:** #10b981 (Verde)
- **Ícone:** 🌿

#### 👹 **Mobs (Criaturas)**
- **Eventos:** NewMobEvent, MobChangeStateEvent
- **Cor:** #ef4444 (Vermelho)
- **Ícone:** 👹

#### 👤 **Players (Jogadores)**
- **Eventos:** NewCharacterEvent, MoveEvent, LeaveEvent, MountedEvent, HealthUpdateEvent, CharacterEquipmentChangedEvent, RegenerationChangedEvent, MistsPlayerJoinedInfoEvent
- **Cor:** #3b82f6 (Azul)
- **Ícone:** 👤

#### 💰 **Loot (Tesouros)**
- **Eventos:** NewLootChestEvent
- **Cor:** #f59e0b (Amarelo)
- **Ícone:** 💰

#### 🔑 **System (Sistema)**
- **Eventos:** KeySyncEvent, LoadClusterObjectsEvent, ChangeFlaggingFinishedEvent
- **Cor:** #6b7280 (Cinza)
- **Ícone:** 🔑

#### 🔍 **Discovery (Descoberta)**
- **Eventos:** ResponsePacket, RequestPacket, EventPacket
- **Cor:** #ec4899 (Rosa)
- **Ícone:** 🔍

## 🚀 Como Usar

### 1. **Acessar o Dashboard de Eventos**
```
http://localhost:5000/events.html
```

### 2. **Aplicar Filtros**

#### **Filtros Disponíveis:**
- **Tipo de Evento:** Discovery ou UDP
- **Categoria:** Selecionar categoria específica
- **Contagem Mínima:** Filtrar por número mínimo de ocorrências
- **Busca:** Pesquisar por nome do evento
- **Visibilidade:** Mostrar/ocultar pacotes escondidos/visíveis

#### **Exemplo de Uso:**
```javascript
// Filtrar apenas eventos de pesca com mais de 10 ocorrências
{
    type: "udp",
    category: "fishing",
    minCount: 10,
    showHidden: false,
    showVisible: true
}
```

### 3. **API Endpoints**

#### **Obter Categorias**
```http
GET /api/events/categories
```

#### **Obter Categorias por Tipo**
```http
GET /api/events/categories/{type}
```

#### **Obter Configuração de Filtros**
```http
GET /api/events/filters
```

#### **Atualizar Filtros**
```http
POST /api/events/filters
Content-Type: application/json

{
    "enabledCategories": ["fishing", "mobs"],
    "disabledEventNames": ["NewCharacterEvent"],
    "disabledPacketCodes": [1, 2, 3],
    "showHiddenPackets": false,
    "showVisiblePackets": true,
    "minPacketCount": 5,
    "searchTerm": "fishing"
}
```

#### **Obter Estatísticas Filtradas**
```http
GET /api/events/stats/{type}?category=fishing&showHidden=false&minCount=10
```

## 📈 **Funcionalidades Avançadas**

### 🔍 **Filtros Dinâmicos**
- **Filtros em Tempo Real:** Aplicação instantânea de filtros
- **Filtros Combinados:** Múltiplos critérios simultâneos
- **Filtros Persistentes:** Configurações salvas entre sessões

### 📊 **Estatísticas em Tempo Real**
- **Discovery Stats:** Pacotes visíveis vs escondidos
- **UDP Stats:** Eventos bem-sucedidos vs falhas
- **Atualizações Automáticas:** Refresh a cada 3 segundos

### 🎨 **Interface Moderna**
- **Design Responsivo:** Funciona em desktop e mobile
- **Tema Escuro:** Interface otimizada para monitoramento
- **Navegação Intuitiva:** Links entre dashboards

## 🔧 **Configuração Avançada**

### **Personalizar Categorias**
```csharp
// Adicionar nova categoria
var newCategory = new EventCategory
{
    Id = "custom",
    Name = "Custom Events",
    Description = "Eventos personalizados",
    Icon = "⭐",
    Color = "#ff6b6b",
    Type = EventType.UDP,
    EventNames = new List<string> { "CustomEvent1", "CustomEvent2" },
    Priority = 9
};
```

### **Configurar Filtros Programaticamente**
```csharp
var filterConfig = new EventFilterConfig
{
    EnabledCategories = new List<string> { "fishing", "mobs" },
    DisabledEventNames = new List<string> { "NewCharacterEvent" },
    ShowHiddenPackets = false,
    MinPacketCount = 5
};

eventCategoryService.UpdateFilterConfig(filterConfig);
```

## 📱 **Interface Web**

### **Dashboard de Eventos**
- **Painel de Filtros:** Controles intuitivos para aplicar filtros
- **Estatísticas em Tempo Real:** Cards com métricas atualizadas
- **Tabela de Eventos:** Lista filtrada com informações detalhadas
- **Navegação:** Links para outros dashboards

### **Recursos da Interface**
- **Filtros Visuais:** Chips coloridos para categorias
- **Status Badges:** Indicadores visuais de status
- **Atualizações Automáticas:** Refresh automático dos dados
- **Responsividade:** Interface adaptável a diferentes telas

## 🎯 **Casos de Uso**

### **1. Análise de Pesca**
```javascript
// Filtrar apenas eventos de pesca
applyFilters({
    type: "udp",
    category: "fishing",
    showVisible: true,
    showHidden: false
});
```

### **2. Monitoramento de Mobs**
```javascript
// Monitorar apenas eventos de mobs
applyFilters({
    type: "udp",
    category: "mobs",
    minCount: 1
});
```

### **3. Análise de Discovery**
```javascript
// Analisar pacotes descobertos
applyFilters({
    type: "discovery",
    showHidden: true,
    showVisible: true
});
```

## 🔍 **Troubleshooting**

### **Problemas Comuns**

#### **Filtros não funcionam**
- Verificar se os serviços estão registrados no DI container
- Confirmar que as APIs estão respondendo
- Verificar logs do navegador para erros JavaScript

#### **Categorias não aparecem**
- Verificar se o `EventCategoryService` está inicializado
- Confirmar que as categorias estão sendo carregadas via API
- Verificar se há erros de CORS

#### **Estatísticas não atualizam**
- Verificar se os serviços de estatísticas estão rodando
- Confirmar que os timers estão configurados corretamente
- Verificar se há erros nos logs do servidor

## 📚 **Referências**

- **Discovery Statistics Guide:** `docs/DISCOVERY_STATISTICS_GUIDE.md`
- **Web Dashboard Guide:** `docs/DISCOVERY_WEB_DASHBOARD_GUIDE.md`
- **API Documentation:** Endpoints disponíveis em `/api/events/*`

## 🚀 **Próximos Passos**

1. **Integração com UDP Statistics:** Conectar eventos UDP ao sistema de estatísticas
2. **Filtros Avançados:** Adicionar filtros por timestamp, região, etc.
3. **Exportação de Dados:** Permitir exportar dados filtrados
4. **Alertas:** Sistema de notificações para eventos específicos
5. **Histórico:** Armazenar histórico de eventos para análise

---

**Sistema implementado com sucesso!** 🎉

O sistema de categorização e filtros está totalmente funcional e integrado ao dashboard web, oferecendo uma experiência completa para análise e monitoramento de eventos do Albion Sniffer.
