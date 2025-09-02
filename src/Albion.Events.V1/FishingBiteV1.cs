using System;
using MessagePack;

namespace Albion.Events.V1;

[MessagePackObject(true)]
public sealed class FishingBiteV1 : BaseEventV1
{
    public required string EventId { get; init; }
    public required DateTimeOffset ObservedAt { get; init; }
    public long FishId { get; init; }
    public float Difficulty { get; init; }
    public float BiteTime { get; init; }
}


