using System.ComponentModel.DataAnnotations;

namespace AlbionOnlineSniffer.Options;

/// <summary>
/// Profile configuration for different use cases (ZvZ, Ganking, etc.)
/// </summary>
public sealed class ProfileOptions
{
    /// <summary>
    /// Profile name identifier
    /// </summary>
    [Required]
    public string Name { get; init; } = "default";

    /// <summary>
    /// Description of the profile's purpose
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Tier color palette to use (classic, vibrant, minimal)
    /// </summary>
    public string TierPalette { get; init; } = "classic";

    /// <summary>
    /// Feature toggles for this profile
    /// </summary>
    public Dictionary<string, bool> FeatureToggles { get; init; } = new()
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

    /// <summary>
    /// Numeric thresholds for various features
    /// </summary>
    public Dictionary<string, double> Thresholds { get; init; } = new()
    {
        ["PlayerDistanceWarn"] = 100.0,
        ["ResourceMinimumTier"] = 4.0,
        ["MobMinimumTier"] = 4.0,
        ["ChestMinimumTier"] = 4.0,
        ["MaxTrackingDistance"] = 500.0
    };

    /// <summary>
    /// Custom filters for this profile
    /// </summary>
    public FilterOptions Filters { get; init; } = new();

    /// <summary>
    /// Priority level for event processing (higher = more important)
    /// </summary>
    [Range(0, 10)]
    public int Priority { get; init; } = 5;
}

/// <summary>
/// Filtering options for events
/// </summary>
public sealed class FilterOptions
{
    /// <summary>
    /// Guild names to highlight
    /// </summary>
    public List<string> HighlightGuilds { get; init; } = new();

    /// <summary>
    /// Alliance names to highlight
    /// </summary>
    public List<string> HighlightAlliances { get; init; } = new();

    /// <summary>
    /// Player names to track specifically
    /// </summary>
    public List<string> TrackPlayers { get; init; } = new();

    /// <summary>
    /// Guild names to ignore
    /// </summary>
    public List<string> IgnoreGuilds { get; init; } = new();

    /// <summary>
    /// Alliance names to ignore
    /// </summary>
    public List<string> IgnoreAlliances { get; init; } = new();

    /// <summary>
    /// Item IDs to track
    /// </summary>
    public List<string> TrackItems { get; init; } = new();
}