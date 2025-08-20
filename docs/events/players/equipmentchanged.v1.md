# EquipmentChangedV1

## Event Metadata

| Property | Value |
|----------|-------|
| **Routing Key** | `albion.event.player.equipment.changed.v1` |
| **Contract** | `Albion.Events.V1.EquipmentChangedV1` |
| **Domain** | players |
| **Version** | v1 |
| **Exchange** | `albion.events` |
| **Content-Type** | `application/x-msgpack` (preferred), `application/json` (fallback) |

## Description

Emitted when a player's equipment changes

## Characteristics

- **Frequency**: Low
- **Idempotent**: Yes

## Headers

### Required Headers
- `x-event-id`: Unique event identifier (GUID/ULID)
- `x-event-ts`: Event timestamp (RFC3339 UTC)
- `x-contract`: `Albion.Events.V1.EquipmentChangedV1`

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
| `MainHand` | string | No |  |
| `OffHand` | string | No |  |
| `Head` | string | No |  |
| `Chest` | string | No |  |
| `Shoes` | string | No |  |

## Example

### JSON Payload
```json
{
  "EventId": "770e8400-e29b-41d4-a716-446655440002",
  "ObservedAt": "2025-01-19T14:40:00Z",
  "Id": 12345,
  "MainHand": "T8_MAIN_CURSEDSTAFF",
  "OffHand": "T8_OFF_BOOK",
  "Head": "T8_HEAD_CLOTH_SET3",
  "Chest": "T8_ARMOR_CLOTH_SET3",
  "Shoes": "T8_SHOES_CLOTH_SET3"
}
```

### MessagePack
When using MessagePack serialization (preferred), the same structure is used but encoded in binary format for better performance.

## Usage Notes

### Publishing
```csharp
// Publish to: albion.event.player.equipment.changed.v1
var contract = new EquipmentChangedV1
{
    EventId = "770e8400-e29b-41d4-a716-446655440002",
    ObservedAt = "2025-01-19T14:40:00Z",
    Id = 12345,
    MainHand = "T8_MAIN_CURSEDSTAFF",
    OffHand = "T8_OFF_BOOK",
    Head = "T8_HEAD_CLOTH_SET3",
    Chest = "T8_ARMOR_CLOTH_SET3",
    Shoes = "T8_SHOES_CLOTH_SET3",
};
await publisher.PublishAsync("albion.event.player.equipment.changed.v1", contract);
```

### Consuming
```csharp
// Bind queue to routing key
channel.QueueBind(queue: "your-queue", exchange: "albion.events", routingKey: "albion.event.player.equipment.changed.v1");

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
