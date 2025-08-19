using System.Threading;
using System.Threading.Tasks;

namespace AlbionOnlineSniffer.Core.Pipeline;

/// <summary>
/// Interface for the event processing pipeline
/// </summary>
public interface IEventPipeline
{
    /// <summary>
    /// Enqueues an event for processing
    /// </summary>
    /// <param name="eventType">Type of the event</param>
    /// <param name="eventData">Event data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the enqueue operation</returns>
    Task EnqueueAsync(string eventType, object eventData, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Starts the pipeline
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the start operation</returns>
    Task StartAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Stops the pipeline
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the stop operation</returns>
    Task StopAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the current pipeline metrics
    /// </summary>
    PipelineMetrics GetMetrics();
    
    /// <summary>
    /// Gets the current buffer usage percentage
    /// </summary>
    double GetBufferUsagePercentage();
}
