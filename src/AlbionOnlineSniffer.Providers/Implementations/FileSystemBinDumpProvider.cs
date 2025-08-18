using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AlbionOnlineSniffer.Options;
using AlbionOnlineSniffer.Providers.Interfaces;
using System.IO.Compression;

namespace AlbionOnlineSniffer.Providers.Implementations;

/// <summary>
/// File system based binary dump provider
/// </summary>
public class FileSystemBinDumpProvider : IBinDumpProvider
{
    private readonly ILogger<FileSystemBinDumpProvider> _logger;
    private readonly ParsingSettings _settings;
    private readonly FileSystemWatcher? _watcher;
    private string _currentVersion = "unknown";
    
    public event EventHandler<DumpsUpdatedEventArgs>? DumpsUpdated;
    
    public FileSystemBinDumpProvider(
        ILogger<FileSystemBinDumpProvider> logger,
        IOptions<SnifferOptions> options)
    {
        _logger = logger;
        _settings = options.Value.Parsing;
        
        if (_settings.AutoReloadDumps && Directory.Exists(_settings.BinDumpsPath))
        {
            _watcher = new FileSystemWatcher(_settings.BinDumpsPath)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                Filter = "*.bin",
                EnableRaisingEvents = true
            };
            
            _watcher.Changed += OnDumpFileChanged;
            _watcher.Created += OnDumpFileChanged;
        }
        
        InitializeVersion();
    }
    
    public async Task<Stream?> GetDumpAsync(string name, CancellationToken cancellationToken = default)
    {
        var basePath = GetDumpsPath();
        if (!Directory.Exists(basePath))
        {
            _logger.LogWarning("Dumps directory not found: {Path}", basePath);
            return null;
        }
        
        // Try different file extensions
        var extensions = new[] { ".bin", ".gz", ".zip" };
        foreach (var ext in extensions)
        {
            var filePath = Path.Combine(basePath, name + ext);
            if (File.Exists(filePath))
            {
                _logger.LogDebug("Loading dump: {File}", filePath);
                
                try
                {
                    var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    
                    // Handle compressed files
                    if (ext == ".gz")
                    {
                        return new GZipStream(fileStream, CompressionMode.Decompress);
                    }
                    else if (ext == ".zip")
                    {
                        var archive = new ZipArchive(fileStream, ZipArchiveMode.Read);
                        var entry = archive.Entries.FirstOrDefault();
                        if (entry != null)
                        {
                            return entry.Open();
                        }
                    }
                    
                    return fileStream;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load dump: {File}", filePath);
                    return null;
                }
            }
        }
        
        _logger.LogWarning("Dump not found: {Name}", name);
        return null;
    }
    
    public Task<IEnumerable<string>> ListDumpsAsync(CancellationToken cancellationToken = default)
    {
        var basePath = GetDumpsPath();
        if (!Directory.Exists(basePath))
        {
            return Task.FromResult(Enumerable.Empty<string>());
        }
        
        var dumps = Directory.GetFiles(basePath, "*.bin")
            .Concat(Directory.GetFiles(basePath, "*.gz"))
            .Concat(Directory.GetFiles(basePath, "*.zip"))
            .Select(f => Path.GetFileNameWithoutExtension(f))
            .Distinct()
            .OrderBy(n => n);
        
        return Task.FromResult(dumps);
    }
    
    public Task<string> GetVersionAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_currentVersion);
    }
    
    public Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        var basePath = GetDumpsPath();
        if (!Directory.Exists(basePath))
        {
            return Task.FromResult(false);
        }
        
        var extensions = new[] { ".bin", ".gz", ".zip" };
        var exists = extensions.Any(ext => 
            File.Exists(Path.Combine(basePath, name + ext)));
        
        return Task.FromResult(exists);
    }
    
    private string GetDumpsPath()
    {
        var basePath = _settings.BinDumpsPath;
        
        // Check for versioned subdirectories
        if (_settings.DumpsVersion != "latest" && !string.IsNullOrEmpty(_settings.DumpsVersion))
        {
            var versionPath = Path.Combine(basePath, _settings.DumpsVersion);
            if (Directory.Exists(versionPath))
            {
                return versionPath;
            }
        }
        
        // Look for latest version directory (format: YYYY-MM-DD)
        if (Directory.Exists(basePath))
        {
            var versionDirs = Directory.GetDirectories(basePath)
                .Where(d => System.Text.RegularExpressions.Regex.IsMatch(
                    Path.GetFileName(d), @"^\d{4}-\d{2}-\d{2}$"))
                .OrderByDescending(d => d)
                .FirstOrDefault();
            
            if (versionDirs != null)
            {
                _logger.LogInformation("Using dumps version: {Version}", Path.GetFileName(versionDirs));
                return versionDirs;
            }
        }
        
        return basePath;
    }
    
    private void InitializeVersion()
    {
        var path = GetDumpsPath();
        if (Directory.Exists(path))
        {
            // Try to extract version from path
            var dirName = Path.GetFileName(path);
            if (System.Text.RegularExpressions.Regex.IsMatch(dirName, @"^\d{4}-\d{2}-\d{2}$"))
            {
                _currentVersion = dirName;
            }
            else
            {
                // Use last write time as version
                var lastWrite = Directory.GetLastWriteTimeUtc(path);
                _currentVersion = lastWrite.ToString("yyyy-MM-dd");
            }
        }
    }
    
    private void OnDumpFileChanged(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation("Dump file changed: {File}", e.Name);
        
        var updatedDumps = new List<string> { Path.GetFileNameWithoutExtension(e.Name ?? "") };
        DumpsUpdated?.Invoke(this, new DumpsUpdatedEventArgs(
            _currentVersion,
            DateTime.UtcNow,
            updatedDumps));
    }
    
    public void Dispose()
    {
        _watcher?.Dispose();
    }
}