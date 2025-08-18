using System.ComponentModel.DataAnnotations;

namespace AlbionOnlineSniffer.Options;

/// <summary>
/// Settings for packet parsing and processing
/// </summary>
public sealed class ParsingSettings
{
    /// <summary>
    /// Provider type for binary dumps (FileSystem, Embedded, Http)
    /// </summary>
    [Required]
    public string BinDumpProvider { get; set; } = "FileSystem";

    /// <summary>
    /// Provider type for item metadata (FileSystem, Embedded, Http)
    /// </summary>
    [Required]
    public string ItemMetadataProvider { get; set; } = "FileSystem";

    /// <summary>
    /// Base path for binary dumps (when using FileSystem provider)
    /// </summary>
    public string BinDumpsPath { get; set; } = "ao-bin-dumps";

    /// <summary>
    /// Base path for item metadata (when using FileSystem provider)
    /// </summary>
    public string ItemsPath { get; set; } = "ITEMS";

    /// <summary>
    /// URL for remote dumps (when using Http provider)
    /// </summary>
    public string? RemoteDumpsUrl { get; set; }

    /// <summary>
    /// URL for remote item metadata (when using Http provider)
    /// </summary>
    public string? RemoteItemsUrl { get; set; }

    /// <summary>
    /// Enable caching for remote resources
    /// </summary>
    public bool EnableRemoteCache { get; set; } = true;

    /// <summary>
    /// Cache directory for remote resources
    /// </summary>
    public string CacheDirectory { get; set; } = ".cache";

    /// <summary>
    /// Cache expiration in hours
    /// </summary>
    [Range(1, 168)] // 1 hour to 1 week
    public int CacheExpirationHours { get; set; } = 24;

    /// <summary>
    /// Enable automatic reload of dumps when changed
    /// </summary>
    public bool AutoReloadDumps { get; set; } = false;

    /// <summary>
    /// Version of dumps to use (latest, specific date, etc.)
    /// </summary>
    public string DumpsVersion { get; set; } = "latest";

    /// <summary>
    /// Enable detailed parsing logs
    /// </summary>
    public bool EnableDetailedLogs { get; set; } = false;

    /// <summary>
    /// Maximum parallel parsing threads
    /// </summary>
    [Range(1, 16)]
    public int MaxParallelism { get; set; } = 4;
}