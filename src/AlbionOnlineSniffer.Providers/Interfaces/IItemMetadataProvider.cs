namespace AlbionOnlineSniffer.Providers.Interfaces;

/// <summary>
/// Provider interface for item metadata
/// </summary>
public interface IItemMetadataProvider
{
    /// <summary>
    /// Gets metadata for a specific item
    /// </summary>
    /// <param name="itemId">The item identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Item metadata or null if not found</returns>
    ValueTask<ItemMetadata?> GetItemAsync(string itemId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets metadata for multiple items
    /// </summary>
    /// <param name="itemIds">Collection of item identifiers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of item metadata by ID</returns>
    Task<IReadOnlyDictionary<string, ItemMetadata>> GetItemsAsync(
        IEnumerable<string> itemIds, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Searches for items by name or partial name
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="maxResults">Maximum number of results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of matching items</returns>
    Task<IEnumerable<ItemMetadata>> SearchItemsAsync(
        string searchTerm, 
        int maxResults = 10,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all items of a specific tier
    /// </summary>
    Task<IEnumerable<ItemMetadata>> GetItemsByTierAsync(
        int tier,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Refreshes the metadata cache
    /// </summary>
    Task RefreshAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Item metadata structure
/// </summary>
public class ItemMetadata
{
    /// <summary>
    /// Unique item identifier
    /// </summary>
    public string Id { get; init; } = string.Empty;
    
    /// <summary>
    /// Localized item name
    /// </summary>
    public string Name { get; init; } = string.Empty;
    
    /// <summary>
    /// Item description
    /// </summary>
    public string? Description { get; init; }
    
    /// <summary>
    /// Item tier (1-8)
    /// </summary>
    public int Tier { get; init; }
    
    /// <summary>
    /// Item enchantment level
    /// </summary>
    public int Enchantment { get; init; }
    
    /// <summary>
    /// Item category (weapon, armor, resource, etc.)
    /// </summary>
    public string Category { get; init; } = string.Empty;
    
    /// <summary>
    /// Item subcategory
    /// </summary>
    public string? Subcategory { get; init; }
    
    /// <summary>
    /// Icon URL or path
    /// </summary>
    public string? IconPath { get; init; }
    
    /// <summary>
    /// Item weight
    /// </summary>
    public double Weight { get; init; }
    
    /// <summary>
    /// Is stackable
    /// </summary>
    public bool IsStackable { get; init; }
    
    /// <summary>
    /// Maximum stack size
    /// </summary>
    public int MaxStackSize { get; init; } = 1;
    
    /// <summary>
    /// Additional properties
    /// </summary>
    public Dictionary<string, object> Properties { get; init; } = new();
}