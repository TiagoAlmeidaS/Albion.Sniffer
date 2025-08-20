# HeartbeatV1

## Event Metadata

| Property | Value |
|----------|-------|
| **Routing Key** | `albion.event.system.heartbeat.v1` |
| **Contract** | `Albion.Events.V1.HeartbeatV1` |
| **Domain** | system |
| **Version** | v1 |
| **Exchange** | `albion.events` |
| **Content-Type** | `application/x-msgpack` (preferred), `application/json` (fallback) |

## Description

Periodic heartbeat to indicate system is alive

## Characteristics

- **Frequency**: Very Low (every 60 seconds)
- **Idempotent**: Yes

## Headers

### Required Headers
- `x-event-id`: Unique event identifier (GUID/ULID)
- `x-event-ts`: Event timestamp (RFC3339 UTC)
- `x-contract`: `Albion.Events.V1.HeartbeatV1`

### Optional Headers
- `x-profile`: Player profile identifier (when applicable)
- `x-correlation-id`: For tracing related events
- `x-source`: Source system identifier

## Schema

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `EventId` | string(uuid) | Yes |  |
| `ObservedAt` | string(date-time) | Yes |  |
| `SnifferId` | string | Yes | Unique sniffer instance identifier |
| `Version` | string | Yes | Sniffer version |
| `Uptime` | int | Yes | Uptime in seconds |
| `EventsProcessed` | int | Yes | Total events processed since startup |
| `PacketsCaptured` | int | Yes | Total packets captured since startup |

## Example

### JSON Payload
```json
{
  "EventId": "ff0e8400-e29b-41d4-a716-446655440010",
  "ObservedAt": "2025-01-19T18:00:00Z",
  "SnifferId": "sniffer-001",
  "Version": "1.0.0",
  "Uptime": 3600,
  "EventsProcessed": 150000,
  "PacketsCaptured": 500000
}
```

### MessagePack
When using MessagePack serialization (preferred), the same structure is used but encoded in binary format for better performance.

## Usage Notes

### Publishing
```csharp
// Publish to: albion.event.system.heartbeat.v1
var contract = new HeartbeatV1
{
    EventId = "ff0e8400-e29b-41d4-a716-446655440010",
    ObservedAt = "2025-01-19T18:00:00Z",
    SnifferId = "sniffer-001",
    Version = "1.0.0",
    Uptime = 3600,
    EventsProcessed = 150000,
    PacketsCaptured = 500000,
};
await publisher.PublishAsync("albion.event.system.heartbeat.v1", contract);
```

### Consuming
```csharp
// Bind queue to routing key
channel.QueueBind(queue: "your-queue", exchange: "albion.events", routingKey: "albion.event.system.heartbeat.v1");

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
