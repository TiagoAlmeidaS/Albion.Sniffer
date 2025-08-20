# HideoutSpottedV1

## Event Metadata

| Property | Value |
|----------|-------|
| **Routing Key** | `albion.event.world.hideout.spotted.v1` |
| **Contract** | `Albion.Events.V1.HideoutSpottedV1` |
| **Domain** | world |
| **Version** | v1 |
| **Exchange** | `albion.events` |
| **Content-Type** | `application/x-msgpack` (preferred), `application/json` (fallback) |

## Description

Emitted when a hideout is discovered

## Characteristics

- **Frequency**: Very Low
- **Idempotent**: Yes

## Headers

### Required Headers
- `x-event-id`: Unique event identifier (GUID/ULID)
- `x-event-ts`: Event timestamp (RFC3339 UTC)
- `x-contract`: `Albion.Events.V1.HideoutSpottedV1`

### Optional Headers
- `x-profile`: Player profile identifier (when applicable)
- `x-correlation-id`: For tracing related events
- `x-source`: Source system identifier

## Schema

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `EventId` | string(uuid) | Yes |  |
| `ObservedAt` | string(date-time) | Yes |  |
| `HideoutId` | int | Yes |  |
| `GuildName` | string | Yes |  |
| `AllianceName` | string | No |  |
| `X` | float | Yes |  |
| `Y` | float | Yes |  |
| `Tier` | int | Yes |  |

## Example

### JSON Payload
```json
{
  "EventId": "990e8400-e29b-41d4-a716-446655440004",
  "ObservedAt": "2025-01-19T15:30:00Z",
  "HideoutId": 98765,
  "GuildName": "ARCH",
  "AllianceName": "ARCH Alliance",
  "X": 2048.0,
  "Y": 1536.0,
  "Tier": 8
}
```

### MessagePack
When using MessagePack serialization (preferred), the same structure is used but encoded in binary format for better performance.

## Usage Notes

### Publishing
```csharp
// Publish to: albion.event.world.hideout.spotted.v1
var contract = new HideoutSpottedV1
{
    EventId = "990e8400-e29b-41d4-a716-446655440004",
    ObservedAt = "2025-01-19T15:30:00Z",
    HideoutId = 98765,
    GuildName = "ARCH",
    AllianceName = "ARCH Alliance",
    X = 2048.0,
    Y = 1536.0,
    Tier = 8,
};
await publisher.PublishAsync("albion.event.world.hideout.spotted.v1", contract);
```

### Consuming
```csharp
// Bind queue to routing key
channel.QueueBind(queue: "your-queue", exchange: "albion.events", routingKey: "albion.event.world.hideout.spotted.v1");

// Or use wildcard patterns:
// - All world events: "albion.event.world.*.v1"
// - All V1 events: "albion.event.*.*.v1"
// - All events: "albion.event.#"
```

## Related Events

- Other world domain events
- See [Event Overview](../00-overview.md) for all available events

## Version History

- **v1** - Initial version
- See [CHANGELOG](../../messaging/CHANGELOG_EVENTS.md) for detailed changes
