using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AlbionOnlineSniffer.Options.Profiles;
using AlbionOnlineSniffer.Core.Enrichers;
using AlbionOnlineSniffer.Core.Workers;

namespace AlbionOnlineSniffer.Core.Pipeline;

/// <summary>
/// Event processing pipeline using Channels for async processing
/// </summary>
public class EventPipeline : IEventPipeline, IDisposable
{
    private readonly ILogger<EventPipeline> _logger;
    private readonly PipelineConfiguration _config;
    private readonly IProfileManager _profileManager;
    private readonly IEventEnricher _compositeEnricher;
    private readonly Channel<PipelineItem> _channel;
    private readonly PipelineMetrics _metrics;
    private readonly List<PipelineWorker> _workers;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly SemaphoreSlim _semaphore;
    private bool _isStarted;
    private bool _disposed;
    
    public EventPipeline(
        ILogger<EventPipeline> logger,
        IOptions<PipelineConfiguration> config,
        IProfileManager profileManager,
        IEventEnricher compositeEnricher)
    {
        _logger = logger;
        _config = config.Value;
        _profileManager = profileManager;
        _compositeEnricher = compositeEnricher;
        _metrics = new PipelineMetrics();
        _cancellationTokenSource = new CancellationTokenSource();
        _semaphore = new SemaphoreSlim(_config.MaxConcurrency, _config.MaxConcurrency);
        
        // Create bounded channel with backpressure handling
        var channelOptions = new BoundedChannelOptions(_config.BufferSize)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = true
        };
        
        _channel = Channel.CreateBounded<PipelineItem>(channelOptions);
        _workers = new List<PipelineWorker>();
        
        _logger.LogInformation("EventPipeline initialized with buffer size {BufferSize}, workers {WorkerCount}", 
            _config.BufferSize, _config.WorkerCount);
    }
    
    public async Task EnqueueAsync(string eventType, object eventData, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(EventPipeline));
        
        if (string.IsNullOrEmpty(eventType))
            throw new ArgumentException("Event type cannot be null or empty", nameof(eventType));
        
        if (eventData == null)
            throw new ArgumentException("Event data cannot be null", nameof(eventData));
        
        try
        {
            var item = new PipelineItem(eventType, eventData);
            
            // Try to write immediately without blocking
            if (_channel.Writer.TryWrite(item))
            {
                _logger.LogTrace("Event {EventType} enqueued immediately", eventType);
            }
            else
            {
                // Channel is full, apply backpressure
                if (_config.EnableBackpressure)
                {
                    _logger.LogWarning("Channel buffer full, applying backpressure for event {EventType}", eventType);
                    _metrics.IncrementDroppedEvents();
                    
                    // Wait for space in the channel
                    await _channel.Writer.WriteAsync(item, cancellationToken);
                    _logger.LogDebug("Event {EventType} enqueued after backpressure", eventType);
                }
                else
                {
                    // Drop the event if backpressure is disabled
                    _logger.LogWarning("Channel buffer full, dropping event {EventType}", eventType);
                    _metrics.IncrementDroppedEvents();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enqueue event {EventType}", eventType);
            _metrics.IncrementErrors();
            throw;
        }
    }
    
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(EventPipeline));
        
        if (_isStarted)
        {
            _logger.LogWarning("EventPipeline is already started");
            return;
        }
        
        try
        {
            _logger.LogInformation("Starting EventPipeline with {WorkerCount} workers", _config.WorkerCount);
            
            // Create and start workers
            for (int i = 0; i < _config.WorkerCount; i++)
            {
                var worker = new PipelineWorker(
                    _logger,
                    _channel.Reader,
                    _profileManager,
                    _compositeEnricher,
                    _metrics,
                    _semaphore,
                    i);
                
                _workers.Add(worker);
                _ = Task.Run(() => worker.StartAsync(_cancellationTokenSource.Token), cancellationToken);
            }
            
            _isStarted = true;
            _logger.LogInformation("EventPipeline started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start EventPipeline");
            throw;
        }
    }
    
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed || !_isStarted)
            return;
        
        try
        {
            _logger.LogInformation("Stopping EventPipeline");
            
            // Signal workers to stop
            _cancellationTokenSource.Cancel();
            
            // Wait for workers to complete
            var stopTasks = _workers.Select(w => w.StopAsync(cancellationToken));
            await Task.WhenAll(stopTasks);
            
            // Complete the channel
            _channel.Writer.Complete();
            
            _isStarted = false;
            _logger.LogInformation("EventPipeline stopped successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping EventPipeline");
            throw;
        }
    }
    
    public PipelineMetrics GetMetrics()
    {
        return _metrics;
    }
    
    public double GetBufferUsagePercentage()
    {
        if (_config.BufferSize == 0)
            return 0;
        
        var currentCount = _channel.Reader.Count;
        return (double)currentCount / _config.BufferSize * 100;
    }
    
    public void Dispose()
    {
        if (_disposed)
            return;
        
        try
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _semaphore?.Dispose();
            
            // Stop workers if not already stopped
            if (_isStarted)
            {
                _ = StopAsync().ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing EventPipeline");
        }
        finally
        {
            _disposed = true;
        }
    }
}
