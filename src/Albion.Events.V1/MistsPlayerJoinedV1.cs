using System;
using MessagePack;

namespace Albion.Events.V1;

[MessagePackObject(true)]
public sealed class MistsPlayerJoinedV1
{
    public required string EventId { get; init; }
    public required DateTimeOffset ObservedAt { get; init; }
    public DateTime TimeCycle { get; init; }
}
