using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using AlbionOnlineSniffer.Options;
using AlbionOnlineSniffer.Providers.Interfaces;

namespace AlbionOnlineSniffer.Providers.Extensions;

/// <summary>
/// Extension methods for configuring providers
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds provider services based on configuration
    /// </summary>
    public static IServiceCollection AddProviders(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add memory cache for providers
        services.AddMemoryCache();
        
        // Add HTTP client for remote providers
        services.AddHttpClient("Providers", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "AlbionSniffer/1.0");
        });
        
        // Register bin dump provider
        services.AddSingleton<IBinDumpProvider>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<SnifferOptions>>();
            return ProviderFactory.CreateBinDumpProvider(provider, options.Value.Parsing);
        });
        
        // Register item metadata provider
        services.AddSingleton<IItemMetadataProvider>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<SnifferOptions>>();
            return ProviderFactory.CreateItemMetadataProvider(provider, options.Value.Parsing);
        });
        
        return services;
    }
    
    /// <summary>
    /// Adds a specific bin dump provider
    /// </summary>
    public static IServiceCollection AddBinDumpProvider<TProvider>(
        this IServiceCollection services)
        where TProvider : class, IBinDumpProvider
    {
        services.AddSingleton<IBinDumpProvider, TProvider>();
        return services;
    }
    
    /// <summary>
    /// Adds a specific item metadata provider
    /// </summary>
    public static IServiceCollection AddItemMetadataProvider<TProvider>(
        this IServiceCollection services)
        where TProvider : class, IItemMetadataProvider
    {
        services.AddSingleton<IItemMetadataProvider, TProvider>();
        return services;
    }
}