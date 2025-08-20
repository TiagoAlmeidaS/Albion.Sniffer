# RabbitMQ Topology

## Overview

This document describes the recommended RabbitMQ topology for the Albion.Sniffer event system. The topology is designed for scalability, reliability, and clear separation of concerns between different consumers.

## Exchange Configuration

### Primary Exchange

```yaml
Name: albion.events
Type: topic
Durable: true
Auto-delete: false
Arguments:
  x-message-ttl: 86400000  # 24 hours in milliseconds
```

### Dead Letter Exchange

```yaml
Name: albion.events.dlx
Type: topic
Durable: true
Auto-delete: false
```

## Queue Definitions

### 1. Radar Overlay Queue

**Purpose**: Real-time player tracking and map overlay visualization

```yaml
Queue: radar.overlay
Durable: true
Exclusive: false
Auto-delete: false
Arguments:
  x-message-ttl: 300000  # 5 minutes
  x-max-length: 10000    # Maximum 10k messages
  x-overflow: drop-head  # Drop oldest when full
```

**Bindings**:
- `albion.event.player.*.v1` - All player events
- `albion.event.world.*.v1` - All world events
- `albion.event.pve.*.v1` - All PvE events
- `albion.event.resource.*.v1` - All resource events

**Consumers**: Radar overlay applications, real-time map tools

### 2. Analytics Ingest Queue

**Purpose**: Data warehouse ingestion for analytics and reporting

```yaml
Queue: analytics.ingest
Durable: true
Exclusive: false
Auto-delete: false
Arguments:
  x-message-ttl: 604800000  # 7 days
  x-max-length: 1000000     # Maximum 1M messages
  x-overflow: reject-publish # Reject new messages when full
```

**Bindings**:
- `albion.event.#` - All events (wildcard)

**Consumers**: ETL pipelines, data warehouse loaders, analytics engines

### 3. Alerts Operations Queue

**Purpose**: Critical alerts and notifications

```yaml
Queue: alerts.ops
Durable: true
Exclusive: false
Auto-delete: false
Arguments:
  x-message-ttl: 3600000   # 1 hour
  x-max-priority: 10       # Priority queue
  x-max-length: 1000       # Maximum 1k messages
```

**Bindings**:
- `albion.event.player.spotted.v1` - Enemy player detection
- `albion.event.pve.chest.spawned.v1` - High-value chest spawns
- `albion.event.world.hideout.spotted.v1` - Hideout discoveries

**Consumers**: Alert systems, Discord bots, notification services

### 4. Sniffer Telemetry Queue

**Purpose**: System monitoring and health checks

```yaml
Queue: sniffer.telemetry
Durable: true
Exclusive: false
Auto-delete: false
Arguments:
  x-message-ttl: 86400000  # 24 hours
  x-max-length: 10000      # Maximum 10k messages
```

**Bindings**:
- `albion.event.system.*.v1` - All system events

**Consumers**: Monitoring dashboards, health check services, metrics collectors

### 5. Player Profile Queue

**Purpose**: Player profile updates and tracking

```yaml
Queue: player.profile
Durable: true
Exclusive: false
Auto-delete: false
Arguments:
  x-message-ttl: 2592000000  # 30 days
  x-max-length: 100000       # Maximum 100k messages
```

**Bindings**:
- `albion.event.player.spotted.v1`
- `albion.event.player.equipment.changed.v1`

**Consumers**: Player database services, profile aggregators

### 6. Resource Tracker Queue

**Purpose**: Resource node tracking and respawn timers

```yaml
Queue: resource.tracker
Durable: true
Exclusive: false
Auto-delete: false
Arguments:
  x-message-ttl: 7200000   # 2 hours
  x-max-length: 50000      # Maximum 50k messages
```

**Bindings**:
- `albion.event.resource.node.v1`
- `albion.event.resource.fishing.found.v1`

**Consumers**: Resource tracking applications, respawn timer services

## Dead Letter Queue Configuration

### DLQ for Failed Messages

```yaml
Queue: albion.events.dlq
Durable: true
Exclusive: false
Auto-delete: false
Arguments:
  x-message-ttl: 2592000000  # 30 days
  x-max-length: 100000       # Maximum 100k messages
```

**Bindings** (on DLX):
- `#` - All failed messages

## Connection Pooling

### Publisher Connection

```yaml
Connection: albion-publisher
Heartbeat: 60
Connection timeout: 30000
Requested channel max: 2047
```

### Consumer Connections

```yaml
Connection: albion-consumer-{service}
Heartbeat: 60
Connection timeout: 30000
Requested channel max: 10
Prefetch count: 100
```

## High Availability Configuration

### Cluster Configuration

```yaml
Cluster nodes: 3 (minimum)
Queue mirroring: all
Synchronization mode: automatic
Master location: min-masters
```

### Queue Mirroring Policy

```json
{
  "pattern": "^(radar|analytics|alerts|sniffer|player|resource)\\.",
  "definition": {
    "ha-mode": "all",
    "ha-sync-mode": "automatic",
    "ha-sync-batch-size": 5
  }
}
```

## Performance Tuning

### Memory Management

```yaml
vm_memory_high_watermark: 0.6  # 60% of available RAM
disk_free_limit: 5GB
```

### Message Store

```yaml
queue_index_embed_msgs_below: 4096  # Embed small messages
lazy_queue_explicit_gc_run_operation_threshold: 1000
```

### Network Configuration

```yaml
tcp_listen_options:
  backlog: 128
  nodelay: true
  linger: [true, 0]
  exit_on_close: false
```

## Monitoring and Alerting

### Key Metrics to Monitor

1. **Queue Depth**
   - Alert if > 10,000 for radar.overlay
   - Alert if > 100,000 for analytics.ingest
   - Alert if > 100 for alerts.ops

2. **Message Rates**
   - Publish rate per second
   - Consume rate per second
   - Redelivery rate

3. **Connection Health**
   - Active connections count
   - Connection churn rate
   - Channel count per connection

4. **Resource Usage**
   - Memory usage (< 80% threshold)
   - Disk usage (< 90% threshold)
   - File descriptor usage

### Prometheus Metrics

Enable Prometheus plugin for metrics export:

```bash
rabbitmq-plugins enable rabbitmq_prometheus
```

Metrics endpoint: `http://localhost:15692/metrics`

## Security Configuration

### Authentication

```yaml
Default user: disabled
Auth backend: LDAP or OAuth2
Password policy:
  min_length: 12
  require_uppercase: true
  require_number: true
  require_special: true
```

### Authorization (Permissions)

#### Publisher User
```yaml
User: albion-publisher
Permissions:
  Configure: ^$
  Write: ^albion\.events$
  Read: ^$
```

#### Consumer Users
```yaml
User: albion-consumer-{service}
Permissions:
  Configure: ^{service}\..*
  Write: ^$
  Read: ^(albion\.events|{service}\.).*
```

### TLS Configuration

```yaml
ssl_options:
  cacertfile: /path/to/ca_certificate.pem
  certfile: /path/to/server_certificate.pem
  keyfile: /path/to/server_key.pem
  verify: verify_peer
  fail_if_no_peer_cert: true
  versions: ['tlsv1.2', 'tlsv1.3']
```

## Backup and Recovery

### Backup Strategy

1. **Configuration Backup**
   - Export definitions daily: `rabbitmqctl export_definitions`
   - Store in version control

2. **Message Backup**
   - Use queue mirroring for HA
   - Periodic snapshots for critical queues

3. **Disaster Recovery**
   - RPO: 1 hour
   - RTO: 15 minutes
   - Automated failover to standby cluster

### Recovery Procedures

1. **Queue Recovery**
```bash
# Import definitions
rabbitmqctl import_definitions /backup/definitions.json

# Verify queues
rabbitmqctl list_queues name messages consumers
```

2. **Message Recovery**
```bash
# Use shovel plugin to move messages from backup
rabbitmq-plugins enable rabbitmq_shovel
rabbitmq-plugins enable rabbitmq_shovel_management
```

## Maintenance Windows

### Scheduled Maintenance

- **Weekly**: Queue statistics reset (Sunday 00:00 UTC)
- **Monthly**: Connection pool recycling (First Sunday 02:00 UTC)
- **Quarterly**: Cluster node rolling updates

### Maintenance Mode

Enable maintenance mode to pause publishers:

```bash
rabbitmqctl set_parameter federation-upstream maintenance-mode '{"uri":"amqp://localhost","expires":3600000}'
```

## Troubleshooting Guide

### Common Issues

1. **High Memory Usage**
   - Check for unconsumed messages
   - Review queue TTL settings
   - Enable lazy queues for large queues

2. **Slow Publishing**
   - Check publisher confirms
   - Review network latency
   - Optimize batch sizes

3. **Message Loss**
   - Verify publisher confirms enabled
   - Check DLQ for failed messages
   - Review consumer acknowledgments

### Debug Commands

```bash
# Check queue details
rabbitmqctl list_queues name messages consumers memory

# Check connections
rabbitmqctl list_connections name state

# Check channel details  
rabbitmqctl list_channels connection messages_unacknowledged

# Trace specific routing key
rabbitmqctl trace_on
rabbitmqctl trace_off
```

## Migration Guide

### Version Upgrades

When upgrading event versions (e.g., V1 to V2):

1. **Dual Publishing Phase** (2 weeks)
   - Publish to both V1 and V2 routing keys
   - Monitor consumer adoption

2. **Consumer Migration** (2 weeks)
   - Update consumers to handle V2
   - Maintain backward compatibility

3. **Deprecation** (1 week)
   - Stop publishing V1
   - Monitor for errors

4. **Cleanup**
   - Remove V1 bindings
   - Archive V1 documentation

## Appendix

### Sample Connection String

```
amqp://username:password@hostname:5672/vhost
```

### Sample Docker Compose

```yaml
version: '3.8'
services:
  rabbitmq:
    image: rabbitmq:3.12-management-alpine
    hostname: rabbitmq
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      RABBITMQ_DEFAULT_USER: admin
      RABBITMQ_DEFAULT_PASS: admin
      RABBITMQ_DEFAULT_VHOST: /
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
      - ./rabbitmq.conf:/etc/rabbitmq/rabbitmq.conf
      - ./definitions.json:/etc/rabbitmq/definitions.json
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "ping"]
      interval: 30s
      timeout: 10s
      retries: 5

volumes:
  rabbitmq_data:
```

### References

- [RabbitMQ Documentation](https://www.rabbitmq.com/documentation.html)
- [Topic Exchange Tutorial](https://www.rabbitmq.com/tutorials/tutorial-five-python.html)
- [High Availability Guide](https://www.rabbitmq.com/ha.html)
- [Production Checklist](https://www.rabbitmq.com/production-checklist.html)