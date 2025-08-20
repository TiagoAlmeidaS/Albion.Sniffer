# 🗂️ Estrutura Hierárquica de Tópicos - Albion Events V1

## 📋 **Visão Geral**

A partir da versão atual, todos os eventos V1 são publicados em tópicos hierárquicos organizados por contexto, facilitando a filtragem e consumo por parte dos serviços downstream.

## 🎯 **Benefícios da Estrutura Hierárquica**

- **✅ Organização por Domínio**: Eventos agrupados por contexto lógico
- **✅ Filtragem Eficiente**: Consumidores podem se inscrever em categorias específicas
- **✅ Escalabilidade**: Fácil adição de novos tipos de eventos
- **✅ Padrão de Mercado**: Segue convenções utilizadas por AWS, Azure, etc.
- **✅ Melhor Debuging**: Identificação mais fácil do tipo de evento

## 🗺️ **Estrutura de Tópicos**

### **1. Player Events** 🧙‍♂️

| Evento V1 | Tópico Hierárquico | Descrição |
|-----------|-------------------|-----------|
| `PlayerSpottedV1` | `albion.event.player.spotted` | Jogador avistado no mundo |
| `PlayerMovedV1` | `albion.event.player.moved` | Jogador se movimentou |
| `PlayerJoinedV1` | `albion.event.player.joined` | Jogador entrou no jogo |
| `EntityLeftV1` | `albion.event.player.left` | Jogador saiu do jogo |
| `PlayerMoveRequestV1` | `albion.event.player.move.request` | Solicitação de movimento do jogador |
| `EquipmentChangedV1` | `albion.event.player.equipment.changed` | Equipamento do jogador alterado |
| `MountedStateChangedV1` | `albion.event.player.mounted.changed` | Estado de montaria alterado |
| `FlaggingFinishedV1` | `albion.event.player.flagging.finished` | Processo de flagging concluído |
| `HealthUpdatedV1` | `albion.event.player.health.updated` | Vida do jogador atualizada |
| `RegenerationChangedV1` | `albion.event.player.regeneration.changed` | Regeneração do jogador alterada |

### **2. Cluster Events** 🗺️

| Evento V1 | Tópico Hierárquico | Descrição |
|-----------|-------------------|-----------|
| `ClusterChangedV1` | `albion.event.cluster.changed` | Mudança de cluster/zona |
| `ClusterObjectsLoadedV1` | `albion.event.cluster.objects.loaded` | Objetos do cluster carregados |
| `KeySyncV1` | `albion.event.cluster.key.sync` | Sincronização de chave do cluster |

### **3. Mob Events** 👹

| Evento V1 | Tópico Hierárquico | Descrição |
|-----------|-------------------|-----------|
| `MobSpawnedV1` | `albion.event.mob.spawned` | Mob apareceu no mundo |
| `MobStateChangedV1` | `albion.event.mob.state.changed` | Estado do mob alterado |

### **4. Harvestable Events** 🌿

| Evento V1 | Tópico Hierárquico | Descrição |
|-----------|-------------------|-----------|
| `HarvestableFoundV1` | `albion.event.harvestable.found` | Recurso coletável encontrado |
| `HarvestablesListFoundV1` | `albion.event.harvestable.list.found` | Lista de recursos encontrada |
| `HarvestableStateChangedV1` | `albion.event.harvestable.state.changed` | Estado do recurso alterado |

### **5. World Objects Events** 🏰

| Evento V1 | Tópico Hierárquico | Descrição |
|-----------|-------------------|-----------|
| `DungeonFoundV1` | `albion.event.world.dungeon.found` | Dungeon encontrada |
| `FishingZoneFoundV1` | `albion.event.world.fishing.zone.found` | Zona de pesca encontrada |
| `LootChestFoundV1` | `albion.event.world.loot.chest.found` | Baú de tesouro encontrado |
| `WispGateOpenedV1` | `albion.event.world.wisp.gate.opened` | Portal de wisp aberto |
| `GatedWispFoundV1` | `albion.event.world.gated.wisp.found` | Wisp com portal encontrado |

### **6. Mists Events** 🌫️

| Evento V1 | Tópico Hierárquico | Descrição |
|-----------|-------------------|-----------|
| `MistsPlayerJoinedV1` | `albion.event.mists.player.joined` | Jogador entrou nas Mists |

## 🔧 **Implementação Técnica**

A estrutura hierárquica é implementada no `EventToQueueBridge` através do método `GetHierarchicalTopic()`:

```csharp
private static string GetHierarchicalTopic(string eventType)
{
    return eventType switch
    {
        // Player Events 🧙‍♂️
        "PlayerMovedV1" => "albion.event.player.moved",
        "PlayerJoinedV1" => "albion.event.player.joined",
        // ... outros mapeamentos
        
        // Evento não mapeado
        _ => string.Empty
    };
}
```

## 📊 **Padrões de Consumo**

### **Consumir todos os eventos de jogadores:**
```
albion.event.player.*
```

### **Consumir apenas movimentação:**
```
albion.event.player.moved
```

### **Consumir todos os eventos do mundo:**
```
albion.event.world.*
```

### **Consumir eventos específicos de cluster:**
```
albion.event.cluster.changed
albion.event.cluster.objects.loaded
```

## 🔄 **Fallback**

Eventos não mapeados ainda funcionam com o padrão anterior:
- **Eventos Core**: `albion.event.{eventname}`
- **Eventos V1 não mapeados**: `albion.event.{eventname}` (sem sufixo V1)

## ⚡ **Performance**

- **Mapeamento via Switch Expression**: O(1) lookup time
- **Zero Allocation**: Strings são constantes compiladas
- **Fallback Eficiente**: Apenas para eventos não mapeados

## 🎉 **Resultado**

**Antes:**
```
PlayerMovedV1 → albion.event.playermovedv1
```

**Depois:**
```
PlayerMovedV1 → albion.event.player.moved
```

Esta estrutura torna o sistema mais profissional, organizando e facilitando o consumo dos eventos por contexto! 🚀
