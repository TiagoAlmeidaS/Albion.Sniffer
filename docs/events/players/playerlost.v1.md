# PlayerLostV1

## Event Metadata

| Property | Value |
|----------|-------|
| **Routing Key** | `albion.event.player.lost.v1` |
| **Contract** | `Albion.Events.V1.EntityLeftV1` |
| **Domain** | players |
| **Version** | v1 |
| **Exchange** | `albion.events` |
| **Content-Type** | `application/x-msgpack` (preferred), `application/json` (fallback) |

## Description

Emitted when a player leaves the observable area

## Characteristics

- **Frequency**: Medium
- **Idempotent**: Yes

## Headers

### Required Headers
- `x-event-id`: Unique event identifier (GUID/ULID)
- `x-event-ts`: Event timestamp (RFC3339 UTC)
- `x-contract`: `Albion.Events.V1.EntityLeftV1`

### Optional Headers
- `x-profile`: Player profile identifier (when applicable)
- `x-correlation-id`: For tracing related events
- `x-source`: Source system identifier

## Schema

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `EventId` | string(uuid) | Yes |  |
| `ObservedAt` | string(date-time) | Yes |  |
| `Id` | int | Yes | Entity ID that left |

## Example

### JSON Payload
```json
{
  "EventId": "660e8400-e29b-41d4-a716-446655440001",
  "ObservedAt": "2025-01-19T14:35:00Z",
  "Id": 12345
}
```

### MessagePack
When using MessagePack serialization (preferred), the same structure is used but encoded in binary format for better performance.

## Usage Notes

### Publishing
```csharp
// Publish to: albion.event.player.lost.v1
var contract = new EntityLeftV1
{
    EventId = "660e8400-e29b-41d4-a716-446655440001",
    ObservedAt = "2025-01-19T14:35:00Z",
    Id = 12345,
};
await publisher.PublishAsync("albion.event.player.lost.v1", contract);
```

### Consuming
```csharp
// Bind queue to routing key
channel.QueueBind(queue: "your-queue", exchange: "albion.events", routingKey: "albion.event.player.lost.v1");

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
