using System;

namespace AlbionOnlineSniffer.Options.Obsolete;

/// <summary>
/// Obsolete interfaces marked for deprecation
/// These will be removed in future versions
/// </summary>
public static class ObsoleteMarkers
{
    public const string PacketCaptureSettingsMessage = 
        "PacketCaptureSettings from appsettings root is deprecated. Use Sniffer:PacketCapture instead.";
    
    public const string QueueSettingsMessage = 
        "QueueSettings is deprecated. Use Sniffer:Publishing instead.";
    
    public const string RabbitMQSettingsMessage = 
        "RabbitMQ section is deprecated. Use Sniffer:Publishing with PublisherType='RabbitMQ' instead.";
    
    public const string BinDumpsSettingsMessage = 
        "BinDumps section is deprecated. Use Sniffer:Parsing instead.";
}

/// <summary>
/// Legacy packet capture settings for backward compatibility
/// </summary>
[Obsolete(ObsoleteMarkers.PacketCaptureSettingsMessage)]
public class LegacyPacketCaptureSettings
{
    public string? DeviceName { get; set; }
    public bool AutoSelectDevice { get; set; }
    public string Filter { get; set; } = "";
}

/// <summary>
/// Legacy queue settings for backward compatibility
/// </summary>
[Obsolete(ObsoleteMarkers.QueueSettingsMessage)]
public class LegacyQueueSettings
{
    public string PublisherType { get; set; } = "";
    public string ConnectionString { get; set; } = "";
}

/// <summary>
/// Legacy RabbitMQ settings for backward compatibility
/// </summary>
[Obsolete(ObsoleteMarkers.RabbitMQSettingsMessage)]
public class LegacyRabbitMQSettings
{
    public string ConnectionString { get; set; } = "";
    public string Exchange { get; set; } = "";
}

/// <summary>
/// Legacy BinDumps settings for backward compatibility
/// </summary>
[Obsolete(ObsoleteMarkers.BinDumpsSettingsMessage)]
public class LegacyBinDumpsSettings
{
    public string BasePath { get; set; } = "";
    public bool Enabled { get; set; }
    public bool AutoReload { get; set; }
}