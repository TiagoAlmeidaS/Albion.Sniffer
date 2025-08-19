using System;
using MessagePack;

namespace Albion.Events.V1;

[MessagePackObject(true)]
public sealed class HarvestablesListFoundV1
{
    public required string EventId { get; init; }
    public required DateTimeOffset ObservedAt { get; init; }
    public HarvestableFoundV1[] Harvestables { get; init; } = Array.Empty<HarvestableFoundV1>();
}
