using MessagePack;

namespace Albion.Events.V1;

/// <summary>
/// Contract for when a player is observed/spotted in the world
/// </summary>
[MessagePackObject(true)]
public sealed class PlayerSpottedV1
{
    public required string EventId { get; init; }
    public required DateTimeOffset ObservedAt { get; init; }
    public required string Cluster { get; init; }
    public required string Region { get; init; }

    public required int PlayerId { get; init; }
    public required string PlayerName { get; init; }
    public string? GuildName { get; init; }
    public string? AllianceName { get; init; }

    public float X { get; init; }
    public float Y { get; init; }
    public int Tier { get; init; }
}

