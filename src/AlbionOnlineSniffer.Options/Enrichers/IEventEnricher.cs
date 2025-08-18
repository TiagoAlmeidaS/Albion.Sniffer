using Microsoft.Extensions.Logging;

namespace AlbionOnlineSniffer.Options.Enrichers;

/// <summary>
/// Interface for event enrichment based on profile settings
/// </summary>
public interface IEventEnricher
{
    /// <summary>
    /// Enriches an event with profile-specific data
    /// </summary>
    Task<T> EnrichAsync<T>(T eventData, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// Gets the priority of this enricher
    /// </summary>
    int Priority { get; }
    
    /// <summary>
    /// Gets whether this enricher is enabled
    /// </summary>
    bool IsEnabled { get; }
}

/// <summary>
/// Base class for event enrichers
/// </summary>
public abstract class BaseEventEnricher : IEventEnricher
{
    protected readonly ILogger Logger;
    protected readonly ProfileOptions Profile;
    
    public virtual int Priority { get; }
    public virtual bool IsEnabled { get; protected set; } = true;
    
    protected BaseEventEnricher(ILogger logger, ProfileOptions profile, int priority = 0)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Profile = profile ?? throw new ArgumentNullException(nameof(profile));
        Priority = priority;
    }
    
    public abstract Task<T> EnrichAsync<T>(T eventData, CancellationToken cancellationToken = default) where T : class;
}

/// <summary>
/// Enricher that adds tier colors based on profile palette
/// </summary>
public class TierColorEnricher : BaseEventEnricher
{
    private readonly ITierPaletteManager _paletteManager;
    
    public TierColorEnricher(
        ILogger<TierColorEnricher> logger,
        ProfileOptions profile,
        ITierPaletteManager paletteManager)
        : base(logger, profile, priority: 100)
    {
        _paletteManager = paletteManager ?? throw new ArgumentNullException(nameof(paletteManager));
    }
    
    public override Task<T> EnrichAsync<T>(T eventData, CancellationToken cancellationToken = default)
    {
        if (eventData is ITierColorable colorable)
        {
            var palette = _paletteManager.GetPalette(Profile.TierPalette);
            var color = palette.GetTierColor(colorable.Tier);
            var highlightColor = palette.GetTierHighlightColor(colorable.Tier);
            
            colorable.TierColor = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            colorable.TierHighlightColor = $"#{highlightColor.R:X2}{highlightColor.G:X2}{highlightColor.B:X2}";
            
            Logger.LogTrace("Applied tier colors for T{Tier}: {Color}/{Highlight}", 
                colorable.Tier, colorable.TierColor, colorable.TierHighlightColor);
        }
        
        return Task.FromResult(eventData);
    }
}

/// <summary>
/// Interface for events that can have tier colors
/// </summary>
public interface ITierColorable
{
    int Tier { get; }
    string? TierColor { get; set; }
    string? TierHighlightColor { get; set; }
}

/// <summary>
/// Enricher that filters events based on profile settings
/// </summary>
public class ProfileFilterEnricher : BaseEventEnricher
{
    public ProfileFilterEnricher(
        ILogger<ProfileFilterEnricher> logger,
        ProfileOptions profile)
        : base(logger, profile, priority: 50)
    {
    }
    
    public override Task<T> EnrichAsync<T>(T eventData, CancellationToken cancellationToken = default)
    {
        if (eventData is IFilterable filterable)
        {
            // Apply feature toggles
            if (!Profile.FeatureToggles.GetValueOrDefault($"Show{filterable.EventType}", true))
            {
                filterable.IsFiltered = true;
                filterable.FilterReason = $"{filterable.EventType} disabled in profile";
            }
            
            // Apply tier thresholds
            if (filterable is ITiered tiered)
            {
                var minTier = Profile.Thresholds.GetValueOrDefault($"{filterable.EventType}MinimumTier", 0);
                if (tiered.Tier < minTier)
                {
                    filterable.IsFiltered = true;
                    filterable.FilterReason = $"Tier {tiered.Tier} below minimum {minTier}";
                }
            }
            
            // Apply distance thresholds
            if (filterable is IDistanced distanced)
            {
                var maxDistance = Profile.Thresholds.GetValueOrDefault("MaxTrackingDistance", double.MaxValue);
                if (distanced.Distance > maxDistance)
                {
                    filterable.IsFiltered = true;
                    filterable.FilterReason = $"Distance {distanced.Distance:F0} exceeds maximum {maxDistance:F0}";
                }
            }
            
            // Apply guild/alliance filters
            if (filterable is IGuildTracked guildTracked)
            {
                if (Profile.Filters.IgnoreGuilds.Contains(guildTracked.GuildName, StringComparer.OrdinalIgnoreCase))
                {
                    filterable.IsFiltered = true;
                    filterable.FilterReason = $"Guild {guildTracked.GuildName} in ignore list";
                }
                
                if (Profile.Filters.HighlightGuilds.Contains(guildTracked.GuildName, StringComparer.OrdinalIgnoreCase))
                {
                    guildTracked.IsHighlighted = true;
                    guildTracked.HighlightReason = $"Guild {guildTracked.GuildName} in highlight list";
                }
            }
            
            if (!filterable.IsFiltered)
            {
                Logger.LogTrace("Event passed filters: {EventType}", filterable.EventType);
            }
            else
            {
                Logger.LogTrace("Event filtered: {Reason}", filterable.FilterReason);
            }
        }
        
        return Task.FromResult(eventData);
    }
}

/// <summary>
/// Interface for filterable events
/// </summary>
public interface IFilterable
{
    string EventType { get; }
    bool IsFiltered { get; set; }
    string? FilterReason { get; set; }
}

/// <summary>
/// Interface for events with tier
/// </summary>
public interface ITiered
{
    int Tier { get; }
}

/// <summary>
/// Interface for events with distance
/// </summary>
public interface IDistanced
{
    double Distance { get; }
}

/// <summary>
/// Interface for guild-tracked events
/// </summary>
public interface IGuildTracked
{
    string GuildName { get; }
    string? AllianceName { get; }
    bool IsHighlighted { get; set; }
    string? HighlightReason { get; set; }
}

/// <summary>
/// Enricher that adds proximity alerts
/// </summary>
public class ProximityAlertEnricher : BaseEventEnricher
{
    public ProximityAlertEnricher(
        ILogger<ProximityAlertEnricher> logger,
        ProfileOptions profile)
        : base(logger, profile, priority: 200)
    {
        IsEnabled = profile.FeatureToggles.GetValueOrDefault("AlertOnPlayerProximity", false);
    }
    
    public override Task<T> EnrichAsync<T>(T eventData, CancellationToken cancellationToken = default)
    {
        if (!IsEnabled)
        {
            return Task.FromResult(eventData);
        }
        
        if (eventData is IProximityAlertable alertable && alertable is IDistanced distanced)
        {
            var warnDistance = Profile.Thresholds.GetValueOrDefault("PlayerDistanceWarn", 100.0);
            
            if (distanced.Distance <= warnDistance)
            {
                alertable.ProximityAlert = true;
                alertable.ProximityAlertLevel = distanced.Distance switch
                {
                    <= 25 => "critical",
                    <= 50 => "high",
                    <= 75 => "medium",
                    _ => "low"
                };
                
                Logger.LogDebug("Proximity alert triggered: {Distance:F0}m - Level: {Level}", 
                    distanced.Distance, alertable.ProximityAlertLevel);
            }
        }
        
        return Task.FromResult(eventData);
    }
}

/// <summary>
/// Interface for proximity alertable events
/// </summary>
public interface IProximityAlertable
{
    bool ProximityAlert { get; set; }
    string? ProximityAlertLevel { get; set; }
}

/// <summary>
/// Composite enricher that runs multiple enrichers
/// </summary>
public class CompositeEventEnricher : IEventEnricher
{
    private readonly List<IEventEnricher> _enrichers;
    private readonly ILogger<CompositeEventEnricher> _logger;
    
    public int Priority => 0;
    public bool IsEnabled => true;
    
    public CompositeEventEnricher(
        IEnumerable<IEventEnricher> enrichers,
        ILogger<CompositeEventEnricher> logger)
    {
        _enrichers = enrichers.OrderBy(e => e.Priority).ToList();
        _logger = logger;
        
        _logger.LogInformation("Initialized composite enricher with {Count} enrichers", _enrichers.Count);
    }
    
    public async Task<T> EnrichAsync<T>(T eventData, CancellationToken cancellationToken = default) where T : class
    {
        foreach (var enricher in _enrichers.Where(e => e.IsEnabled))
        {
            try
            {
                eventData = await enricher.EnrichAsync(eventData, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in enricher {EnricherType}", enricher.GetType().Name);
            }
        }
        
        return eventData;
    }
}