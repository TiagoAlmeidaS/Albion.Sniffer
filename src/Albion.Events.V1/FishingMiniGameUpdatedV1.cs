using System;
using MessagePack;

namespace Albion.Events.V1;

[MessagePackObject(true)]
public sealed class FishingMiniGameUpdatedV1 : BaseEventV1
{
    public required string EventId { get; init; }
    public required DateTimeOffset ObservedAt { get; init; }
    public float BobPosition { get; init; }
    public float BarSpeed { get; init; }
    public int Direction { get; init; }
}


