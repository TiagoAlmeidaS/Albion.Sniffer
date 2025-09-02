using System;
using MessagePack;

namespace Albion.Events.V1;

[MessagePackObject(true)]
public sealed class FishingStartedV1 : BaseEventV1
{
    public required string EventId { get; init; }
    public required DateTimeOffset ObservedAt { get; init; }
    public int RodId { get; init; }
    public int BaitId { get; init; }
    public float TargetX { get; init; }
    public float TargetY { get; init; }
}


