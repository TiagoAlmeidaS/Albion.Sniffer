using System;
using MessagePack;

namespace Albion.Events.V1;

[MessagePackObject(true)]
public sealed class MobSpawnedV1
{
	public required string EventId { get; init; }
	public required DateTimeOffset ObservedAt { get; init; }
	public required int MobId { get; init; }
	public required int TypeId { get; init; }
	public int Tier { get; init; }
	public float X { get; init; }
	public float Y { get; init; }
	public float Health { get; init; }
	public float MaxHealth { get; init; }
}
