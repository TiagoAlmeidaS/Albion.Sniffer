using MessagePack;

namespace Albion.Events.V1;

/// <summary>
/// Contract for when a player requests to move
/// </summary>
[MessagePackObject(true)]
public sealed class PlayerMoveRequestV1
{
    public required string EventId { get; init; }
    public required DateTimeOffset ObservedAt { get; init; }
    public required string Cluster { get; init; }
    public required string Region { get; init; }

    public required int PlayerId { get; init; }
    
    public float FromX { get; init; }
    public float FromY { get; init; }
    public float ToX { get; init; }
    public float ToY { get; init; }
    
    public float Speed { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}
