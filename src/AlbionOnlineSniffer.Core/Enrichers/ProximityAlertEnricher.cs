using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Options;

namespace AlbionOnlineSniffer.Core.Enrichers;

/// <summary>
/// Enricher that adds proximity alerts for player events
/// </summary>
public class ProximityAlertEnricher : IEventEnricher
{
    private readonly ILogger<ProximityAlertEnricher> _logger;
    
    public string Name => "ProximityAlert";
    public int Priority => 200; // Lower priority - runs after filtering
    public bool IsEnabled => true;
    
    public ProximityAlertEnricher(ILogger<ProximityAlertEnricher> logger)
    {
        _logger = logger;
    }
    
    public async Task<object> EnrichAsync(object eventData, ProfileOptions profile, CancellationToken cancellationToken = default)
    {
        try
        {
            // Only process player-related events
            if (!IsPlayerEvent(eventData))
                return eventData;
            
            // Check if proximity alerts are enabled
            if (profile.FeatureToggles?.TryGetValue("AlertOnPlayerProximity", out var alertEnabled) == true && 
                !alertEnabled)
            {
                return eventData;
            }
            
            // Get proximity threshold from profile
            var proximityThreshold = profile.Thresholds?.GetValueOrDefault("PlayerDistanceWarn", 100.0) ?? 100.0;
            
            // Extract position information (simplified - in practice you'd have proper position data)
            var position = ExtractPositionFromEvent(eventData);
            if (position.HasValue)
            {
                // Calculate distance to local player (simplified)
                var distance = CalculateDistance(position.Value, GetLocalPlayerPosition());
                
                if (distance <= proximityThreshold)
                {
                    _logger.LogInformation("Player proximity alert: {EventType} at distance {Distance:F1}m (threshold: {Threshold:F1}m)", 
                        eventData.GetType().Name, distance, proximityThreshold);
                    
                    // Add proximity alert information to the event
                    // In practice, you'd add this to a proper event structure
                }
            }
            
            return eventData;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to apply proximity alert enrichment");
            return eventData;
        }
    }
    
    public bool CanProcess(string eventType)
    {
        // Can process player-related events
        return eventType.Contains("Player") || 
               eventType.Contains("Character") ||
               eventType.Contains("Move");
    }
    
    private bool IsPlayerEvent(object eventData)
    {
        var eventType = eventData.GetType().Name;
        return eventType.Contains("Player") || 
               eventType.Contains("Character") ||
               eventType.Contains("Move");
    }
    
    private (float X, float Y)? ExtractPositionFromEvent(object eventData)
    {
        try
        {
            // Try to get position from common properties
            var xProperty = eventData.GetType().GetProperty("X");
            var yProperty = eventData.GetType().GetProperty("Y");
            
            if (xProperty?.PropertyType == typeof(float) && yProperty?.PropertyType == typeof(float))
            {
                var x = (float)xProperty.GetValue(eventData);
                var y = (float)yProperty.GetValue(eventData);
                return (x, y);
            }
            
            // Try Position property
            var positionProperty = eventData.GetType().GetProperty("Position");
            if (positionProperty != null)
            {
                var position = positionProperty.GetValue(eventData);
                if (position != null)
                {
                    var xProp = position.GetType().GetProperty("X");
                    var yProp = position.GetType().GetProperty("Y");
                    
                    if (xProp?.PropertyType == typeof(float) && yProp?.PropertyType == typeof(float))
                    {
                        var x = (float)xProp.GetValue(position);
                        var y = (float)yProp.GetValue(position);
                        return (x, y);
                    }
                }
            }
            
            return null;
        }
        catch
        {
            return null;
        }
    }
    
    private (float X, float Y) GetLocalPlayerPosition()
    {
        // In practice, this would get the current local player position
        // For now, return a default position
        return (0, 0);
    }
    
    private float CalculateDistance((float X, float Y) pos1, (float X, float Y) pos2)
    {
        var dx = pos1.X - pos2.X;
        var dy = pos1.Y - pos2.Y;
        return (float)Math.Sqrt(dx * dx + dy * dy);
    }
}
