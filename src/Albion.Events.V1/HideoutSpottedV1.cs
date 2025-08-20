using System;
using MessagePack;

namespace Albion.Events.V1;

/// <summary>
/// Contract for when a hideout is discovered
/// </summary>
[MessagePackObject(true)]
public sealed class HideoutSpottedV1
{
    public required string EventId { get; init; }
    public required DateTimeOffset ObservedAt { get; init; }
    public required int HideoutId { get; init; }
    public required string GuildName { get; init; }
    public string? AllianceName { get; init; }
    public float X { get; init; }
    public float Y { get; init; }
    public int Tier { get; init; }
}