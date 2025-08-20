# PlayerSpottedV1

## Event Metadata

| Property | Value |
|----------|-------|
| **Routing Key** | `albion.event.player.spotted.v1` |
| **Contract** | `Albion.Events.V1.PlayerSpottedV1` |
| **Domain** | players |
| **Version** | v1 |
| **Exchange** | `albion.events` |
| **Content-Type** | `application/x-msgpack` (preferred), `application/json` (fallback) |

## Description

Emitted when a player is observed in the game world

## Characteristics

- **Frequency**: High (every player movement)
- **Idempotent**: Yes

## Headers

### Required Headers
- `x-event-id`: Unique event identifier (GUID/ULID)
- `x-event-ts`: Event timestamp (RFC3339 UTC)
- `x-contract`: `Albion.Events.V1.PlayerSpottedV1`

### Optional Headers
- `x-profile`: Player profile identifier (when applicable)
- `x-correlation-id`: For tracing related events
- `x-source`: Source system identifier

## Schema

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `EventId` | string(uuid) | Yes | Unique event identifier |
| `ObservedAt` | string(date-time) | Yes | Timestamp when player was observed |
| `Cluster` | string | Yes | Current cluster/zone identifier |
| `Region` | string | Yes | Region name |
| `PlayerId` | int | Yes | Unique player identifier |
| `PlayerName` | string | Yes | Player character name |
| `GuildName` | string | No | Guild tag if player is in a guild |
| `AllianceName` | string | No | Alliance name if guild is in an alliance |
| `X` | float | Yes | X coordinate in world space |
| `Y` | float | Yes | Y coordinate in world space |
| `Tier` | int | Yes | Estimated equipment tier |

## Example

### JSON Payload
```json
{
  "EventId": "550e8400-e29b-41d4-a716-446655440000",
  "ObservedAt": "2025-01-19T14:30:00Z",
  "Cluster": "Caerleon",
  "Region": "Royal",
  "PlayerId": 12345,
  "PlayerName": "JohnDoe",
  "GuildName": "ARCH",
  "AllianceName": "ARCH Alliance",
  "X": 1024.5,
  "Y": 768.2,
  "Tier": 8
}
```

### MessagePack
When using MessagePack serialization (preferred), the same structure is used but encoded in binary format for better performance.

## Usage Notes

### Publishing
```csharp
// Publish to: albion.event.player.spotted.v1
var contract = new PlayerSpottedV1
{
    EventId = "550e8400-e29b-41d4-a716-446655440000",
    ObservedAt = "2025-01-19T14:30:00Z",
    Cluster = "Caerleon",
    Region = "Royal",
    PlayerId = 12345,
    PlayerName = "JohnDoe",
    GuildName = "ARCH",
    AllianceName = "ARCH Alliance",
    X = 1024.5,
    Y = 768.2,
    Tier = 8,
};
await publisher.PublishAsync("albion.event.player.spotted.v1", contract);
```

### Consuming
```csharp
// Bind queue to routing key
channel.QueueBind(queue: "your-queue", exchange: "albion.events", routingKey: "albion.event.player.spotted.v1");

// Or use wildcard patterns:
// - All players events: "albion.event.player.*.v1"
// - All V1 events: "albion.event.*.*.v1"
// - All events: "albion.event.#"
```

## Related Events

- Other players domain events
- See [Event Overview](../00-overview.md) for all available events

## Version History

- **v1** - Initial version
- See [CHANGELOG](../../messaging/CHANGELOG_EVENTS.md) for detailed changes
