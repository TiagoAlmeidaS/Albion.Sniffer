using System;
using MessagePack;

namespace Albion.Events.V1;

[MessagePackObject(true)]
public sealed class RegenerationChangedV1
{
    public required string EventId { get; init; }
    public required DateTimeOffset ObservedAt { get; init; }
    public required int Id { get; init; }
    public int CurrentHealth { get; init; }
    public int MaxHealth { get; init; }
    public float RegenerationRate { get; init; }
}
