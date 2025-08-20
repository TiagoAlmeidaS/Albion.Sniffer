using System;
using MessagePack;

namespace Albion.Events.V1;

/// <summary>
/// Emitted when event publishing fails after retries
/// </summary>
[MessagePackObject(true)]
public sealed class PublishFailureV1
{
    public required string EventId { get; init; }
    public required DateTimeOffset ObservedAt { get; init; }
    public required string OriginalEventType { get; init; }
    public required string OriginalEventId { get; init; }
    public required string Error { get; init; }
    public required int RetryCount { get; init; }
}