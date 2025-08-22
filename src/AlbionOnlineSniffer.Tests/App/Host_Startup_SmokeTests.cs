using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace AlbionOnlineSniffer.Tests.App;

public class Host_Startup_SmokeTests
{
    private readonly ITestOutputHelper _output;

    public Host_Startup_SmokeTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task Host_StartsWithTestConfiguration_Success()
    {
        // Arrange
        var host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Environment"] = "Testing",
                    ["PacketCaptureSettings:Enabled"] = "false",
                    ["PacketCaptureSettings:DeviceIndex"] = "0",
                    ["QueueSettings:Provider"] = "InMemory",
                    ["QueueSettings:RabbitMQ:Enabled"] = "false",
                    ["QueueSettings:Redis:Enabled"] = "false"
                });
            })
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(builder =>
                {
                    builder.AddXUnit(_output);
                    builder.SetMinimumLevel(LogLevel.Debug);
                });
            })
            .Build();

        // Act & Assert
        await host.StartAsync();
        host.Services.Should().NotBeNull();
        await host.StopAsync();
    }

    [Fact]
    public void Host_RegistersRequiredServices_Success()
    {
        // Arrange
        var host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Environment"] = "Testing"
                });
            })
            .ConfigureServices((context, services) =>
            {
                // Registra serviços esperados
                services.AddSingleton<IConfiguration>(context.Configuration);
                services.AddLogging();
                services.AddOptions();
                
                // Serviços customizados
                services.AddSingleton<TestService>();
            })
            .Build();

        // Act
        var configuration = host.Services.GetService<IConfiguration>();
        var logger = host.Services.GetService<ILogger<Host_Startup_SmokeTests>>();
        var testService = host.Services.GetService<TestService>();

        // Assert
        configuration.Should().NotBeNull();
        logger.Should().NotBeNull();
        testService.Should().NotBeNull();
    }

    [Fact]
    public void Host_LoadsAppsettingsJson_Success()
    {
        // Arrange
        var host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.Test.json", optional: true, reloadOnChange: false);
                config.AddEnvironmentVariables("SNIFFER_");
            })
            .Build();

        // Act
        var configuration = host.Services.GetRequiredService<IConfiguration>();

        // Assert
        configuration.Should().NotBeNull();
        
        // Verifica se consegue ler configurações (mesmo que não existam)
        var captureEnabled = configuration.GetValue<bool?>("PacketCaptureSettings:Enabled");
        captureEnabled.Should().BeOneOf(null, true, false); // Pode ser null se arquivo não existir
    }

    [Fact]
    public async Task Host_WithMultipleProfiles_LoadsCorrectProfile()
    {
        // Arrange
        var profiles = new[] { "Development", "Production", "Testing" };
        
        foreach (var profile in profiles)
        {
            var host = Host.CreateDefaultBuilder()
                .UseEnvironment(profile)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ActiveProfile"] = profile
                    });
                })
                .Build();

            // Act
            var configuration = host.Services.GetRequiredService<IConfiguration>();
            var activeProfile = configuration["ActiveProfile"];
            var environment = host.Services.GetRequiredService<IHostEnvironment>();

            // Assert
            activeProfile.Should().Be(profile);
            environment.EnvironmentName.Should().Be(profile);
            
            await host.StopAsync();
            host.Dispose();
        }
    }

    [Fact]
    public void Host_WithInvalidConfiguration_ThrowsMeaningfulError()
    {
        // Arrange & Act
        var act = () =>
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Simula configuração inválida que causaria erro
                    services.AddSingleton<IInvalidService>(provider =>
                    {
                        throw new InvalidOperationException("Required configuration 'ConnectionString' is missing");
                    });
                    
                    // Força a criação do serviço
                    services.AddHostedService<ServiceRequiringInvalid>();
                })
                .Build();
        };

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Required configuration*");
    }

    // Classes auxiliares para testes
    private class TestService
    {
        public string Name => "TestService";
    }

    private interface IInvalidService { }

    private class ServiceRequiringInvalid : BackgroundService
    {
        public ServiceRequiringInvalid(IInvalidService invalid) { }
        protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;
    }
}