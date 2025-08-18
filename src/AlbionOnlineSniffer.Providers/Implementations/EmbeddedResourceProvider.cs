using System.Reflection;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Providers.Interfaces;

namespace AlbionOnlineSniffer.Providers.Implementations;

/// <summary>
/// Provider that loads dumps from embedded resources
/// </summary>
public class EmbeddedResourceBinDumpProvider : IBinDumpProvider
{
    private readonly ILogger<EmbeddedResourceBinDumpProvider> _logger;
    private readonly Assembly _resourceAssembly;
    private readonly string _resourceNamespace;
    private readonly Dictionary<string, string> _resourceMap;
    
    public event EventHandler<DumpsUpdatedEventArgs>? DumpsUpdated;
    
    public EmbeddedResourceBinDumpProvider(
        ILogger<EmbeddedResourceBinDumpProvider> logger,
        Assembly? resourceAssembly = null,
        string? resourceNamespace = null)
    {
        _logger = logger;
        _resourceAssembly = resourceAssembly ?? Assembly.GetExecutingAssembly();
        _resourceNamespace = resourceNamespace ?? "AlbionOnlineSniffer.Providers.Resources.Dumps";
        _resourceMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        InitializeResourceMap();
    }
    
    private void InitializeResourceMap()
    {
        var resources = _resourceAssembly.GetManifestResourceNames()
            .Where(r => r.StartsWith(_resourceNamespace, StringComparison.OrdinalIgnoreCase));
        
        foreach (var resource in resources)
        {
            var name = resource.Substring(_resourceNamespace.Length + 1);
            if (name.EndsWith(".bin", StringComparison.OrdinalIgnoreCase))
            {
                name = name.Substring(0, name.Length - 4);
            }
            
            _resourceMap[name] = resource;
            _logger.LogDebug("Mapped embedded resource: {Name} -> {Resource}", name, resource);
        }
        
        _logger.LogInformation("Initialized with {Count} embedded dump resources", _resourceMap.Count);
    }
    
    public Task<Stream?> GetDumpAsync(string name, CancellationToken cancellationToken = default)
    {
        if (!_resourceMap.TryGetValue(name, out var resourceName))
        {
            _logger.LogWarning("Embedded dump not found: {Name}", name);
            return Task.FromResult<Stream?>(null);
        }
        
        try
        {
            var stream = _resourceAssembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                _logger.LogError("Failed to load embedded resource: {Resource}", resourceName);
                return Task.FromResult<Stream?>(null);
            }
            
            _logger.LogDebug("Loaded embedded dump: {Name}", name);
            return Task.FromResult<Stream?>(stream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading embedded dump: {Name}", name);
            return Task.FromResult<Stream?>(null);
        }
    }
    
    public Task<IEnumerable<string>> ListDumpsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_resourceMap.Keys.AsEnumerable());
    }
    
    public Task<string> GetVersionAsync(CancellationToken cancellationToken = default)
    {
        // Use assembly version as dump version
        var version = _resourceAssembly.GetName().Version?.ToString() ?? "embedded";
        return Task.FromResult(version);
    }
    
    public Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_resourceMap.ContainsKey(name));
    }
}

/// <summary>
/// Provider that loads item metadata from embedded resources
/// </summary>
public class EmbeddedResourceItemMetadataProvider : IItemMetadataProvider
{
    private readonly ILogger<EmbeddedResourceItemMetadataProvider> _logger;
    private readonly Assembly _resourceAssembly;
    private readonly string _resourceNamespace;
    private readonly Dictionary<string, ItemMetadata> _items;
    private readonly SemaphoreSlim _loadLock;
    private bool _isLoaded;
    
    public EmbeddedResourceItemMetadataProvider(
        ILogger<EmbeddedResourceItemMetadataProvider> logger,
        Assembly? resourceAssembly = null,
        string? resourceNamespace = null)
    {
        _logger = logger;
        _resourceAssembly = resourceAssembly ?? Assembly.GetExecutingAssembly();
        _resourceNamespace = resourceNamespace ?? "AlbionOnlineSniffer.Providers.Resources.Items";
        _items = new Dictionary<string, ItemMetadata>(StringComparer.OrdinalIgnoreCase);
        _loadLock = new SemaphoreSlim(1, 1);
        _isLoaded = false;
    }
    
    public async ValueTask<ItemMetadata?> GetItemAsync(string itemId, CancellationToken cancellationToken = default)
    {
        await EnsureLoadedAsync(cancellationToken);
        return _items.TryGetValue(itemId, out var item) ? item : null;
    }
    
    public async Task<IReadOnlyDictionary<string, ItemMetadata>> GetItemsAsync(
        IEnumerable<string> itemIds,
        CancellationToken cancellationToken = default)
    {
        await EnsureLoadedAsync(cancellationToken);
        
        var result = new Dictionary<string, ItemMetadata>();
        foreach (var id in itemIds)
        {
            if (_items.TryGetValue(id, out var item))
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
        var itemsResource = $"{_resourceNamespace}.items.json";
        
        try
        {
            using var stream = _resourceAssembly.GetManifestResourceStream(itemsResource);
            if (stream == null)
            {
                _logger.LogWarning("Embedded items resource not found: {Resource}", itemsResource);
                _isLoaded = true;
                return;
            }
            
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync(cancellationToken);
            
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
            }
            
            _logger.LogInformation("Loaded {Count} items from embedded resources", _items.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load embedded items");
        }
        
        _isLoaded = true;
    }
}