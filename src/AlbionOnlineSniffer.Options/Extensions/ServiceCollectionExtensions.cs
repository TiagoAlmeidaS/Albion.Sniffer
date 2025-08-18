using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Options.Profiles;
using AlbionOnlineSniffer.Options.Enrichers;
using Microsoft.Extensions.Hosting;

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
					options.GetActiveProfile();
					break;
				}
			}
			
			// Check for profile override from environment variable
			var envProfile = Environment.GetEnvironmentVariable("SNIFFER_PROFILE");
			if (!string.IsNullOrEmpty(envProfile))
			{
				options.ActiveProfile = envProfile;
			}
			// Support hierarchical env binding SNIFFER__ACTIVEPROFILE
			var envProfile2 = Environment.GetEnvironmentVariable("SNIFFER__ACTIVEPROFILE");
			if (!string.IsNullOrEmpty(envProfile2))
			{
				options.ActiveProfile = envProfile2;
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

	/// <summary>
	/// Adds profile selection wiring and a basic default enricher to confirm cycle
	/// </summary>
	public static IServiceCollection AddProfileSelection(
		this IServiceCollection services,
		IConfiguration configuration,
		IHostEnvironment? environment = null)
	{
		services.AddSnifferOptions(configuration);
		services.AddProfileManagement();
		services.ValidateOptionsOnStart<SnifferOptions>();
		
		// Log active profile on startup
		services.AddSingleton<IStartupFilter, ProfileLogStartupFilter>();
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

internal class ProfileLogStartupFilter : IStartupFilter
{
	private readonly IOptions<SnifferOptions> _options;
	private readonly ILogger<ProfileLogStartupFilter> _logger;
	
	public ProfileLogStartupFilter(IOptions<SnifferOptions> options, ILogger<ProfileLogStartupFilter> logger)
	{
		_options = options;
		_logger = logger;
	}
	
	public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
	{
		var source = "appsettings";
		var args = Environment.GetCommandLineArgs();
		for (int i = 0; i < args.Length - 1; i++)
		{
			if (args[i] == "--profile" && i + 1 < args.Length)
			{
				source = "cli";
				break;
			}
		}
		var envProfile = Environment.GetEnvironmentVariable("SNIFFER_PROFILE");
		var envProfile2 = Environment.GetEnvironmentVariable("SNIFFER__ACTIVEPROFILE");
		if (!string.IsNullOrEmpty(envProfile) || !string.IsNullOrEmpty(envProfile2))
		{
			source = "env";
		}
		var profile = _options.Value.GetActiveProfile();
		_logger.LogInformation("Active profile resolved: {Profile} (source={Source})", profile.Name, source);
		return next;
	}
}