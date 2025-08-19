using System;
using MessagePack;

namespace Albion.Events.V1;

[MessagePackObject(true)]
public sealed class ClusterObjectsLoadedV1
{
    public required string EventId { get; init; }
    public required DateTimeOffset ObservedAt { get; init; }
    public ClusterObjectiveV1[] Objectives { get; init; } = Array.Empty<ClusterObjectiveV1>();
}

[MessagePackObject(true)]
public sealed class ClusterObjectiveV1
{
    public required int Id { get; init; }
    public int Charge { get; init; }
    public float X { get; init; }
    public float Y { get; init; }
    public required string Type { get; init; }
    public DateTime Timer { get; init; }
}
