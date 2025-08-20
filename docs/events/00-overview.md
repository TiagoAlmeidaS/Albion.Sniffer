# Event System Overview

## Conventions and Standards

This document defines the conventions and standards for the Albion.Sniffer event system.

### Exchange Configuration

- **Exchange Name**: `albion.events`
- **Exchange Type**: `topic`
- **Durability**: `true`
- **Auto-delete**: `false`

### Routing Key Convention

All events follow a hierarchical routing key pattern:

```
albion.event.<domain>.<event>.v<version>
```

**Components:**
- `albion.event` - Fixed prefix for all events
- `<domain>` - Event domain (player, world, pve, resource, system)
- `<event>` - Specific event name (spotted, mapchanged, etc.)
- `v<version>` - Version suffix (v1, v2, etc.)

**Examples:**
- `albion.event.player.spotted.v1`
- `albion.event.world.mapchanged.v1`
- `albion.event.pve.chest.spawned.v1`

### Content Type

- **Primary**: `application/x-msgpack` (preferred for performance)
- **Fallback**: `application/json` (for compatibility)

### Standard Headers

All messages include the following headers:

| Header | Type | Required | Description |
|--------|------|----------|-------------|
| `x-event-id` | string (GUID/ULID) | Yes | Unique event identifier |
| `x-event-ts` | string (RFC3339) | Yes | Event timestamp in UTC |
| `x-contract` | string | Yes | Full contract type name (e.g., `Albion.Events.V1.PlayerSpottedV1`) |
| `x-profile` | string | No | Player profile identifier when applicable |
| `x-correlation-id` | string | No | Correlation ID for tracing related events |
| `x-source` | string | No | Source system identifier |

### Versioning Policy

#### Version 1 (V1) Rules
- **Non-breaking changes allowed:**
  - Adding optional fields
  - Adding new events
  - Adding optional headers
  - Extending enums with new values

- **Breaking changes require V2:**
  - Removing fields
  - Changing field types
  - Renaming fields
  - Changing field requirements (optional → required)
  - Changing routing key structure

#### Version Migration
- New versions get new routing keys (e.g., `*.v1` → `*.v2`)
- Consumers can bind to multiple versions during migration
- Publishers should emit both versions during transition period
- Deprecation notices provided via CHANGELOG

### Event Contract Requirements

All event contracts must:

1. **Inherit common fields:**
   - `EventId` (string, GUID/ULID)
   - `ObservedAt` (DateTimeOffset, UTC)

2. **Use MessagePack attributes:**
   ```csharp
   [MessagePackObject(true)]
   public sealed class EventNameV1
   ```

3. **Follow naming conventions:**
   - Contract class: `{EventName}V{Version}`
   - File name: `{EventName}V{Version}.cs`

4. **Include XML documentation:**
   - Class-level summary
   - Property-level documentation for non-obvious fields

### Idempotency

Events should be designed for idempotency:
- Include unique `EventId` for deduplication
- Use `ObservedAt` for temporal ordering
- Include entity IDs for correlation
- Avoid mutable state references

### Performance Considerations

1. **Message Size:**
   - Keep payloads under 256KB
   - Use MessagePack for binary serialization
   - Avoid deep nesting (max 3 levels)

2. **Batching:**
   - Default batch size: 100 messages
   - Batch timeout: 100ms
   - Use persistent delivery mode

3. **Compression:**
   - Enabled by default for MessagePack
   - Gzip for JSON fallback

### Error Handling

1. **Publisher responsibilities:**
   - Retry with exponential backoff
   - Circuit breaker after 5 failures
   - Dead letter queue for failed messages

2. **Consumer responsibilities:**
   - Handle duplicate messages (idempotency)
   - Validate message schema
   - Log and skip malformed messages

### Monitoring and Observability

1. **Metrics to track:**
   - Messages published per second
   - Message size distribution
   - Publishing latency
   - Error rates by event type

2. **Logging requirements:**
   - Log event type and ID on publish
   - Log failures with full context
   - Use structured logging

### Security Considerations

1. **Data sensitivity:**
   - No passwords or tokens in events
   - Minimize PII exposure
   - Use player IDs instead of personal info

2. **Access control:**
   - Authenticate publishers
   - Authorize consumers by queue
   - Encrypt sensitive fields if needed

## Event Domains

### Player Events
Player-related events including movement, combat, and interactions.

### World Events
World state changes including map transitions, territory updates.

### PvE Events
PvE content events including dungeons, chests, and mob spawns.

### Resource Events
Resource gathering events including nodes, fishing zones, and harvesting.

### System Events
System-level events including heartbeats, errors, and monitoring.

## See Also

- [RabbitMQ Topology](../messaging/rabbit-topology.md)
- [Event Changelog](../messaging/CHANGELOG_EVENTS.md)
- [Message Settings Sample](../messaging/messaging.settings.sample.json)