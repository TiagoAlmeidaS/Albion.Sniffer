using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Options.Profiles;
using AlbionOnlineSniffer.Core.Enrichers;
using AlbionOnlineSniffer.Core.Pipeline;

namespace AlbionOnlineSniffer.Core.Workers;

/// <summary>
/// Worker that processes events from the pipeline channel
/// </summary>
public class PipelineWorker : IPipelineWorker
{
    private readonly ILogger _logger;
    private readonly ChannelReader<PipelineItem> _channelReader;
    private readonly IProfileManager _profileManager;
    private readonly IEventEnricher _compositeEnricher;
    private readonly PipelineMetrics _metrics;
    private readonly SemaphoreSlim _semaphore;
    private readonly int _workerId;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private bool _isRunning;
    
    public int WorkerId => _workerId;
    public bool IsRunning => _isRunning;
    
    public PipelineWorker(
        ILogger logger,
        ChannelReader<PipelineItem> channelReader,
        IProfileManager profileManager,
        IEventEnricher compositeEnricher,
        PipelineMetrics metrics,
        SemaphoreSlim semaphore,
        int workerId)
    {
        _logger = logger;
        _channelReader = channelReader;
        _profileManager = profileManager;
        _compositeEnricher = compositeEnricher;
        _metrics = metrics;
        _semaphore = semaphore;
        _workerId = workerId;
        _cancellationTokenSource = new CancellationTokenSource();
    }
    
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning)
        {
            _logger.LogWarning("Worker {WorkerId} is already running", _workerId);
            return;
        }
        
        try
        {
            _isRunning = true;
            _logger.LogInformation("Worker {WorkerId} started", _workerId);
            
            // Process events from the channel
            await foreach (var item in _channelReader.ReadAllAsync(_cancellationTokenSource.Token))
            {
                try
                {
                    // Acquire semaphore to limit concurrency
                    await _semaphore.WaitAsync(_cancellationTokenSource.Token);
                    
                    // Process the event asynchronously
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await ProcessEventAsync(item);
                        }
                        finally
                        {
                            _semaphore.Release();
                        }
                    }, _cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    // Worker is being stopped
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Worker {WorkerId} error processing event {EventType}", 
                        _workerId, item.EventType);
                    _metrics.IncrementErrors();
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Worker {WorkerId} stopped due to cancellation", _workerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Worker {WorkerId} failed", _workerId);
        }
        finally
        {
            _isRunning = false;
            _logger.LogInformation("Worker {WorkerId} stopped", _workerId);
        }
    }
    
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_isRunning)
            return;
        
        try
        {
            _logger.LogDebug("Stopping worker {WorkerId}", _workerId);
            _cancellationTokenSource.Cancel();
            
            // Wait for the worker to complete
            var timeout = TimeSpan.FromSeconds(30);
            var stopTask = Task.Run(() => _isRunning ? Task.CompletedTask : Task.CompletedTask);
            
            if (await Task.WhenAny(stopTask, Task.Delay(timeout, cancellationToken)) == stopTask)
            {
                _logger.LogDebug("Worker {WorkerId} stopped gracefully", _workerId);
            }
            else
            {
                _logger.LogWarning("Worker {WorkerId} did not stop within timeout", _workerId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping worker {WorkerId}", _workerId);
        }
    }
    
    private async Task ProcessEventAsync(PipelineItem item)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogTrace("Worker {WorkerId} processing event {EventType} (correlation: {CorrelationId})", 
                _workerId, item.EventType, item.CorrelationId);
            
            // Get current profile
            var profile = _profileManager.CurrentProfile;
            
            // Apply enrichment chain
            var enrichedEvent = await _compositeEnricher.EnrichAsync(
                item.EventData, 
                profile, 
                _cancellationTokenSource.Token);
            
            // If enrichment returned null, the event was filtered out
            if (enrichedEvent == null)
            {
                _logger.LogDebug("Event {EventType} filtered out by enrichment chain", item.EventType);
                return;
            }
            
            // TODO: Publish enriched event to queue publishers
            // This will be implemented when we integrate with the existing queue system
            
            _logger.LogTrace("Worker {WorkerId} completed processing event {EventType}", 
                _workerId, item.EventType);
            
            // Update metrics
            _metrics.IncrementProcessedEvents();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Worker {WorkerId} failed to process event {EventType}", 
                _workerId, item.EventType);
            _metrics.IncrementErrors();
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _metrics.AddProcessingTime(stopwatch.ElapsedMilliseconds);
        }
    }
}
