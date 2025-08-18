using System.ComponentModel.DataAnnotations;

namespace AlbionOnlineSniffer.Options;

/// <summary>
/// Settings for event publishing and distribution
/// </summary>
public sealed class PublishingSettings
{
    /// <summary>
    /// Publisher type (RabbitMQ, Redis, InMemory)
    /// </summary>
    [Required]
    public string PublisherType { get; set; } = "RabbitMQ";

    /// <summary>
    /// Connection string for the publisher
    /// </summary>
    [Required]
    public string ConnectionString { get; set; } = "amqp://guest:guest@localhost:5672";

    /// <summary>
    /// Exchange or channel name
    /// </summary>
    [Required]
    public string Exchange { get; set; } = "albion.sniffer";

    /// <summary>
    /// Enable message persistence
    /// </summary>
    public bool PersistentMessages { get; set; } = true;

    /// <summary>
    /// Message serialization format (MessagePack, Json)
    /// </summary>
    public string SerializationFormat { get; set; } = "MessagePack";

    /// <summary>
    /// Enable message compression
    /// </summary>
    public bool EnableCompression { get; set; } = true;

    /// <summary>
    /// Batch size for publishing
    /// </summary>
    [Range(1, 1000)]
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// Batch timeout in milliseconds
    /// </summary>
    [Range(10, 5000)]
    public int BatchTimeoutMs { get; set; } = 100;

    /// <summary>
    /// Retry configuration
    /// </summary>
    [Required]
    public RetrySettings Retry { get; init; } = new();

    /// <summary>
    /// Circuit breaker configuration
    /// </summary>
    [Required]
    public CircuitBreakerSettings CircuitBreaker { get; init; } = new();

    /// <summary>
    /// Enable dead letter queue
    /// </summary>
    public bool EnableDeadLetterQueue { get; set; } = true;

    /// <summary>
    /// Dead letter queue name
    /// </summary>
    public string DeadLetterQueue { get; set; } = "albion.sniffer.dlq";
}

/// <summary>
/// Retry policy settings
/// </summary>
public sealed class RetrySettings
{
    /// <summary>
    /// Maximum number of retry attempts
    /// </summary>
    [Range(0, 10)]
    public int MaxAttempts { get; set; } = 3;

    /// <summary>
    /// Initial delay between retries in milliseconds
    /// </summary>
    [Range(10, 10000)]
    public int InitialDelayMs { get; set; } = 100;

    /// <summary>
    /// Maximum delay between retries in milliseconds
    /// </summary>
    [Range(100, 60000)]
    public int MaxDelayMs { get; set; } = 5000;

    /// <summary>
    /// Use exponential backoff
    /// </summary>
    public bool UseExponentialBackoff { get; set; } = true;

    /// <summary>
    /// Add jitter to retry delays
    /// </summary>
    public bool UseJitter { get; set; } = true;
}

/// <summary>
/// Circuit breaker settings
/// </summary>
public sealed class CircuitBreakerSettings
{
    /// <summary>
    /// Enable circuit breaker
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Failure threshold before opening circuit
    /// </summary>
    [Range(1, 100)]
    public int FailureThreshold { get; set; } = 5;

    /// <summary>
    /// Sampling duration in seconds
    /// </summary>
    [Range(1, 300)]
    public int SamplingDurationSeconds { get; set; } = 60;

    /// <summary>
    /// Duration to keep circuit open in seconds
    /// </summary>
    [Range(1, 300)]
    public int BreakDurationSeconds { get; set; } = 30;

    /// <summary>
    /// Minimum throughput for circuit evaluation
    /// </summary>
    [Range(1, 1000)]
    public int MinimumThroughput { get; set; } = 10;
}