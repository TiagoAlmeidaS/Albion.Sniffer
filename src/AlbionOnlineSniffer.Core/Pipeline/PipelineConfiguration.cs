namespace AlbionOnlineSniffer.Core.Pipeline;

/// <summary>
/// Configuration for the event processing pipeline
/// </summary>
public class PipelineConfiguration
{
    /// <summary>
    /// Size of the channel buffer
    /// </summary>
    public int BufferSize { get; set; } = 10000;
    
    /// <summary>
    /// Number of worker tasks
    /// </summary>
    public int WorkerCount { get; set; } = 4;
    
    /// <summary>
    /// Maximum concurrency for processing
    /// </summary>
    public int MaxConcurrency { get; set; } = 8;
    
    /// <summary>
    /// Number of retry attempts for failed operations
    /// </summary>
    public int RetryAttempts { get; set; } = 3;
    
    /// <summary>
    /// Delay between retry attempts in milliseconds
    /// </summary>
    public int RetryDelayMs { get; set; } = 1000;
    
    /// <summary>
    /// Circuit breaker threshold (number of consecutive failures)
    /// </summary>
    public int CircuitBreakerThreshold { get; set; } = 5;
    
    /// <summary>
    /// Circuit breaker duration in milliseconds
    /// </summary>
    public int CircuitBreakerDurationMs { get; set; } = 30000;
    
    /// <summary>
    /// Timeout for operations in milliseconds
    /// </summary>
    public int TimeoutMs { get; set; } = 5000;
    
    /// <summary>
    /// Whether to enable backpressure handling
    /// </summary>
    public bool EnableBackpressure { get; set; } = true;
    
    /// <summary>
    /// Whether to enable circuit breaker
    /// </summary>
    public bool EnableCircuitBreaker { get; set; } = true;
    
    /// <summary>
    /// Whether to enable retry policies
    /// </summary>
    public bool EnableRetryPolicies { get; set; } = true;
}
