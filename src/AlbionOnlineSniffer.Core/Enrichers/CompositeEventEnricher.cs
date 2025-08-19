using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Options;

namespace AlbionOnlineSniffer.Core.Enrichers;

/// <summary>
/// Composite enricher that orchestrates a chain of individual enrichers
/// </summary>
public class CompositeEventEnricher : IEventEnricher
{
    private readonly IEnumerable<IEventEnricher> _enrichers;
    private readonly ILogger<CompositeEventEnricher> _logger;
    
    public string Name => "Composite";
    public int Priority => 0; // Highest priority - orchestrates others
    public bool IsEnabled => true;
    
    public CompositeEventEnricher(
        IEnumerable<IEventEnricher> enrichers,
        ILogger<CompositeEventEnricher> logger)
    {
        _enrichers = enrichers?.OrderBy(e => e.Priority).ToList() ?? Enumerable.Empty<IEventEnricher>();
        _logger = logger;
    }
    
    public async Task<object> EnrichAsync(object eventData, ProfileOptions profile, CancellationToken cancellationToken = default)
    {
        if (eventData == null)
            return null!;
        
        var eventType = eventData.GetType().Name;
        
        try
        {
            var enriched = eventData;
            
            _logger.LogDebug("Starting enrichment chain for event {EventType} with {EnricherCount} enrichers", 
                eventType, _enrichers.Count());
            
            foreach (var enricher in _enrichers)
            {
                if (!enricher.IsEnabled)
                {
                    _logger.LogTrace("Skipping disabled enricher {EnricherName}", enricher.Name);
                    continue;
                }
                
                if (!enricher.CanProcess(eventType))
                {
                    _logger.LogTrace("Enricher {EnricherName} cannot process event type {EventType}", 
                        enricher.Name, eventType);
                    continue;
                }
                
                try
                {
                    var startTime = DateTime.UtcNow;
                    enriched = await enricher.EnrichAsync(enriched, profile, cancellationToken);
                    var duration = DateTime.UtcNow - startTime;
                    
                    _logger.LogTrace("Enricher {EnricherName} completed in {DurationMs}ms", 
                        enricher.Name, duration.TotalMilliseconds);
                    
                    // If any enricher returns null, the event should be filtered out
                    if (enriched == null)
                    {
                        _logger.LogDebug("Event {EventType} filtered out by enricher {EnricherName}", 
                            eventType, enricher.Name);
                        return null!;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Enricher {EnricherName} failed for event {EventType}", 
                        enricher.Name, eventType);
                    
                    // Continue with other enrichers, but log the failure
                    // In production, you might want to handle this differently
                }
            }
            
            _logger.LogDebug("Enrichment chain completed for event {EventType}", eventType);
            return enriched;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Composite enrichment failed for event {EventType}", eventType);
            return eventData; // Return original data on failure
        }
    }
    
    public bool CanProcess(string eventType)
    {
        // Composite enricher can process all event types
        return true;
    }
    
    /// <summary>
    /// Gets the list of active enrichers
    /// </summary>
    public IEnumerable<IEventEnricher> GetActiveEnrichers()
    {
        return _enrichers.Where(e => e.IsEnabled);
    }
    
    /// <summary>
    /// Gets the list of enrichers that can process a specific event type
    /// </summary>
    public IEnumerable<IEventEnricher> GetEnrichersForEventType(string eventType)
    {
        return _enrichers.Where(e => e.IsEnabled && e.CanProcess(eventType));
    }
}
