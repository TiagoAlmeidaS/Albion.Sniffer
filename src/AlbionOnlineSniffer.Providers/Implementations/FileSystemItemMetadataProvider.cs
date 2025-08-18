using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AlbionOnlineSniffer.Options;
using AlbionOnlineSniffer.Providers.Interfaces;

namespace AlbionOnlineSniffer.Providers.Implementations;

/// <summary>
/// File system based item metadata provider
/// </summary>
public class FileSystemItemMetadataProvider : IItemMetadataProvider
{
    private readonly ILogger<FileSystemItemMetadataProvider> _logger;
    private readonly ParsingSettings _settings;
    private readonly IMemoryCache _cache;
    private readonly Dictionary<string, ItemMetadata> _items;
    private readonly SemaphoreSlim _loadLock;
    private bool _isLoaded;
    
    public FileSystemItemMetadataProvider(
        ILogger<FileSystemItemMetadataProvider> logger,
        IOptions<SnifferOptions> options,
        IMemoryCache cache)
    {
        _logger = logger;
        _settings = options.Value.Parsing;
        _cache = cache;
        _items = new Dictionary<string, ItemMetadata>(StringComparer.OrdinalIgnoreCase);
        _loadLock = new SemaphoreSlim(1, 1);
        _isLoaded = false;
    }
    
    public async ValueTask<ItemMetadata?> GetItemAsync(string itemId, CancellationToken cancellationToken = default)
    {
        await EnsureLoadedAsync(cancellationToken);
        
        return _cache.GetOrCreate($"item_{itemId}", entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(5);
            return _items.TryGetValue(itemId, out var item) ? item : null;
        });
    }
    
    public async Task<IReadOnlyDictionary<string, ItemMetadata>> GetItemsAsync(
        IEnumerable<string> itemIds,
        CancellationToken cancellationToken = default)
    {
        await EnsureLoadedAsync(cancellationToken);
        
        var result = new Dictionary<string, ItemMetadata>();
        foreach (var id in itemIds)
        {
            var item = await GetItemAsync(id, cancellationToken);
            if (item != null)
            {
                result[id] = item;
            }
        }
        
        return result;
    }
    
    public async Task<IEnumerable<ItemMetadata>> SearchItemsAsync(
        string searchTerm,
        int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        await EnsureLoadedAsync(cancellationToken);
        
        var searchLower = searchTerm.ToLowerInvariant();
        return _items.Values
            .Where(i => i.Name.Contains(searchLower, StringComparison.OrdinalIgnoreCase) ||
                       i.Id.Contains(searchLower, StringComparison.OrdinalIgnoreCase))
            .Take(maxResults);
    }
    
    public async Task<IEnumerable<ItemMetadata>> GetItemsByTierAsync(
        int tier,
        CancellationToken cancellationToken = default)
    {
        await EnsureLoadedAsync(cancellationToken);
        
        return _items.Values.Where(i => i.Tier == tier);
    }
    
    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        await _loadLock.WaitAsync(cancellationToken);
        try
        {
            _isLoaded = false;
            _items.Clear();
            _cache.Remove("items_loaded");
            await LoadItemsAsync(cancellationToken);
        }
        finally
        {
            _loadLock.Release();
        }
    }
    
    private async Task EnsureLoadedAsync(CancellationToken cancellationToken)
    {
        if (_isLoaded)
        {
            return;
        }
        
        await _loadLock.WaitAsync(cancellationToken);
        try
        {
            if (!_isLoaded)
            {
                await LoadItemsAsync(cancellationToken);
            }
        }
        finally
        {
            _loadLock.Release();
        }
    }
    
    private async Task LoadItemsAsync(CancellationToken cancellationToken)
    {
        var itemsPath = ResolveItemsPath();
        
        if (!Directory.Exists(itemsPath))
        {
            _logger.LogWarning("Items directory not found: {Path}", itemsPath);
            _isLoaded = true;
            return;
        }
        
        _logger.LogInformation("Loading items from: {Path}", itemsPath);
        
        // Look for index.json or items.json or individual item files
        var indexFile = Path.Combine(itemsPath, "index.json");
        var itemsFile = Path.Combine(itemsPath, "items.json");
        if (File.Exists(indexFile))
        {
            await LoadIndexAsync(indexFile, cancellationToken);
        }
        else if (File.Exists(itemsFile))
        {
            await LoadItemsFromJsonAsync(itemsFile, cancellationToken);
        }
        else
        {
            await LoadItemsFromDirectoryAsync(itemsPath, cancellationToken);
        }
        
        _isLoaded = true;
        _logger.LogInformation("Loaded {Count} items", _items.Count);
    }
    
    private string ResolveItemsPath()
    {
        var basePath = _settings.ItemsPath;
        // Support versioned directory ITEMS/{version}
        if (!string.IsNullOrEmpty(_settings.DumpsVersion))
        {
            var versioned = Path.Combine(basePath, _settings.DumpsVersion);
            if (Directory.Exists(versioned))
            {
                return versioned;
            }
        }
        return basePath;
    }

    private async Task LoadIndexAsync(string indexFile, CancellationToken cancellationToken)
    {
        try
        {
            var json = await File.ReadAllTextAsync(indexFile, cancellationToken);
            var dict = JsonSerializer.Deserialize<Dictionary<string, ItemMetadata>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (dict != null)
            {
                foreach (var kv in dict)
                {
                    _items[kv.Key] = kv.Value;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load index.json: {File}", indexFile);
        }
    }
    
    private async Task LoadItemsFromJsonAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            var json = await File.ReadAllTextAsync(filePath, cancellationToken);
            var items = JsonSerializer.Deserialize<List<ItemMetadata>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            if (items != null)
            {
                foreach (var item in items)
                {
                    _items[item.Id] = item;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load items from {File}", filePath);
        }
    }
    
    private async Task LoadItemsFromDirectoryAsync(string directory, CancellationToken cancellationToken)
    {
        // Try to load from XML files (deatheye format)
        var xmlFiles = Directory.GetFiles(directory, "*.xml", SearchOption.AllDirectories);
        foreach (var file in xmlFiles)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            
            try
            {
                var item = await ParseItemXmlAsync(file, cancellationToken);
                if (item != null)
                {
                    _items[item.Id] = item;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse item file: {File}", file);
            }
        }
        
        // Also try JSON files
        var jsonFiles = Directory.GetFiles(directory, "*.json", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith("items.json", StringComparison.OrdinalIgnoreCase));
        
        foreach (var file in jsonFiles)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            
            try
            {
                var json = await File.ReadAllTextAsync(file, cancellationToken);
                var item = JsonSerializer.Deserialize<ItemMetadata>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (item != null)
                {
                    _items[item.Id] = item;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse item file: {File}", file);
            }
        }
    }
    
    private async Task<ItemMetadata?> ParseItemXmlAsync(string filePath, CancellationToken cancellationToken)
    {
        // Simple XML parsing for deatheye format
        var xml = await File.ReadAllTextAsync(filePath, cancellationToken);
        
        // Extract basic item data using regex (simplified)
        var idMatch = System.Text.RegularExpressions.Regex.Match(xml, @"uniquename=""([^""]+)""");
        var tierMatch = System.Text.RegularExpressions.Regex.Match(xml, @"tier=""(\d+)""");
        
        if (!idMatch.Success)
        {
            return null;
        }
        
        var item = new ItemMetadata
        {
            Id = idMatch.Groups[1].Value,
            Name = Path.GetFileNameWithoutExtension(filePath),
            Tier = tierMatch.Success ? int.Parse(tierMatch.Groups[1].Value) : 1,
            Category = DetermineCategory(idMatch.Groups[1].Value),
            IconPath = $"ITEMS/{idMatch.Groups[1].Value}.png"
        };
        
        return item;
    }
    
    private string DetermineCategory(string itemId)
    {
        // Simple category determination based on item ID patterns
        var id = itemId.ToUpperInvariant();
        
        if (id.Contains("SWORD") || id.Contains("AXE") || id.Contains("MACE") || 
            id.Contains("HAMMER") || id.Contains("BOW") || id.Contains("STAFF"))
        {
            return "Weapon";
        }
        
        if (id.Contains("ARMOR") || id.Contains("HELMET") || id.Contains("SHOES") || 
            id.Contains("BOOTS") || id.Contains("CLOTH") || id.Contains("LEATHER") || 
            id.Contains("PLATE"))
        {
            return "Armor";
        }
        
        if (id.Contains("WOOD") || id.Contains("ORE") || id.Contains("HIDE") || 
            id.Contains("FIBER") || id.Contains("STONE") || id.Contains("ROCK"))
        {
            return "Resource";
        }
        
        if (id.Contains("POTION") || id.Contains("FOOD"))
        {
            return "Consumable";
        }
        
        if (id.Contains("MOUNT") || id.Contains("HORSE"))
        {
            return "Mount";
        }
        
        return "Other";
    }
}