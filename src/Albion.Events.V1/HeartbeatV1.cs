using System;
using MessagePack;

namespace Albion.Events.V1;

/// <summary>
/// Periodic heartbeat to indicate system is alive
/// </summary>
[MessagePackObject(true)]
public sealed class HeartbeatV1
{
    public required string EventId { get; init; }
    public required DateTimeOffset ObservedAt { get; init; }
    public required string SnifferId { get; init; }
    public required string Version { get; init; }
    public required int Uptime { get; init; }
    public required int EventsProcessed { get; init; }
    public required int PacketsCaptured { get; init; }
}