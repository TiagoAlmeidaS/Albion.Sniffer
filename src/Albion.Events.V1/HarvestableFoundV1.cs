using System;
using MessagePack;

namespace Albion.Events.V1;

[MessagePackObject(true)]
public sealed class HarvestableFoundV1
{
    public required string EventId { get; init; }
    public required DateTimeOffset ObservedAt { get; init; }
    public required int Id { get; init; }
    public required int TypeId { get; init; }
    public float X { get; init; }
    public float Y { get; init; }
    public int Tier { get; init; }
    public int Charges { get; init; }
}
