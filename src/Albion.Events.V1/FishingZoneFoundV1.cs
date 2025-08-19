using System;
using MessagePack;

namespace Albion.Events.V1;

[MessagePackObject(true)]
public sealed class FishingZoneFoundV1
{
    public required string EventId { get; init; }
    public required DateTimeOffset ObservedAt { get; init; }
    public required int Id { get; init; }
    public float X { get; init; }
    public float Y { get; init; }
}
