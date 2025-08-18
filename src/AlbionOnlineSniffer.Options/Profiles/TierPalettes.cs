using System.Drawing;

namespace AlbionOnlineSniffer.Options.Profiles;

/// <summary>
/// Tier color palette definitions
/// </summary>
public interface ITierPalette
{
    /// <summary>
    /// Gets the color for a specific tier
    /// </summary>
    Color GetTierColor(int tier);
    
    /// <summary>
    /// Gets the highlight color for a specific tier
    /// </summary>
    Color GetTierHighlightColor(int tier);
    
    /// <summary>
    /// Gets the palette name
    /// </summary>
    string Name { get; }
}

/// <summary>
/// Manager for tier palettes
/// </summary>
public interface ITierPaletteManager
{
    /// <summary>
    /// Gets a palette by name
    /// </summary>
    ITierPalette GetPalette(string name);
    
    /// <summary>
    /// Gets all available palette names
    /// </summary>
    IEnumerable<string> AvailablePalettes { get; }
    
    /// <summary>
    /// Registers a custom palette
    /// </summary>
    void RegisterPalette(ITierPalette palette);
}

/// <summary>
/// Base implementation of tier palette
/// </summary>
public abstract class BaseTierPalette : ITierPalette
{
    public abstract string Name { get; }
    
    protected abstract Dictionary<int, Color> TierColors { get; }
    protected abstract Dictionary<int, Color> TierHighlightColors { get; }
    
    public virtual Color GetTierColor(int tier)
    {
        // Clamp tier to valid range (1-8)
        tier = Math.Max(1, Math.Min(8, tier));
        
        return TierColors.TryGetValue(tier, out var color) 
            ? color 
            : Color.Gray;
    }
    
    public virtual Color GetTierHighlightColor(int tier)
    {
        // Clamp tier to valid range (1-8)
        tier = Math.Max(1, Math.Min(8, tier));
        
        return TierHighlightColors.TryGetValue(tier, out var color) 
            ? color 
            : Color.LightGray;
    }
}

/// <summary>
/// Classic tier palette (similar to game colors)
/// </summary>
public class ClassicTierPalette : BaseTierPalette
{
    public override string Name => "classic";
    
    protected override Dictionary<int, Color> TierColors => new()
    {
        { 1, Color.FromArgb(128, 128, 128) }, // Gray - T1
        { 2, Color.FromArgb(255, 255, 255) }, // White - T2
        { 3, Color.FromArgb(0, 255, 0) },     // Green - T3
        { 4, Color.FromArgb(0, 128, 255) },   // Blue - T4
        { 5, Color.FromArgb(128, 0, 255) },   // Purple - T5
        { 6, Color.FromArgb(255, 128, 0) },   // Orange - T6
        { 7, Color.FromArgb(255, 0, 0) },     // Red - T7
        { 8, Color.FromArgb(255, 215, 0) }    // Gold - T8
    };
    
    protected override Dictionary<int, Color> TierHighlightColors => new()
    {
        { 1, Color.FromArgb(192, 192, 192) }, // Light Gray
        { 2, Color.FromArgb(240, 240, 240) }, // Off White
        { 3, Color.FromArgb(128, 255, 128) }, // Light Green
        { 4, Color.FromArgb(128, 192, 255) }, // Light Blue
        { 5, Color.FromArgb(192, 128, 255) }, // Light Purple
        { 6, Color.FromArgb(255, 192, 128) }, // Light Orange
        { 7, Color.FromArgb(255, 128, 128) }, // Light Red
        { 8, Color.FromArgb(255, 235, 128) }  // Light Gold
    };
}

/// <summary>
/// Vibrant tier palette (higher contrast for PvP)
/// </summary>
public class VibrantTierPalette : BaseTierPalette
{
    public override string Name => "vibrant";
    
    protected override Dictionary<int, Color> TierColors => new()
    {
        { 1, Color.FromArgb(64, 64, 64) },     // Dark Gray
        { 2, Color.FromArgb(192, 192, 192) },  // Silver
        { 3, Color.FromArgb(0, 255, 64) },     // Bright Green
        { 4, Color.FromArgb(0, 192, 255) },    // Cyan
        { 5, Color.FromArgb(192, 0, 255) },    // Magenta
        { 6, Color.FromArgb(255, 165, 0) },    // Bright Orange
        { 7, Color.FromArgb(255, 0, 128) },    // Hot Pink
        { 8, Color.FromArgb(255, 255, 0) }     // Yellow
    };
    
    protected override Dictionary<int, Color> TierHighlightColors => new()
    {
        { 1, Color.FromArgb(128, 128, 128) },
        { 2, Color.FromArgb(224, 224, 224) },
        { 3, Color.FromArgb(128, 255, 192) },
        { 4, Color.FromArgb(128, 224, 255) },
        { 5, Color.FromArgb(224, 128, 255) },
        { 6, Color.FromArgb(255, 210, 128) },
        { 7, Color.FromArgb(255, 128, 192) },
        { 8, Color.FromArgb(255, 255, 192) }
    };
}

/// <summary>
/// Minimal tier palette (subtle colors for stealth)
/// </summary>
public class MinimalTierPalette : BaseTierPalette
{
    public override string Name => "minimal";
    
    protected override Dictionary<int, Color> TierColors => new()
    {
        { 1, Color.FromArgb(96, 96, 96) },     // Dark Gray
        { 2, Color.FromArgb(144, 144, 144) },  // Medium Gray
        { 3, Color.FromArgb(96, 144, 96) },    // Muted Green
        { 4, Color.FromArgb(96, 96, 144) },    // Muted Blue
        { 5, Color.FromArgb(144, 96, 144) },   // Muted Purple
        { 6, Color.FromArgb(144, 112, 96) },   // Muted Brown
        { 7, Color.FromArgb(144, 96, 96) },    // Muted Red
        { 8, Color.FromArgb(144, 144, 96) }    // Muted Gold
    };
    
    protected override Dictionary<int, Color> TierHighlightColors => new()
    {
        { 1, Color.FromArgb(128, 128, 128) },
        { 2, Color.FromArgb(176, 176, 176) },
        { 3, Color.FromArgb(128, 176, 128) },
        { 4, Color.FromArgb(128, 128, 176) },
        { 5, Color.FromArgb(176, 128, 176) },
        { 6, Color.FromArgb(176, 144, 128) },
        { 7, Color.FromArgb(176, 128, 128) },
        { 8, Color.FromArgb(176, 176, 128) }
    };
}

/// <summary>
/// Default implementation of tier palette manager
/// </summary>
public class TierPaletteManager : ITierPaletteManager
{
    private readonly Dictionary<string, ITierPalette> _palettes;
    
    public IEnumerable<string> AvailablePalettes => _palettes.Keys;
    
    public TierPaletteManager()
    {
        _palettes = new Dictionary<string, ITierPalette>(StringComparer.OrdinalIgnoreCase);
        
        // Register default palettes
        RegisterPalette(new ClassicTierPalette());
        RegisterPalette(new VibrantTierPalette());
        RegisterPalette(new MinimalTierPalette());
    }
    
    public ITierPalette GetPalette(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            name = "classic";
        }
        
        return _palettes.TryGetValue(name, out var palette) 
            ? palette 
            : _palettes["classic"];
    }
    
    public void RegisterPalette(ITierPalette palette)
    {
        if (palette == null)
        {
            throw new ArgumentNullException(nameof(palette));
        }
        
        _palettes[palette.Name] = palette;
    }
}