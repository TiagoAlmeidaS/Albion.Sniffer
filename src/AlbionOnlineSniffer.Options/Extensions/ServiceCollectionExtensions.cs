using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Options.Profiles;
using AlbionOnlineSniffer.Options.Enrichers;

namespace AlbionOnlineSniffer.Options.Extensions;

/// <summary>
/// Extension methods for service collection configuration
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Sniffer options with validation
    /// </summary>
    public static IServiceCollection AddSnifferOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure main options
        services.Configure<SnifferOptions>(configuration.GetSection(SnifferOptions.SectionName));
        
        // Add validation
        services.AddSingleton<IValidateOptions<SnifferOptions>, SnifferOptionsValidator>();
        
        // Add options accessor
        services.AddSingleton(provider => provider.GetRequiredService<IOptions<SnifferOptions>>().Value);
        
        // Configure profile from command line or environment
        services.PostConfigure<SnifferOptions>(options =>
        {
            // Check for profile override from command line
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "--profile" && i + 1 < args.Length)
                {
                    options.ActiveProfile = args[i + 1];
                    break;
                }
            }
            
            // Check for profile override from environment variable
            var envProfile = Environment.GetEnvironmentVariable("SNIFFER_PROFILE");
            if (!string.IsNullOrEmpty(envProfile))
            {
                options.ActiveProfile = envProfile;
            }
        });
        
        return services;
    }

    /// <summary>
    /// Adds profile management services
    /// </summary>
    public static IServiceCollection AddProfileManagement(this IServiceCollection services)
    {
        // Register profile manager
        services.AddSingleton<IProfileManager, ProfileManager>();
        
        // Register tier palette manager
        services.AddSingleton<ITierPaletteManager, TierPaletteManager>();
        
        // Register enrichers
        services.AddTransient<TierColorEnricher>();
        services.AddTransient<ProfileFilterEnricher>();
        services.AddTransient<ProximityAlertEnricher>();
        
        // Register composite enricher
        services.AddSingleton<IEventEnricher>(provider =>
        {
            var profileManager = provider.GetRequiredService<IProfileManager>();
            var logger = provider.GetRequiredService<ILogger<CompositeEventEnricher>>();
            var paletteManager = provider.GetRequiredService<ITierPaletteManager>();
            
            var enrichers = new List<IEventEnricher>
            {
                new TierColorEnricher(
                    provider.GetRequiredService<ILogger<TierColorEnricher>>(),
                    profileManager.CurrentProfile,
                    paletteManager),
                new ProfileFilterEnricher(
                    provider.GetRequiredService<ILogger<ProfileFilterEnricher>>(),
                    profileManager.CurrentProfile),
                new ProximityAlertEnricher(
                    provider.GetRequiredService<ILogger<ProximityAlertEnricher>>(),
                    profileManager.CurrentProfile)
            };
            
            return new CompositeEventEnricher(enrichers, logger);
        });
        
        return services;
    }

    /// <summary>
    /// Validates options on startup
    /// </summary>
    public static IServiceCollection ValidateOptionsOnStart<TOptions>(
        this IServiceCollection services) where TOptions : class
    {
        services.AddTransient<IStartupFilter, OptionsValidationStartupFilter<TOptions>>();
        return services;
    }
}

/// <summary>
/// Startup filter to validate options
/// </summary>
internal class OptionsValidationStartupFilter<TOptions> : IStartupFilter where TOptions : class
{
    private readonly IOptions<TOptions> _options;

    public OptionsValidationStartupFilter(IOptions<TOptions> options)
    {
        _options = options;
    }

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        // Force options validation on startup
        _ = _options.Value;
        return next;
    }
}

/// <summary>
/// Custom application builder interface for console apps
/// </summary>
public interface IApplicationBuilder
{
    IServiceProvider ApplicationServices { get; }
}

/// <summary>
/// Startup filter interface for console apps
/// </summary>
public interface IStartupFilter
{
    Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next);
}