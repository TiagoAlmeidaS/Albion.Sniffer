# PublishFailureV1

## Event Metadata

| Property | Value |
|----------|-------|
| **Routing Key** | `albion.event.system.publishfailure.v1` |
| **Contract** | `Albion.Events.V1.PublishFailureV1` |
| **Domain** | system |
| **Version** | v1 |
| **Exchange** | `albion.events` |
| **Content-Type** | `application/x-msgpack` (preferred), `application/json` (fallback) |

## Description

Emitted when event publishing fails after retries

## Characteristics

- **Frequency**: Very Low
- **Idempotent**: No

## Headers

### Required Headers
- `x-event-id`: Unique event identifier (GUID/ULID)
- `x-event-ts`: Event timestamp (RFC3339 UTC)
- `x-contract`: `Albion.Events.V1.PublishFailureV1`

### Optional Headers
- `x-profile`: Player profile identifier (when applicable)
- `x-correlation-id`: For tracing related events
- `x-source`: Source system identifier

## Schema

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `EventId` | string(uuid) | Yes |  |
| `ObservedAt` | string(date-time) | Yes |  |
| `OriginalEventType` | string | Yes | Type of event that failed to publish |
| `OriginalEventId` | string | Yes | ID of event that failed to publish |
| `Error` | string | Yes | Error message |
| `RetryCount` | int | Yes | Number of retries attempted |

## Example

### JSON Payload
```json
{
  "EventId": "110e8400-e29b-41d4-a716-446655440011",
  "ObservedAt": "2025-01-19T18:30:00Z",
  "OriginalEventType": "PlayerSpottedV1",
  "OriginalEventId": "550e8400-e29b-41d4-a716-446655440000",
  "Error": "Connection timeout after 5 retries",
  "RetryCount": 5
}
```

### MessagePack
When using MessagePack serialization (preferred), the same structure is used but encoded in binary format for better performance.

## Usage Notes

### Publishing
```csharp
// Publish to: albion.event.system.publishfailure.v1
var contract = new PublishFailureV1
{
    EventId = "110e8400-e29b-41d4-a716-446655440011",
    ObservedAt = "2025-01-19T18:30:00Z",
    OriginalEventType = "PlayerSpottedV1",
    OriginalEventId = "550e8400-e29b-41d4-a716-446655440000",
    Error = "Connection timeout after 5 retries",
    RetryCount = 5,
};
await publisher.PublishAsync("albion.event.system.publishfailure.v1", contract);
```

### Consuming
```csharp
// Bind queue to routing key
channel.QueueBind(queue: "your-queue", exchange: "albion.events", routingKey: "albion.event.system.publishfailure.v1");

// Or use wildcard patterns:
// - All system events: "albion.event.system.*.v1"
// - All V1 events: "albion.event.*.*.v1"
// - All events: "albion.event.#"
```

## Related Events

- Other system domain events
- See [Event Overview](../00-overview.md) for all available events

## Version History

- **v1** - Initial version
- See [CHANGELOG](../../messaging/CHANGELOG_EVENTS.md) for detailed changes
