using System.ComponentModel.DataAnnotations;

namespace AlbionOnlineSniffer.Options;

/// <summary>
/// Settings for packet capture configuration
/// </summary>
public sealed class PacketCaptureSettings
{
    /// <summary>
    /// Network interface to capture packets from
    /// </summary>
    public string? DeviceName { get; set; }

    /// <summary>
    /// Automatically select the first available device
    /// </summary>
    public bool AutoSelectDevice { get; set; } = true;

    /// <summary>
    /// BPF filter for packet capture
    /// </summary>
    public string Filter { get; set; } = "udp port 5056";

    /// <summary>
    /// Packet capture timeout in milliseconds
    /// </summary>
    [Range(100, 10000)]
    public int TimeoutMs { get; set; } = 1000;

    /// <summary>
    /// Buffer size for packet capture
    /// </summary>
    [Range(1024 * 1024, 100 * 1024 * 1024)]
    public int BufferSize { get; set; } = 10 * 1024 * 1024; // 10MB

    /// <summary>
    /// Enable promiscuous mode
    /// </summary>
    public bool PromiscuousMode { get; set; } = false;

    /// <summary>
    /// Maximum packet size to capture
    /// </summary>
    [Range(64, 65535)]
    public int SnapLength { get; set; } = 65535;

    /// <summary>
    /// Enable packet capture statistics
    /// </summary>
    public bool EnableStatistics { get; set; } = true;

    /// <summary>
    /// Statistics update interval in seconds
    /// </summary>
    [Range(1, 300)]
    public int StatisticsIntervalSeconds { get; set; } = 30;
}