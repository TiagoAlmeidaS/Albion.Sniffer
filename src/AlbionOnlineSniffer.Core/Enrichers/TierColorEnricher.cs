using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Options;
using AlbionOnlineSniffer.Options.Profiles;

namespace AlbionOnlineSniffer.Core.Enrichers;

/// <summary>
/// Enricher that adds tier-based colors to events
/// </summary>
public class TierColorEnricher : IEventEnricher
{
    private readonly ILogger<TierColorEnricher> _logger;
    private readonly ITierPaletteManager _paletteManager;
    
    public string Name => "TierColor";
    public int Priority => 100;
    public bool IsEnabled => true;
    
    public TierColorEnricher(
        ILogger<TierColorEnricher> logger,
        ITierPaletteManager paletteManager)
    {
        _logger = logger;
        _paletteManager = paletteManager;
    }
    
    public async Task<object> EnrichAsync(object eventData, ProfileOptions profile, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get the tier palette for the current profile
            var palette = _paletteManager.GetPalette(profile.TierPalette);
            
            // Try to extract tier information from the event
            var tier = ExtractTierFromEvent(eventData);
            if (tier.HasValue)
            {
                var color = palette.GetTierColor(tier.Value);
                
                // Add color information to the event
                // This is a simplified example - in practice, you'd add this to a proper event structure
                _logger.LogDebug("Applied tier color {Color} for tier {Tier} to event {EventType}", 
                    color, tier.Value, eventData.GetType().Name);
            }
            
            return eventData;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to apply tier color enrichment");
            return eventData;
        }
    }
    
    public bool CanProcess(string eventType)
    {
        // Can process events that might have tier information
        return eventType.Contains("Harvestable") || 
               eventType.Contains("Mob") || 
               eventType.Contains("Chest") ||
               eventType.Contains("Item");
    }
    
    private int? ExtractTierFromEvent(object eventData)
    {
        // This is a simplified implementation
        // In practice, you'd use reflection or specific event types to extract tier
        try
        {
            // Try to get tier from common properties
            var tierProperty = eventData.GetType().GetProperty("Tier");
            if (tierProperty?.PropertyType == typeof(int))
            {
                return (int)tierProperty.GetValue(eventData);
            }
            
            // Try other common tier property names
            var tierProperty2 = eventData.GetType().GetProperty("ItemTier");
            if (tierProperty2?.PropertyType == typeof(int))
            {
                return (int)tierProperty2.GetValue(eventData);
            }
            
            return null;
        }
        catch
        {
            return null;
        }
    }
}
