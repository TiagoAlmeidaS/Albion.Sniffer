using System.ComponentModel.DataAnnotations;

namespace AlbionOnlineSniffer.Options;

/// <summary>
/// Root configuration options for the Albion Online Sniffer
/// </summary>
public sealed class SnifferOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "Sniffer";

    /// <summary>
    /// Packet capture settings
    /// </summary>
    [Required]
    public PacketCaptureSettings PacketCapture { get; init; } = new();

    /// <summary>
    /// Parsing settings for packet processing
    /// </summary>
    [Required]
    public ParsingSettings Parsing { get; init; } = new();

    /// <summary>
    /// Publishing settings for event distribution
    /// </summary>
    [Required]
    public PublishingSettings Publishing { get; init; } = new();

    /// <summary>
    /// Available profiles for different use cases
    /// </summary>
    public List<ProfileOptions> Profiles { get; init; } = new() { new ProfileOptions() };

    /// <summary>
    /// Currently active profile name
    /// </summary>
    public string ActiveProfile { get; set; } = "default";

    /// <summary>
    /// Gets the active profile configuration
    /// </summary>
    public ProfileOptions GetActiveProfile()
    {
        return Profiles.FirstOrDefault(p => p.Name.Equals(ActiveProfile, StringComparison.OrdinalIgnoreCase))
            ?? Profiles.FirstOrDefault(p => p.Name.Equals("default", StringComparison.OrdinalIgnoreCase))
            ?? new ProfileOptions();
    }
}