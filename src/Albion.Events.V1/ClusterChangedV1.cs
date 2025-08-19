using System;
using MessagePack;

namespace Albion.Events.V1;

[MessagePackObject(true)]
public sealed class ClusterChangedV1
{
    public required string EventId { get; init; }
    public required DateTimeOffset ObservedAt { get; init; }
    public required string LocationId { get; init; }
    public required string Type { get; init; }
    public string? ClusterId { get; init; }
    public string? ClusterName { get; init; }
    public string? ClusterType { get; init; }
    public int Level { get; init; }
}
