using System;
using MessagePack;

namespace Albion.Events.V1;

[MessagePackObject(true)]
public sealed class HarvestableStateChangedV1
{
    public required string EventId { get; init; }
    public required DateTimeOffset ObservedAt { get; init; }
    public required int Id { get; init; }
    public int Count { get; init; }
    public int Charge { get; init; }
}
