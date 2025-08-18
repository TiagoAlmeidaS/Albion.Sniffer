using System.IO.Compression;
using System.Net.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AlbionOnlineSniffer.Options;
using AlbionOnlineSniffer.Providers.Interfaces;

namespace AlbionOnlineSniffer.Providers.Implementations;

/// <summary>
/// HTTP-based binary dump provider with local caching
/// </summary>
public class HttpCachedBinDumpProvider : IBinDumpProvider
{
    private readonly ILogger<HttpCachedBinDumpProvider> _logger;
    private readonly ParsingSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _memoryCache;
    private readonly string _cacheDirectory;
    private readonly string _etagIndexPath;
    
    public event EventHandler<DumpsUpdatedEventArgs>? DumpsUpdated;
    
    public HttpCachedBinDumpProvider(
        ILogger<HttpCachedBinDumpProvider> logger,
        IOptions<SnifferOptions> options,
        HttpClient httpClient,
        IMemoryCache memoryCache)
    {
        _logger = logger;
        _settings = options.Value.Parsing;
        _httpClient = httpClient;
        _memoryCache = memoryCache;
        
        _cacheDirectory = Path.Combine(_settings.CacheDirectory, "dumps");
        _etagIndexPath = Path.Combine(_cacheDirectory, "etag-index.json");
        
        if (_settings.EnableRemoteCache && !Directory.Exists(_cacheDirectory))
        {
            Directory.CreateDirectory(_cacheDirectory);
        }
    }
    
    public async Task<Stream?> GetDumpAsync(string name, CancellationToken cancellationToken = default)
    {
        // Check memory cache first
        var cacheKey = $"dump_{name}";
        if (_memoryCache.TryGetValue<byte[]>(cacheKey, out var cachedData))
        {
            _logger.LogDebug("Dump {Name} found in memory cache", name);
            return new MemoryStream(cachedData);
        }
        
        // Check file cache
        if (_settings.EnableRemoteCache)
        {
            var cachedFile = Path.Combine(_cacheDirectory, $"{name}.bin");
            if (File.Exists(cachedFile))
            {
                var fileInfo = new FileInfo(cachedFile);
                var age = DateTime.UtcNow - fileInfo.LastWriteTimeUtc;
                
                if (age.TotalHours < _settings.CacheExpirationHours)
                {
                    _logger.LogDebug("Dump {Name} found in file cache", name);
                    var data = await File.ReadAllBytesAsync(cachedFile, cancellationToken);
                    
                    // Add to memory cache
                    _memoryCache.Set(cacheKey, data, TimeSpan.FromMinutes(30));
                    
                    return new MemoryStream(data);
                }
                else
                {
                    _logger.LogInformation("Dump {Name} cache STALE (age={AgeHours:F1}h >= {Ttl}h)", name, age.TotalHours, _settings.CacheExpirationHours);
                }
            }
        }
        
        // Download from remote
        return await DownloadDumpAsync(name, cacheKey, cancellationToken);
    }
    
    private async Task<Stream?> DownloadDumpAsync(string name, string cacheKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.RemoteDumpsUrl))
        {
            _logger.LogError("Remote dumps URL not configured");
            return null;
        }
        
        var url = $"{_settings.RemoteDumpsUrl.TrimEnd('/')}/{name}.bin";
        
        try
        {
            _logger.LogInformation("Downloading dump from: {Url}", url);
            
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            // ETag conditional request
            var etagMap = LoadEtagIndex();
            if (etagMap.TryGetValue(name, out var etag))
            {
                request.Headers.IfNoneMatch.ParseAdd(etag);
            }
            
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotModified && _settings.EnableRemoteCache)
                {
                    _logger.LogInformation("Dump {Name} cache HIT (ETag not modified)", name);
                    var cachedFile = Path.Combine(_cacheDirectory, $"{name}.bin");
                    if (File.Exists(cachedFile))
                    {
                        var dataCached = await File.ReadAllBytesAsync(cachedFile, cancellationToken);
                        _memoryCache.Set(cacheKey, dataCached, TimeSpan.FromMinutes(30));
                        return new MemoryStream(dataCached);
                    }
                }
                _logger.LogWarning("Failed to download dump {Name}: {Status}", name, response.StatusCode);
                return null;
            }
            
            var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            
            // Handle compressed content
            if (response.Content.Headers.ContentEncoding.Contains("gzip"))
            {
                using var compressed = new MemoryStream(data);
                using var gzip = new GZipStream(compressed, CompressionMode.Decompress);
                using var decompressed = new MemoryStream();
                await gzip.CopyToAsync(decompressed, cancellationToken);
                data = decompressed.ToArray();
            }
            
            // Cache the data
            _memoryCache.Set(cacheKey, data, TimeSpan.FromMinutes(30));
            
            if (_settings.EnableRemoteCache)
            {
                var cachedFile = Path.Combine(_cacheDirectory, $"{name}.bin");
                await File.WriteAllBytesAsync(cachedFile, data, cancellationToken);
                // Persist ETag
                if (response.Headers.ETag != null)
                {
                    etagMap[name] = response.Headers.ETag.Tag ?? string.Empty;
                    SaveEtagIndex(etagMap);
                }
            }
            
            _logger.LogInformation("Successfully downloaded and cached dump {Name}", name);
            
            return new MemoryStream(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download dump {Name}", name);
            return null;
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
    
    public async Task<IEnumerable<string>> ListDumpsAsync(CancellationToken cancellationToken = default)
    {
        // Try to get list from remote manifest
        if (!string.IsNullOrWhiteSpace(_settings.RemoteDumpsUrl))
        {
            try
            {
                var manifestUrl = $"{_settings.RemoteDumpsUrl.TrimEnd('/')}/manifest.json";
                var response = await _httpClient.GetStringAsync(manifestUrl, cancellationToken);
                
                var manifest = System.Text.Json.JsonSerializer.Deserialize<DumpManifest>(response);
                if (manifest?.Dumps != null)
                {
                    return manifest.Dumps;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch remote manifest");
            }
        }
        
        // Fallback to cached files
        if (Directory.Exists(_cacheDirectory))
        {
            return Directory.GetFiles(_cacheDirectory, "*.bin")
                .Select(Path.GetFileNameWithoutExtension)
                .Where(n => !string.IsNullOrEmpty(n))
                .Cast<string>();
        }
        
        return Enumerable.Empty<string>();
    }
    
    public async Task<string> GetVersionAsync(CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(_settings.RemoteDumpsUrl))
        {
            try
            {
                var versionUrl = $"{_settings.RemoteDumpsUrl.TrimEnd('/')}/version.txt";
                return await _httpClient.GetStringAsync(versionUrl, cancellationToken);
            }
            catch
            {
                // Ignore
            }
        }
        
        return "remote";
    }
    
    public async Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        // Check cache first
        if (_settings.EnableRemoteCache)
        {
            var cachedFile = Path.Combine(_cacheDirectory, $"{name}.bin");
            if (File.Exists(cachedFile))
            {
                return true;
            }
        }
        
        // Check remote
        if (!string.IsNullOrWhiteSpace(_settings.RemoteDumpsUrl))
        {
            var url = $"{_settings.RemoteDumpsUrl.TrimEnd('/')}/{name}.bin";
            
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Head, url);
                var response = await _httpClient.SendAsync(request, cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        
        return false;
    }
    
    private class DumpManifest
    {
        public List<string>? Dumps { get; set; }
        public string? Version { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}