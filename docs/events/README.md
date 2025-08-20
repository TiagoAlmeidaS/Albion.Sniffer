# Albion.Sniffer Events Documentation

## Overview

This directory contains comprehensive documentation for all events published by the Albion.Sniffer system. The documentation is automatically generated from the source of truth YAML file and follows strict conventions for consistency.

## Quick Start

### For Event Consumers

1. **Browse Events by Domain:**
   - [Player Events](players/) - Player tracking, equipment changes
   - [World Events](world/) - Map changes, hideout discoveries
   - [PvE Events](pve/) - Dungeons, chests, mob spawns
   - [Resource Events](resource/) - Resource nodes, fishing zones
   - [System Events](system/) - Heartbeats, errors, monitoring

2. **Understand the Conventions:**
   - Read [00-overview.md](00-overview.md) for routing key patterns and standards
   - Check [../messaging/rabbit-topology.md](../messaging/rabbit-topology.md) for queue setup

3. **Subscribe to Events:**
   ```csharp
   // Example: Subscribe to all player events
   channel.QueueBind(queue: "your-queue", 
                    exchange: "albion.events", 
                    routingKey: "albion.event.player.*.v1");
   ```

### For Event Publishers

1. **Use the V1 Contracts:**
   ```csharp
   using Albion.Events.V1;
   
   var playerSpotted = new PlayerSpottedV1
   {
       EventId = Guid.NewGuid().ToString(),
       ObservedAt = DateTimeOffset.UtcNow,
       // ... other fields
   };
   
   await publisher.PublishAsync("albion.event.player.spotted.v1", playerSpotted);
   ```

2. **Include Required Headers:**
   - `x-event-id`: Unique event identifier
   - `x-event-ts`: Event timestamp (RFC3339)
   - `x-contract`: Full contract type name

## Documentation Structure

```
events/
├── 00-overview.md                 # Conventions and standards
├── _events.yaml                    # Source of truth for all events
├── generate_docs.py               # Documentation generator script
├── README.md                      # This file
├── players/                       # Player domain events
│   ├── playerspotted.v1.md
│   ├── playerlost.v1.md
│   └── equipmentchanged.v1.md
├── world/                         # World domain events
│   ├── mapchanged.v1.md
│   └── hideoutspotted.v1.md
├── pve/                          # PvE domain events
│   ├── dungeonfound.v1.md
│   ├── chestspawned.v1.md
│   └── mobspawned.v1.md
├── resource/                      # Resource domain events
│   ├── resourcenode.v1.md
│   └── fishingzonefound.v1.md
└── system/                        # System domain events
    ├── heartbeat.v1.md
    └── publishfailure.v1.md
```

## Event Domains

### Player Domain
Events related to player activities and state changes:
- **PlayerSpotted**: When a player enters the observable area
- **PlayerLost**: When a player leaves the observable area
- **EquipmentChanged**: When a player's equipment changes

### World Domain
Events related to world state and environment:
- **MapChanged**: When the observed player changes zones/clusters
- **HideoutSpotted**: When a hideout is discovered

### PvE Domain
Events related to PvE content:
- **DungeonFound**: When a dungeon entrance is discovered
- **ChestSpawned**: When a loot chest is found
- **MobSpawned**: When a mob is spawned or discovered

### Resource Domain
Events related to resource gathering:
- **ResourceNode**: When a harvestable resource is found
- **FishingZoneFound**: When a fishing zone is discovered

### System Domain
Events related to system health and monitoring:
- **Heartbeat**: Periodic health check signal
- **PublishFailure**: When event publishing fails

## Updating Documentation

### Adding New Events

1. **Update the YAML source:**
   Edit `_events.yaml` to add your new event definition

2. **Regenerate documentation:**
   ```bash
   cd docs/events
   python3 generate_docs.py
   ```

3. **Update changelog:**
   Document the addition in [../messaging/CHANGELOG_EVENTS.md](../messaging/CHANGELOG_EVENTS.md)

### Modifying Existing Events

For non-breaking changes (adding optional fields):
1. Update the event in `_events.yaml`
2. Regenerate docs
3. Note the change in changelog

For breaking changes:
1. Create a new version (V2) in `_events.yaml`
2. Keep V1 for backward compatibility
3. Document migration path in changelog

## Testing Events

### Unit Tests
```bash
dotnet test --filter "FullyQualifiedName~V1ContractsTests"
```

### Integration Tests
```bash
dotnet test --filter "FullyQualifiedName~RabbitProvisioningIntegrationTests"
```

### Manual Testing
Use the sample configuration in [../messaging/messaging.settings.sample.json](../messaging/messaging.settings.sample.json) to set up a test environment.

## Performance Considerations

1. **Use MessagePack**: Primary serialization format for better performance
2. **Batch Publishing**: Default batch size of 100 messages
3. **Message Size**: Keep payloads under 256KB
4. **Compression**: Enabled by default for MessagePack

## Monitoring

Key metrics to track:
- Messages published per second by event type
- Message size distribution
- Publishing latency (p50, p95, p99)
- Error rates by event type

## Support

- **Issues**: Report problems in the GitHub repository
- **Questions**: Check the FAQ in [00-overview.md](00-overview.md)
- **Changes**: Follow the changelog in [../messaging/CHANGELOG_EVENTS.md](../messaging/CHANGELOG_EVENTS.md)

## Related Documentation

- [RabbitMQ Topology](../messaging/rabbit-topology.md) - Queue and exchange configuration
- [Event Changelog](../messaging/CHANGELOG_EVENTS.md) - Version history and migration guides
- [Configuration Sample](../messaging/messaging.settings.sample.json) - Example configuration

## License

This documentation is part of the Albion.Sniffer project and follows the same license terms.