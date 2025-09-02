using System;
using MessagePack;

namespace Albion.Events.V1;

[MessagePackObject(true)]
public sealed class FishingFinishedV1 : BaseEventV1
{
    public required string EventId { get; init; }
    public required DateTimeOffset ObservedAt { get; init; }
    public int Result { get; init; }
    public long ItemId { get; init; }
    public int Quantity { get; init; }
    public int Rarity { get; init; }
}


