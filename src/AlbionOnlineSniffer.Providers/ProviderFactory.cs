using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using AlbionOnlineSniffer.Options;
using AlbionOnlineSniffer.Providers.Interfaces;
using AlbionOnlineSniffer.Providers.Implementations;

namespace AlbionOnlineSniffer.Providers;

/// <summary>
/// Factory for creating providers based on configuration
/// </summary>
public static class ProviderFactory
{
    /// <summary>
    /// Creates a bin dump provider based on configuration
    /// </summary>
    public static IBinDumpProvider CreateBinDumpProvider(
        IServiceProvider serviceProvider,
        ParsingSettings settings)
    {
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
        var options = serviceProvider.GetRequiredService<IOptions<SnifferOptions>>();
        
        return settings.BinDumpProvider?.ToLowerInvariant() switch
        {
            "filesystem" => new FileSystemBinDumpProvider(
                logger.CreateLogger<FileSystemBinDumpProvider>(),
                options),
            
            "http" => new HttpCachedBinDumpProvider(
                logger.CreateLogger<HttpCachedBinDumpProvider>(),
                options,
                serviceProvider.GetRequiredService<HttpClient>(),
                serviceProvider.GetRequiredService<IMemoryCache>()),
            
            "embedded" => new EmbeddedResourceBinDumpProvider(
                logger.CreateLogger<EmbeddedResourceBinDumpProvider>()),
            
            _ => new FileSystemBinDumpProvider(
                logger.CreateLogger<FileSystemBinDumpProvider>(),
                options)
        };
    }
    
    /// <summary>
    /// Creates an item metadata provider based on configuration
    /// </summary>
    public static IItemMetadataProvider CreateItemMetadataProvider(
        IServiceProvider serviceProvider,
        ParsingSettings settings)
    {
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
        var options = serviceProvider.GetRequiredService<IOptions<SnifferOptions>>();
        var cache = serviceProvider.GetRequiredService<IMemoryCache>();
        
        return settings.ItemMetadataProvider?.ToLowerInvariant() switch
        {
            "filesystem" => new FileSystemItemMetadataProvider(
                logger.CreateLogger<FileSystemItemMetadataProvider>(),
                options,
                cache),
            
            "http" => new HttpCachedItemMetadataProvider(
                logger.CreateLogger<HttpCachedItemMetadataProvider>(),
                options,
                serviceProvider.GetRequiredService<HttpClient>(),
                cache),
            
            "embedded" => new EmbeddedResourceItemMetadataProvider(
                logger.CreateLogger<EmbeddedResourceItemMetadataProvider>()),
            
            _ => new FileSystemItemMetadataProvider(
                logger.CreateLogger<FileSystemItemMetadataProvider>(),
                options,
                cache)
        };
    }
}

/// <summary>
/// HTTP-based item metadata provider with caching
/// </summary>
public class HttpCachedItemMetadataProvider : IItemMetadataProvider
{
    private readonly ILogger<HttpCachedItemMetadataProvider> _logger;
    private readonly ParsingSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly string _cacheDirectory;
    private readonly Dictionary<string, ItemMetadata> _items;
    private readonly SemaphoreSlim _loadLock;
    private bool _isLoaded;
    
    public HttpCachedItemMetadataProvider(
        ILogger<HttpCachedItemMetadataProvider> logger,
        IOptions<SnifferOptions> options,
        HttpClient httpClient,
        IMemoryCache cache)
    {
        _logger = logger;
        _settings = options.Value.Parsing;
        _httpClient = httpClient;
        _cache = cache;
        _cacheDirectory = Path.Combine(_settings.CacheDirectory, "items");
        _items = new Dictionary<string, ItemMetadata>(StringComparer.OrdinalIgnoreCase);
        _loadLock = new SemaphoreSlim(1, 1);
        _isLoaded = false;
        
        if (_settings.EnableRemoteCache && !Directory.Exists(_cacheDirectory))
        {
            Directory.CreateDirectory(_cacheDirectory);
        }
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
        // Check cache first
        if (_settings.EnableRemoteCache)
        {
            var cacheFile = Path.Combine(_cacheDirectory, "items.json");
            if (File.Exists(cacheFile))
            {
                var fileInfo = new FileInfo(cacheFile);
                var age = DateTime.UtcNow - fileInfo.LastWriteTimeUtc;
                
                if (age.TotalHours < _settings.CacheExpirationHours)
                {
                    await LoadItemsFromFileAsync(cacheFile, cancellationToken);
                    _isLoaded = true;
                    return;
                }
            }
        }
        
        // Download from remote
        if (!string.IsNullOrWhiteSpace(_settings.RemoteItemsUrl))
        {
            try
            {
                _logger.LogInformation("Downloading items from: {Url}", _settings.RemoteItemsUrl);
                
                var json = await _httpClient.GetStringAsync(_settings.RemoteItemsUrl, cancellationToken);
                var items = System.Text.Json.JsonSerializer.Deserialize<List<ItemMetadata>>(json,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        _items[item.Id] = item;
                    }
                    
                    // Cache the data
                    if (_settings.EnableRemoteCache)
                    {
                        var cacheFile = Path.Combine(_cacheDirectory, "items.json");
                        await File.WriteAllTextAsync(cacheFile, json, cancellationToken);
                    }
                    
                    _logger.LogInformation("Loaded {Count} items from remote", _items.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download items from remote");
            }
        }
        
        _isLoaded = true;
    }
    
    private async Task LoadItemsFromFileAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            var json = await File.ReadAllTextAsync(filePath, cancellationToken);
            var items = System.Text.Json.JsonSerializer.Deserialize<List<ItemMetadata>>(json,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            
            if (items != null)
            {
                foreach (var item in items)
                {
                    _items[item.Id] = item;
                }
                
                _logger.LogInformation("Loaded {Count} items from cache", _items.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load items from cache");
        }
    }
}