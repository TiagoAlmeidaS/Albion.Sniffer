using System.Threading;
using System.Threading.Tasks;

namespace AlbionOnlineSniffer.Core.Workers;

/// <summary>
/// Interface for pipeline workers that process events
/// </summary>
public interface IPipelineWorker
{
    /// <summary>
    /// Worker ID for identification
    /// </summary>
    int WorkerId { get; }
    
    /// <summary>
    /// Whether the worker is currently running
    /// </summary>
    bool IsRunning { get; }
    
    /// <summary>
    /// Starts the worker
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the start operation</returns>
    Task StartAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Stops the worker
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the stop operation</returns>
    Task StopAsync(CancellationToken cancellationToken = default);
}
