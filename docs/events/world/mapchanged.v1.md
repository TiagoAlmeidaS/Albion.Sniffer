# MapChangedV1

## Event Metadata

| Property | Value |
|----------|-------|
| **Routing Key** | `albion.event.world.mapchanged.v1` |
| **Contract** | `Albion.Events.V1.ClusterChangedV1` |
| **Domain** | world |
| **Version** | v1 |
| **Exchange** | `albion.events` |
| **Content-Type** | `application/x-msgpack` (preferred), `application/json` (fallback) |

## Description

Emitted when the observed player changes map/cluster

## Characteristics

- **Frequency**: Low
- **Idempotent**: Yes

## Headers

### Required Headers
- `x-event-id`: Unique event identifier (GUID/ULID)
- `x-event-ts`: Event timestamp (RFC3339 UTC)
- `x-contract`: `Albion.Events.V1.ClusterChangedV1`

### Optional Headers
- `x-profile`: Player profile identifier (when applicable)
- `x-correlation-id`: For tracing related events
- `x-source`: Source system identifier

## Schema

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `EventId` | string(uuid) | Yes |  |
| `ObservedAt` | string(date-time) | Yes |  |
| `ClusterIndex` | string | Yes |  |
| `ClusterName` | string | Yes |  |
| `Region` | string | Yes |  |
| `MapType` | string | Yes | Type of map (OPEN_WORLD, DUNGEON, EXPEDITION, etc.) |

## Example

### JSON Payload
```json
{
  "EventId": "880e8400-e29b-41d4-a716-446655440003",
  "ObservedAt": "2025-01-19T15:00:00Z",
  "ClusterIndex": "1004",
  "ClusterName": "Bridgewatch",
  "Region": "Royal",
  "MapType": "OPEN_WORLD"
}
```

### MessagePack
When using MessagePack serialization (preferred), the same structure is used but encoded in binary format for better performance.

## Usage Notes

### Publishing
```csharp
// Publish to: albion.event.world.mapchanged.v1
var contract = new ClusterChangedV1
{
    EventId = "880e8400-e29b-41d4-a716-446655440003",
    ObservedAt = "2025-01-19T15:00:00Z",
    ClusterIndex = "1004",
    ClusterName = "Bridgewatch",
    Region = "Royal",
    MapType = "OPEN_WORLD",
};
await publisher.PublishAsync("albion.event.world.mapchanged.v1", contract);
```

### Consuming
```csharp
// Bind queue to routing key
channel.QueueBind(queue: "your-queue", exchange: "albion.events", routingKey: "albion.event.world.mapchanged.v1");

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
