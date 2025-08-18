using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AlbionOnlineSniffer.Options.Profiles;

/// <summary>
/// Manages profile selection and switching
/// </summary>
public interface IProfileManager
{
    /// <summary>
    /// Gets the currently active profile
    /// </summary>
    ProfileOptions CurrentProfile { get; }
    
    /// <summary>
    /// Gets all available profiles
    /// </summary>
    IReadOnlyList<ProfileOptions> AvailableProfiles { get; }
    
    /// <summary>
    /// Switches to a different profile
    /// </summary>
    bool SwitchProfile(string profileName);
    
    /// <summary>
    /// Event raised when profile changes
    /// </summary>
    event EventHandler<ProfileChangedEventArgs>? ProfileChanged;
}

/// <summary>
/// Event args for profile change events
/// </summary>
public class ProfileChangedEventArgs : EventArgs
{
    public ProfileOptions OldProfile { get; }
    public ProfileOptions NewProfile { get; }
    
    public ProfileChangedEventArgs(ProfileOptions oldProfile, ProfileOptions newProfile)
    {
        OldProfile = oldProfile;
        NewProfile = newProfile;
    }
}

/// <summary>
/// Default implementation of profile manager
/// </summary>
public class ProfileManager : IProfileManager
{
    private readonly IOptionsMonitor<SnifferOptions> _optionsMonitor;
    private readonly ILogger<ProfileManager> _logger;
    private ProfileOptions _currentProfile;
    
    public ProfileOptions CurrentProfile => _currentProfile;
    
    public IReadOnlyList<ProfileOptions> AvailableProfiles => 
        _optionsMonitor.CurrentValue.Profiles.AsReadOnly();
    
    public event EventHandler<ProfileChangedEventArgs>? ProfileChanged;
    
    public ProfileManager(
        IOptionsMonitor<SnifferOptions> optionsMonitor,
        ILogger<ProfileManager> logger)
    {
        _optionsMonitor = optionsMonitor;
        _logger = logger;
        _currentProfile = optionsMonitor.CurrentValue.GetActiveProfile();
        
        // Monitor for configuration changes
        optionsMonitor.OnChange(options =>
        {
            var newProfile = options.GetActiveProfile();
            if (newProfile.Name != _currentProfile.Name)
            {
                var oldProfile = _currentProfile;
                _currentProfile = newProfile;
                OnProfileChanged(oldProfile, newProfile);
            }
        });
        
        _logger.LogInformation("ProfileManager initialized with profile: {ProfileName}", 
            _currentProfile.Name);
    }
    
    public bool SwitchProfile(string profileName)
    {
        if (string.IsNullOrWhiteSpace(profileName))
        {
            _logger.LogWarning("Cannot switch to empty profile name");
            return false;
        }
        
        var newProfile = AvailableProfiles.FirstOrDefault(p => 
            p.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase));
        
        if (newProfile == null)
        {
            _logger.LogWarning("Profile not found: {ProfileName}", profileName);
            return false;
        }
        
        if (newProfile.Name == _currentProfile.Name)
        {
            _logger.LogDebug("Already using profile: {ProfileName}", profileName);
            return true;
        }
        
        var oldProfile = _currentProfile;
        _currentProfile = newProfile;
        
        _logger.LogInformation("Switched from profile {OldProfile} to {NewProfile}", 
            oldProfile.Name, newProfile.Name);
        
        OnProfileChanged(oldProfile, newProfile);
        return true;
    }
    
    private void OnProfileChanged(ProfileOptions oldProfile, ProfileOptions newProfile)
    {
        ProfileChanged?.Invoke(this, new ProfileChangedEventArgs(oldProfile, newProfile));
        
        // Log profile differences
        LogProfileDifferences(oldProfile, newProfile);
    }
    
    private void LogProfileDifferences(ProfileOptions oldProfile, ProfileOptions newProfile)
    {
        _logger.LogDebug("Profile change details:");
        _logger.LogDebug("  TierPalette: {Old} -> {New}", 
            oldProfile.TierPalette, newProfile.TierPalette);
        _logger.LogDebug("  Priority: {Old} -> {New}", 
            oldProfile.Priority, newProfile.Priority);
        
        // Log feature toggle changes
        foreach (var toggle in newProfile.FeatureToggles)
        {
            if (!oldProfile.FeatureToggles.TryGetValue(toggle.Key, out var oldValue) || 
                oldValue != toggle.Value)
            {
                _logger.LogDebug("  Feature {Feature}: {Old} -> {New}", 
                    toggle.Key, 
                    oldProfile.FeatureToggles.GetValueOrDefault(toggle.Key, false), 
                    toggle.Value);
            }
        }
        
        // Log threshold changes
        foreach (var threshold in newProfile.Thresholds)
        {
            if (!oldProfile.Thresholds.TryGetValue(threshold.Key, out var oldValue) || 
                Math.Abs(oldValue - threshold.Value) > 0.01)
            {
                _logger.LogDebug("  Threshold {Threshold}: {Old:F2} -> {New:F2}", 
                    threshold.Key, 
                    oldProfile.Thresholds.GetValueOrDefault(threshold.Key, 0), 
                    threshold.Value);
            }
        }
    }
}