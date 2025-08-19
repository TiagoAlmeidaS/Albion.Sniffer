using System;
using MessagePack;

namespace Albion.Events.V1;

[MessagePackObject(true)]
public sealed class FlaggingFinishedV1
{
    public required string EventId { get; init; }
    public required DateTimeOffset ObservedAt { get; init; }
    public required int Id { get; init; }
    public string Faction { get; init; } = string.Empty;
}
