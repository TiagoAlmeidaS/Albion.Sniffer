# DungeonFoundV1

## Event Metadata

| Property | Value |
|----------|-------|
| **Routing Key** | `albion.event.pve.dungeon.found.v1` |
| **Contract** | `Albion.Events.V1.DungeonFoundV1` |
| **Domain** | pve |
| **Version** | v1 |
| **Exchange** | `albion.events` |
| **Content-Type** | `application/x-msgpack` (preferred), `application/json` (fallback) |

## Description

Emitted when a dungeon entrance is discovered

## Characteristics

- **Frequency**: Low
- **Idempotent**: Yes

## Headers

### Required Headers
- `x-event-id`: Unique event identifier (GUID/ULID)
- `x-event-ts`: Event timestamp (RFC3339 UTC)
- `x-contract`: `Albion.Events.V1.DungeonFoundV1`

### Optional Headers
- `x-profile`: Player profile identifier (when applicable)
- `x-correlation-id`: For tracing related events
- `x-source`: Source system identifier

## Schema

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `EventId` | string(uuid) | Yes |  |
| `ObservedAt` | string(date-time) | Yes |  |
| `Id` | int | Yes |  |
| `Type` | string | Yes | Dungeon type (SOLO, GROUP, AVALONIAN) |
| `X` | float | Yes |  |
| `Y` | float | Yes |  |

## Example

### JSON Payload
```json
{
  "EventId": "aa0e8400-e29b-41d4-a716-446655440005",
  "ObservedAt": "2025-01-19T16:00:00Z",
  "Id": 54321,
  "Type": "GROUP",
  "X": 3072.0,
  "Y": 2048.0
}
```

### MessagePack
When using MessagePack serialization (preferred), the same structure is used but encoded in binary format for better performance.

## Usage Notes

### Publishing
```csharp
// Publish to: albion.event.pve.dungeon.found.v1
var contract = new DungeonFoundV1
{
    EventId = "aa0e8400-e29b-41d4-a716-446655440005",
    ObservedAt = "2025-01-19T16:00:00Z",
    Id = 54321,
    Type = "GROUP",
    X = 3072.0,
    Y = 2048.0,
};
await publisher.PublishAsync("albion.event.pve.dungeon.found.v1", contract);
```

### Consuming
```csharp
// Bind queue to routing key
channel.QueueBind(queue: "your-queue", exchange: "albion.events", routingKey: "albion.event.pve.dungeon.found.v1");

// Or use wildcard patterns:
// - All pve events: "albion.event.pve.*.v1"
// - All V1 events: "albion.event.*.*.v1"
// - All events: "albion.event.#"
```

## Related Events

- Other pve domain events
- See [Event Overview](../00-overview.md) for all available events

## Version History

- **v1** - Initial version
- See [CHANGELOG](../../messaging/CHANGELOG_EVENTS.md) for detailed changes
