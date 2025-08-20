# ResourceNodeV1

## Event Metadata

| Property | Value |
|----------|-------|
| **Routing Key** | `albion.event.resource.node.v1` |
| **Contract** | `Albion.Events.V1.HarvestableFoundV1` |
| **Domain** | resource |
| **Version** | v1 |
| **Exchange** | `albion.events` |
| **Content-Type** | `application/x-msgpack` (preferred), `application/json` (fallback) |

## Description

Emitted when a harvestable resource node is discovered

## Characteristics

- **Frequency**: Medium
- **Idempotent**: Yes

## Headers

### Required Headers
- `x-event-id`: Unique event identifier (GUID/ULID)
- `x-event-ts`: Event timestamp (RFC3339 UTC)
- `x-contract`: `Albion.Events.V1.HarvestableFoundV1`

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
| `Type` | string | Yes | Resource type (WOOD, STONE, ORE, FIBER, HIDE) |
| `Tier` | int | Yes |  |
| `Charges` | int | Yes | Remaining charges/resources |
| `X` | float | Yes |  |
| `Y` | float | Yes |  |

## Example

### JSON Payload
```json
{
  "EventId": "dd0e8400-e29b-41d4-a716-446655440008",
  "ObservedAt": "2025-01-19T17:00:00Z",
  "Id": 33333,
  "Type": "ORE",
  "Tier": 8,
  "Charges": 27,
  "X": 2200.0,
  "Y": 1800.0
}
```

### MessagePack
When using MessagePack serialization (preferred), the same structure is used but encoded in binary format for better performance.

## Usage Notes

### Publishing
```csharp
// Publish to: albion.event.resource.node.v1
var contract = new HarvestableFoundV1
{
    EventId = "dd0e8400-e29b-41d4-a716-446655440008",
    ObservedAt = "2025-01-19T17:00:00Z",
    Id = 33333,
    Type = "ORE",
    Tier = 8,
    Charges = 27,
    X = 2200.0,
    Y = 1800.0,
};
await publisher.PublishAsync("albion.event.resource.node.v1", contract);
```

### Consuming
```csharp
// Bind queue to routing key
channel.QueueBind(queue: "your-queue", exchange: "albion.events", routingKey: "albion.event.resource.node.v1");

// Or use wildcard patterns:
// - All resource events: "albion.event.resource.*.v1"
// - All V1 events: "albion.event.*.*.v1"
// - All events: "albion.event.#"
```

## Related Events

- Other resource domain events
- See [Event Overview](../00-overview.md) for all available events

## Version History

- **v1** - Initial version
- See [CHANGELOG](../../messaging/CHANGELOG_EVENTS.md) for detailed changes
