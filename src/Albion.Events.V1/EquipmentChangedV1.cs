using System;
using MessagePack;

namespace Albion.Events.V1;

[MessagePackObject(true)]
public sealed class EquipmentChangedV1
{
    public required string EventId { get; init; }
    public required DateTimeOffset ObservedAt { get; init; }
    public required int Id { get; init; }
    public int[] Equipments { get; init; } = Array.Empty<int>();
    public int[] Spells { get; init; } = Array.Empty<int>();
}
