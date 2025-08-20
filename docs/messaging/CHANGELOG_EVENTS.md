# Event Contract Changelog

All notable changes to event contracts will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html) for event contracts.

## Contract Versioning Rules

- **Major version (V1 → V2)**: Breaking changes
- **Minor version (additions)**: New optional fields (non-breaking)
- **Documentation**: Clarifications and examples

## [Unreleased]

### Planning
- `albion.event.combat.damage.dealt.v1` - Combat damage events
- `albion.event.market.order.placed.v1` - Market order events
- `albion.event.guild.territory.captured.v1` - Territory control events

## [1.0.0] - 2025-01-19

### Added - Initial V1 Contracts

#### Player Domain
- `albion.event.player.spotted.v1` - Initial implementation
  - Fields: EventId, ObservedAt, Cluster, Region, PlayerId, PlayerName, GuildName, AllianceName, X, Y, Tier
  - Headers: x-event-id, x-event-ts, x-contract, x-profile
  
- `albion.event.player.lost.v1` - Initial implementation
  - Fields: EventId, ObservedAt, Id
  
- `albion.event.player.equipment.changed.v1` - Initial implementation
  - Fields: EventId, ObservedAt, Id, MainHand, OffHand, Head, Chest, Shoes

#### World Domain
- `albion.event.world.mapchanged.v1` - Initial implementation
  - Fields: EventId, ObservedAt, ClusterIndex, ClusterName, Region, MapType
  
- `albion.event.world.hideout.spotted.v1` - Initial implementation
  - Fields: EventId, ObservedAt, HideoutId, GuildName, AllianceName, X, Y, Tier

#### PvE Domain
- `albion.event.pve.dungeon.found.v1` - Initial implementation
  - Fields: EventId, ObservedAt, Id, Type, X, Y
  
- `albion.event.pve.chest.spawned.v1` - Initial implementation
  - Fields: EventId, ObservedAt, Id, X, Y
  
- `albion.event.pve.mob.spawned.v1` - Initial implementation
  - Fields: EventId, ObservedAt, Id, Name, Type, Tier, X, Y

#### Resource Domain
- `albion.event.resource.node.v1` - Initial implementation
  - Fields: EventId, ObservedAt, Id, Type, Tier, Charges, X, Y
  
- `albion.event.resource.fishing.found.v1` - Initial implementation
  - Fields: EventId, ObservedAt, Id, Tier, X, Y

#### System Domain
- `albion.event.system.heartbeat.v1` - Initial implementation
  - Fields: EventId, ObservedAt, SnifferId, Version, Uptime, EventsProcessed, PacketsCaptured
  
- `albion.event.system.publishfailure.v1` - Initial implementation
  - Fields: EventId, ObservedAt, OriginalEventType, OriginalEventId, Error, RetryCount

### Infrastructure
- Exchange: `albion.events` (topic, durable)
- Dead Letter Exchange: `albion.events.dlx`
- Content-Type support: `application/x-msgpack` (primary), `application/json` (fallback)
- Standard headers defined: x-event-id, x-event-ts, x-contract, x-profile

## Migration Guide

### Migrating from Legacy Events to V1

If you're currently consuming raw game events, follow these steps to migrate to V1 contracts:

1. **Update Dependencies**
   ```xml
   <PackageReference Include="Albion.Events.V1" Version="1.0.0" />
   ```

2. **Update Queue Bindings**
   - Old: Direct event names
   - New: `albion.event.<domain>.<event>.v1` pattern

3. **Update Deserialization**
   ```csharp
   // Old
   var rawEvent = JsonSerializer.Deserialize<Dictionary<string, object>>(message);
   
   // New
   var contract = MessagePackSerializer.Deserialize<PlayerSpottedV1>(message);
   ```

4. **Handle Both During Transition**
   - Bind to both old and new routing keys
   - Detect message format via headers
   - Gradually phase out old format

### Adding New Fields (Non-Breaking)

When adding optional fields to existing V1 contracts:

1. **Update Contract**
   ```csharp
   public string? NewOptionalField { get; init; }  // Nullable = optional
   ```

2. **Update Documentation**
   - Add field to schema table
   - Update example JSON
   - Note addition in this changelog

3. **Backward Compatibility**
   - Existing consumers continue working (field is optional)
   - New consumers can opt-in to use new field

### Creating New Version (Breaking Changes)

When breaking changes are required:

1. **Create V2 Contract**
   ```csharp
   [MessagePackObject(true)]
   public sealed class PlayerSpottedV2  // New class
   ```

2. **New Routing Key**
   - V1: `albion.event.player.spotted.v1`
   - V2: `albion.event.player.spotted.v2`

3. **Dual Publishing Period**
   - Publish to both V1 and V2 for 2-4 weeks
   - Monitor consumer migration
   - Deprecate V1 after migration

4. **Update Documentation**
   - Create new `.v2.md` documentation
   - Mark V1 as deprecated with timeline
   - Update this changelog

## Deprecation Policy

### Deprecation Timeline

1. **Announcement** (Day 0)
   - Add deprecation notice to documentation
   - Log warnings in publisher
   - Notify all known consumers

2. **Migration Period** (Day 0-30)
   - Continue publishing deprecated version
   - Publish new version in parallel
   - Provide migration guide

3. **Final Warning** (Day 30-37)
   - Increase warning log level
   - Send final notifications
   - Verify consumer migration

4. **Removal** (Day 37+)
   - Stop publishing deprecated version
   - Remove from documentation
   - Archive contract code

### Currently Deprecated

None at this time.

## Support Matrix

| Contract Version | Status | Support Until | Notes |
|-----------------|--------|---------------|-------|
| V1 | Active | - | Current version |

## FAQ

### Q: Can I add fields to V1 contracts?
A: Yes, as long as they are optional (nullable) and don't change existing field semantics.

### Q: When should I create V2?
A: When you need to:
- Remove fields
- Change field types
- Make optional fields required
- Change field semantics

### Q: How long should dual publishing last?
A: Minimum 2 weeks, recommended 4 weeks for production systems.

### Q: Can consumers handle multiple versions?
A: Yes, bind to multiple routing keys and detect version via x-contract header.

## Contact

For questions about event contracts:
- GitHub Issues: [Report Issue](https://github.com/your-org/albion-sniffer/issues)
- Documentation: [Event Overview](../events/00-overview.md)
- Topology: [RabbitMQ Topology](rabbit-topology.md)