using System;
using MessagePack;

namespace Albion.Events.V1;

[MessagePackObject(true)]
public sealed class HealthUpdatedV1
{
    public required string EventId { get; init; }
    public required DateTimeOffset ObservedAt { get; init; }
    public required int Id { get; init; }
    public required float Health { get; init; }
    public required float MaxHealth { get; init; }
    public float Energy { get; init; }
}
