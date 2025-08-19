using System;
using MessagePack;

namespace Albion.Events.V1;

[MessagePackObject(true)]
public sealed class KeySyncV1
{
    public required string EventId { get; init; }
    public required DateTimeOffset ObservedAt { get; init; }
    public byte[] Code { get; init; } = Array.Empty<byte>();
    public ulong Key { get; init; }
}
