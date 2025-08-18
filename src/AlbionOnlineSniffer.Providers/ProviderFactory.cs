using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using AlbionOnlineSniffer.Options;
using AlbionOnlineSniffer.Providers.Interfaces;
using AlbionOnlineSniffer.Providers.Implementations;
using System.Net.Http;

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
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var options = serviceProvider.GetRequiredService<IOptions<SnifferOptions>>();
        
        return settings.BinDumpProvider?.ToLowerInvariant() switch
        {
            "filesystem" => new FileSystemBinDumpProvider(
                loggerFactory.CreateLogger<FileSystemBinDumpProvider>(),
                options),
            "embedded" => new EmbeddedResourceBinDumpProvider(
                loggerFactory.CreateLogger<EmbeddedResourceBinDumpProvider>()),
            _ => new FileSystemBinDumpProvider(
                loggerFactory.CreateLogger<FileSystemBinDumpProvider>(),
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
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var options = serviceProvider.GetRequiredService<IOptions<SnifferOptions>>();
        var cache = serviceProvider.GetRequiredService<IMemoryCache>();
        
        return settings.ItemMetadataProvider?.ToLowerInvariant() switch
        {
            "filesystem" => new FileSystemItemMetadataProvider(
                loggerFactory.CreateLogger<FileSystemItemMetadataProvider>(),
                options,
                cache),
            "embedded" => new EmbeddedResourceItemMetadataProvider(
                loggerFactory.CreateLogger<EmbeddedResourceItemMetadataProvider>()),
            _ => new FileSystemItemMetadataProvider(
                loggerFactory.CreateLogger<FileSystemItemMetadataProvider>(),
                options,
                cache)
        };
    }
}

// NOTE: Fallback wrappers and HTTP providers moved to a dedicated Providers v2 PR scope.
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
    private readonly string _etagIndexPath;
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
        _etagIndexPath = Path.Combine(_cacheDirectory, "etag-index.json");
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
                    _logger.LogInformation("ItemMetadata cache HIT (fresh): {File}", cacheFile);
                    await LoadItemsFromFileAsync(cacheFile, cancellationToken);
                    _isLoaded = true;
                    return;
                }
                else
                {
                    _logger.LogInformation("ItemMetadata cache STALE: {File} age={AgeHours:F1}h", cacheFile, age.TotalHours);
                }
            }
            else
            {
                _logger.LogInformation("ItemMetadata cache MISS: {File}", cacheFile);
            }
        }
        
        // Download from remote
        if (!string.IsNullOrWhiteSpace(_settings.RemoteItemsUrl))
        {
            try
            {
                _logger.LogInformation("Downloading items from: {Url}", _settings.RemoteItemsUrl);
                var request = new HttpRequestMessage(HttpMethod.Get, _settings.RemoteItemsUrl);
                var etagMap = LoadEtagIndex();
                if (etagMap.TryGetValue("items", out var etag))
                {
                    request.Headers.IfNoneMatch.ParseAdd(etag);
                }
                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                if (response.StatusCode == System.Net.HttpStatusCode.NotModified && _settings.EnableRemoteCache)
                {
                    var cacheFile = Path.Combine(_cacheDirectory, "items.json");
                    if (File.Exists(cacheFile))
                    {
                        _logger.LogInformation("Remote items not modified (ETag). Using cached file.");
                        await LoadItemsFromFileAsync(cacheFile, cancellationToken);
                        _isLoaded = true;
                        return;
                    }
                }
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
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
                        if (response.Headers.ETag != null)
                        {
                            etagMap["items"] = response.Headers.ETag.Tag ?? string.Empty;
                            SaveEtagIndex(etagMap);
                        }
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

    private Dictionary<string, string> LoadEtagIndex()
    {
        try
        {
            if (File.Exists(_etagIndexPath))
            {
                var json = File.ReadAllText(_etagIndexPath);
                var map = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                return map ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
        }
        catch { }
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    private void SaveEtagIndex(Dictionary<string, string> map)
    {
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(map);
            File.WriteAllText(_etagIndexPath, json);
        }
        catch { }
    }
}