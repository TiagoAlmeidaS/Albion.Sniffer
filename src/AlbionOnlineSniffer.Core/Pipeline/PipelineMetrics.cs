using System;
using System.Threading;

namespace AlbionOnlineSniffer.Core.Pipeline;

/// <summary>
/// Metrics for the event processing pipeline
/// </summary>
public class PipelineMetrics
{
    private long _totalEventsProcessed;
    private long _totalEventsDropped;
    private long _totalErrors;
    private long _totalProcessingTimeMs;
    private long _lastEventTimestamp;
    
    /// <summary>
    /// Total number of events processed
    /// </summary>
    public long TotalEventsProcessed => Interlocked.Read(ref _totalEventsProcessed);
    
    /// <summary>
    /// Total number of events dropped due to buffer overflow
    /// </summary>
    public long TotalEventsDropped => Interlocked.Read(ref _totalEventsDropped);
    
    /// <summary>
    /// Total number of processing errors
    /// </summary>
    public long TotalErrors => Interlocked.Read(ref _totalErrors);
    
    /// <summary>
    /// Total processing time in milliseconds
    /// </summary>
    public long TotalProcessingTimeMs => Interlocked.Read(ref _totalProcessingTimeMs);
    
    /// <summary>
    /// Timestamp of the last processed event
    /// </summary>
    public DateTime LastEventTimestamp => DateTime.FromBinary(Interlocked.Read(ref _lastEventTimestamp));
    
    /// <summary>
    /// Average processing time per event in milliseconds
    /// </summary>
    public double AverageProcessingTimeMs
    {
        get
        {
            var total = TotalEventsProcessed;
            return total > 0 ? (double)TotalProcessingTimeMs / total : 0;
        }
    }
    
    /// <summary>
    /// Error rate as a percentage
    /// </summary>
    public double ErrorRate
    {
        get
        {
            var total = TotalEventsProcessed;
            return total > 0 ? (double)TotalErrors / total * 100 : 0;
        }
    }
    
    /// <summary>
    /// Drop rate as a percentage
    /// </summary>
    public double DropRate
    {
        get
        {
            var total = TotalEventsProcessed + TotalEventsDropped;
            return total > 0 ? (double)TotalEventsDropped / total * 100 : 0;
        }
    }
    
    /// <summary>
    /// Increments the total events processed counter
    /// </summary>
    public void IncrementProcessedEvents()
    {
        Interlocked.Increment(ref _totalEventsProcessed);
        Interlocked.Exchange(ref _lastEventTimestamp, DateTime.UtcNow.ToBinary());
    }
    
    /// <summary>
    /// Increments the total events dropped counter
    /// </summary>
    public void IncrementDroppedEvents()
    {
        Interlocked.Increment(ref _totalEventsDropped);
    }
    
    /// <summary>
    /// Increments the total errors counter
    /// </summary>
    public void IncrementErrors()
    {
        Interlocked.Increment(ref _totalErrors);
    }
    
    /// <summary>
    /// Adds processing time to the total
    /// </summary>
    /// <param name="processingTimeMs">Processing time in milliseconds</param>
    public void AddProcessingTime(long processingTimeMs)
    {
        Interlocked.Add(ref _totalProcessingTimeMs, processingTimeMs);
    }
    
    /// <summary>
    /// Resets all metrics to zero
    /// </summary>
    public void Reset()
    {
        Interlocked.Exchange(ref _totalEventsProcessed, 0);
        Interlocked.Exchange(ref _totalEventsDropped, 0);
        Interlocked.Exchange(ref _totalErrors, 0);
        Interlocked.Exchange(ref _totalProcessingTimeMs, 0);
        Interlocked.Exchange(ref _lastEventTimestamp, 0);
    }
}
