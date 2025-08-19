# Albion.Events.V1

Contratos para eventos de domínio publicados em tópicos versionados (*.v1).
- Use MessagePack para publicação binária
- JSON para debugging, snapshots e ferramentas

## Tópicos Disponíveis

### Jogadores
- **Player Spotted**: `albion.event.player.spotted.v1`
- **Equipment Changed**: `albion.event.equipment.changed.v1`
- **Health Updated**: `albion.event.health.updated.v1`
- **Mounted State**: `albion.event.mounted.state.changed.v1`
- **Key Sync**: `albion.event.key.sync.v1`
- **Entity Left**: `albion.event.entity.left.v1`

### Mobs
- **Mob Spawned**: `albion.event.mob.spawned.v1`
- **Mob State Changed**: `albion.event.mob.state.changed.v1`

### Recursos
- **Harvestable Found**: `albion.event.harvestable.found.v1`
- **Harvestable State Changed**: `albion.event.harvestable.state.changed.v1`
- **Harvestables List Found**: `albion.event.harvestables.list.found.v1`

### Mundo
- **Cluster Changed**: `albion.event.cluster.changed.v1`
- **Cluster Objects Loaded**: `albion.event.cluster.objects.loaded.v1`
- **Dungeon Found**: `albion.event.dungeon.found.v1`
- **Fishing Zone Found**: `albion.event.fishing.zone.found.v1`
- **Gated Wisp Found**: `albion.event.gated.wisp.found.v1`
- **Loot Chest Found**: `albion.event.loot.chest.found.v1`
- **Wisp Gate Opened**: `albion.event.wisp.gate.opened.v1`

### Sistema
- **Flagging Finished**: `albion.event.flagging.finished.v1`
- **Regeneration Changed**: `albion.event.regeneration.changed.v1`
- **Mists Player Joined**: `albion.event.mists.player.joined.v1`

## Versionamento

- Mudanças que quebram compatibilidade requerem novo pacote V2 e sufixo de tópico `.v2`
- Contratos V1 permanecem estáveis para consumidores existentes
- Novos campos opcionais podem ser adicionados sem quebrar V1

## Estrutura dos Contratos

Todos os contratos V1 seguem o padrão:
- `EventId`: Identificador único do evento
- `ObservedAt`: Timestamp UTC da observação
- Campos específicos do tipo de evento
- Campos opcionais marcados com `?`

## Serialização

- **MessagePack**: Formato principal para publicação em filas
- **JSON**: Fallback para debugging e ferramentas
- Todos os contratos são `sealed` para performance
- Uso de `required` para campos obrigatórios
