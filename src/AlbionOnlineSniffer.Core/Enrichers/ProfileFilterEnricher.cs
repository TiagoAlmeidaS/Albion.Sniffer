using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Options;

namespace AlbionOnlineSniffer.Core.Enrichers;

/// <summary>
/// Enricher that filters events based on profile configuration
/// </summary>
public class ProfileFilterEnricher : IEventEnricher
{
    private readonly ILogger<ProfileFilterEnricher> _logger;
    
    public string Name => "ProfileFilter";
    public int Priority => 50; // High priority - should run early
    public bool IsEnabled => true;
    
    public ProfileFilterEnricher(ILogger<ProfileFilterEnricher> logger)
    {
        _logger = logger;
    }
    
    public async Task<object> EnrichAsync(object eventData, ProfileOptions profile, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if the event should be filtered out based on profile settings
            if (ShouldFilterEvent(eventData, profile))
            {
                _logger.LogDebug("Filtering out event {EventType} based on profile {Profile}", 
                    eventData.GetType().Name, profile.Name);
                
                // Return null to indicate the event should be filtered out
                return null!;
            }
            
            return eventData;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to apply profile filter enrichment");
            return eventData;
        }
    }
    
    public bool CanProcess(string eventType)
    {
        // Can process all event types
        return true;
    }
    
    private bool ShouldFilterEvent(object eventData, ProfileOptions profile)
    {
        if (profile.FeatureToggles == null)
            return false;
        
        var eventType = eventData.GetType().Name;
        
        // Check feature toggles based on event type
        if (eventType.Contains("Harvestable") && 
            profile.FeatureToggles.TryGetValue("ShowResources", out var showResources))
        {
            return !showResources;
        }
        
        if (eventType.Contains("Mob") && 
            profile.FeatureToggles.TryGetValue("ShowMobs", out var showMobs))
        {
            return !showMobs;
        }
        
        if (eventType.Contains("Chest") && 
            profile.FeatureToggles.TryGetValue("ShowChests", out var showChests))
        {
            return !showChests;
        }
        
        if (eventType.Contains("Dungeon") && 
            profile.FeatureToggles.TryGetValue("ShowDungeons", out var showDungeons))
        {
            return !showDungeons;
        }
        
        if (eventType.Contains("Player") && 
            profile.FeatureToggles.TryGetValue("ShowPlayers", out var showPlayers))
        {
            return !showPlayers;
        }
        
        // Check tier thresholds
        if (profile.Thresholds != null)
        {
            var tier = ExtractTierFromEvent(eventData);
            if (tier.HasValue)
            {
                if (profile.Thresholds.TryGetValue("ResourceMinimumTier", out var minTier) && 
                    tier.Value < minTier)
                {
                    return true;
                }
                
                if (profile.Thresholds.TryGetValue("MobMinimumTier", out var minMobTier) && 
                    tier.Value < minMobTier)
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    private int? ExtractTierFromEvent(object eventData)
    {
        try
        {
            var tierProperty = eventData.GetType().GetProperty("Tier");
            if (tierProperty?.PropertyType == typeof(int))
            {
                return (int)tierProperty.GetValue(eventData);
            }
            
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
