namespace AlbionOnlineSniffer.Providers.Interfaces;

/// <summary>
/// Provider interface for binary dump files
/// </summary>
public interface IBinDumpProvider
{
    /// <summary>
    /// Gets a binary dump stream by name
    /// </summary>
    /// <param name="name">Name of the dump file (without extension)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Stream containing the dump data</returns>
    Task<Stream?> GetDumpAsync(string name, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Lists all available dump names
    /// </summary>
    Task<IEnumerable<string>> ListDumpsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the version of the dumps
    /// </summary>
    Task<string> GetVersionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if a specific dump exists
    /// </summary>
    Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Event raised when dumps are updated
    /// </summary>
    event EventHandler<DumpsUpdatedEventArgs>? DumpsUpdated;
}

/// <summary>
/// Event args for dump updates
/// </summary>
public class DumpsUpdatedEventArgs : EventArgs
{
    public string Version { get; }
    public DateTime UpdateTime { get; }
    public IReadOnlyList<string> UpdatedDumps { get; }
    
    public DumpsUpdatedEventArgs(string version, DateTime updateTime, IReadOnlyList<string> updatedDumps)
    {
        Version = version;
        UpdateTime = updateTime;
        UpdatedDumps = updatedDumps;
    }
}