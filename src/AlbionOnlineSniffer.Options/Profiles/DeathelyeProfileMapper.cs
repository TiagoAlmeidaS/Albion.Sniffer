using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace AlbionOnlineSniffer.Options.Profiles;

/// <summary>
/// Maps deatheye configuration to Sniffer profiles
/// </summary>
public interface IDeathelyeProfileMapper
{
    /// <summary>
    /// Loads profiles from deatheye settings directory
    /// </summary>
    Task<List<ProfileOptions>> LoadDeathelyeProfilesAsync(string settingsPath);
    
    /// <summary>
    /// Maps deatheye settings to profile options
    /// </summary>
    ProfileOptions MapDeathelyeSettings(DeathelyeSettings settings);
}

/// <summary>
/// Deatheye settings structure
/// </summary>
public class DeathelyeSettings
{
    public string? Name { get; set; }
    public Dictionary<string, bool>? Features { get; set; }
    public Dictionary<string, double>? Thresholds { get; set; }
    public TierSettings? Tiers { get; set; }
    public FilterSettings? Filters { get; set; }
}

/// <summary>
/// Tier settings from deatheye
/// </summary>
public class TierSettings
{
    public string? Palette { get; set; }
    public Dictionary<int, string>? Colors { get; set; }
}

/// <summary>
/// Filter settings from deatheye
/// </summary>
public class FilterSettings
{
    public List<string>? HighlightGuilds { get; set; }
    public List<string>? HighlightAlliances { get; set; }
    public List<string>? IgnoreGuilds { get; set; }
    public List<string>? IgnoreAlliances { get; set; }
    public List<string>? TrackPlayers { get; set; }
}

/// <summary>
/// Default implementation of deatheye profile mapper
/// </summary>
public class DeathelyeProfileMapper : IDeathelyeProfileMapper
{
    private readonly ILogger<DeathelyeProfileMapper> _logger;
    
    public DeathelyeProfileMapper(ILogger<DeathelyeProfileMapper> logger)
    {
        _logger = logger;
    }
    
    public async Task<List<ProfileOptions>> LoadDeathelyeProfilesAsync(string settingsPath)
    {
        var profiles = new List<ProfileOptions>();
        
        if (!Directory.Exists(settingsPath))
        {
            _logger.LogWarning("Deatheye settings path not found: {Path}", settingsPath);
            return profiles;
        }
        
        // Look for JSON files in Settings directory
        var settingsFiles = Directory.GetFiles(settingsPath, "*.json", SearchOption.TopDirectoryOnly);
        
        foreach (var file in settingsFiles)
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                var settings = JsonSerializer.Deserialize<DeathelyeSettings>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (settings != null)
                {
                    var profile = MapDeathelyeSettings(settings);
                    profiles.Add(profile);
                    _logger.LogInformation("Loaded deatheye profile: {Name} from {File}", 
                        profile.Name, Path.GetFileName(file));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load deatheye settings from {File}", file);
            }
        }
        
        // Add default profiles if none found
        if (profiles.Count == 0)
        {
            profiles.AddRange(GetDefaultProfiles());
        }
        
        return profiles;
    }
    
    public ProfileOptions MapDeathelyeSettings(DeathelyeSettings settings)
    {
        var profile = new ProfileOptions
        {
            Name = settings.Name ?? "imported",
            Description = $"Imported from deatheye settings",
            TierPalette = settings.Tiers?.Palette ?? "classic",
            FeatureToggles = MapFeatureToggles(settings.Features),
            Thresholds = MapThresholds(settings.Thresholds),
            Filters = MapFilters(settings.Filters),
            Priority = 5
        };
        
        return profile;
    }
    
    private Dictionary<string, bool> MapFeatureToggles(Dictionary<string, bool>? features)
    {
        var toggles = new Dictionary<string, bool>
        {
            ["ShowHideouts"] = true,
            ["ShowDungeons"] = true,
            ["ShowResources"] = true,
            ["ShowPlayers"] = true,
            ["ShowMobs"] = true,
            ["ShowChests"] = true,
            ["AlertOnPlayerProximity"] = false,
            ["TrackGuildMembers"] = false,
            ["TrackAlliances"] = false
        };
        
        if (features != null)
        {
            // Map deatheye feature names to our feature names
            foreach (var feature in features)
            {
                var mappedName = MapFeatureName(feature.Key);
                if (!string.IsNullOrEmpty(mappedName))
                {
                    toggles[mappedName] = feature.Value;
                }
            }
        }
        
        return toggles;
    }
    
    private string? MapFeatureName(string deathelyeFeature)
    {
        return deathelyeFeature.ToLowerInvariant() switch
        {
            "hideouts" => "ShowHideouts",
            "dungeons" => "ShowDungeons",
            "resources" => "ShowResources",
            "players" => "ShowPlayers",
            "mobs" => "ShowMobs",
            "chests" => "ShowChests",
            "playeralert" => "AlertOnPlayerProximity",
            "trackguild" => "TrackGuildMembers",
            "trackalliance" => "TrackAlliances",
            _ => null
        };
    }
    
    private Dictionary<string, double> MapThresholds(Dictionary<string, double>? thresholds)
    {
        var mapped = new Dictionary<string, double>
        {
            ["PlayerDistanceWarn"] = 100.0,
            ["ResourceMinimumTier"] = 4.0,
            ["MobMinimumTier"] = 4.0,
            ["ChestMinimumTier"] = 4.0,
            ["MaxTrackingDistance"] = 500.0
        };
        
        if (thresholds != null)
        {
            foreach (var threshold in thresholds)
            {
                var mappedName = MapThresholdName(threshold.Key);
                if (!string.IsNullOrEmpty(mappedName))
                {
                    mapped[mappedName] = threshold.Value;
                }
            }
        }
        
        return mapped;
    }
    
    private string? MapThresholdName(string deathelyeThreshold)
    {
        return deathelyeThreshold.ToLowerInvariant() switch
        {
            "playerdistance" => "PlayerDistanceWarn",
            "resourcetier" => "ResourceMinimumTier",
            "mobtier" => "MobMinimumTier",
            "chesttier" => "ChestMinimumTier",
            "maxdistance" => "MaxTrackingDistance",
            _ => null
        };
    }
    
    private FilterOptions MapFilters(FilterSettings? filters)
    {
        return new FilterOptions
        {
            HighlightGuilds = filters?.HighlightGuilds ?? new List<string>(),
            HighlightAlliances = filters?.HighlightAlliances ?? new List<string>(),
            TrackPlayers = filters?.TrackPlayers ?? new List<string>(),
            IgnoreGuilds = filters?.IgnoreGuilds ?? new List<string>(),
            IgnoreAlliances = filters?.IgnoreAlliances ?? new List<string>(),
            TrackItems = new List<string>()
        };
    }
    
    private List<ProfileOptions> GetDefaultProfiles()
    {
        return new List<ProfileOptions>
        {
            new ProfileOptions
            {
                Name = "default",
                Description = "Default profile for general use",
                TierPalette = "classic",
                Priority = 5
            },
            new ProfileOptions
            {
                Name = "zvz",
                Description = "Profile optimized for ZvZ combat",
                TierPalette = "vibrant",
                FeatureToggles = new Dictionary<string, bool>
                {
                    ["ShowHideouts"] = false,
                    ["ShowDungeons"] = false,
                    ["ShowResources"] = false,
                    ["ShowPlayers"] = true,
                    ["ShowMobs"] = false,
                    ["ShowChests"] = false,
                    ["AlertOnPlayerProximity"] = true,
                    ["TrackGuildMembers"] = true,
                    ["TrackAlliances"] = true
                },
                Thresholds = new Dictionary<string, double>
                {
                    ["PlayerDistanceWarn"] = 150.0,
                    ["MaxTrackingDistance"] = 1000.0
                },
                Priority = 8
            },
            new ProfileOptions
            {
                Name = "gank",
                Description = "Profile optimized for ganking",
                TierPalette = "minimal",
                FeatureToggles = new Dictionary<string, bool>
                {
                    ["ShowHideouts"] = true,
                    ["ShowDungeons"] = true,
                    ["ShowResources"] = false,
                    ["ShowPlayers"] = true,
                    ["ShowMobs"] = false,
                    ["ShowChests"] = false,
                    ["AlertOnPlayerProximity"] = true,
                    ["TrackGuildMembers"] = false,
                    ["TrackAlliances"] = false
                },
                Thresholds = new Dictionary<string, double>
                {
                    ["PlayerDistanceWarn"] = 75.0,
                    ["MaxTrackingDistance"] = 300.0
                },
                Priority = 7
            }
        };
    }
}