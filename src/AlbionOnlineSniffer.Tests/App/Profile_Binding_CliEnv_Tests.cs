using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace AlbionOnlineSniffer.Tests.App;

public class Profile_Binding_CliEnv_Tests
{
    [Fact]
    public void Configuration_CliArgument_TakesPrecedence()
    {
        // Arrange
        var args = new[] { "--profile", "CliProfile", "--queue", "RabbitMQ" };
        
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                // Adiciona configurações em ordem de precedência (menor para maior)
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Profile"] = "DefaultProfile",
                    ["Queue"] = "Redis"
                });
                
                config.AddEnvironmentVariables("SNIFFER_");
                config.AddCommandLine(args);
            })
            .Build();

        // Act
        var configuration = host.Services.GetRequiredService<IConfiguration>();
        var profile = configuration["profile"];
        var queue = configuration["queue"];

        // Assert
        profile.Should().Be("CliProfile");
        queue.Should().Be("RabbitMQ");
    }

    [Fact]
    public void Configuration_EnvironmentVariable_OverridesFile()
    {
        // Arrange
        Environment.SetEnvironmentVariable("SNIFFER_PROFILE", "EnvProfile");
        Environment.SetEnvironmentVariable("SNIFFER__ACTIVEPROFILE", "EnvActiveProfile");
        
        try
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Profile"] = "FileProfile",
                        ["ActiveProfile"] = "FileActiveProfile"
                    });
                    
                    config.AddEnvironmentVariables("SNIFFER_");
                })
                .Build();

            // Act
            var configuration = host.Services.GetRequiredService<IConfiguration>();
            var profile = configuration["Profile"];
            var activeProfile = configuration["ActiveProfile"];

            // Assert
            profile.Should().Be("EnvProfile");
            activeProfile.Should().Be("EnvActiveProfile");
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("SNIFFER_PROFILE", null);
            Environment.SetEnvironmentVariable("SNIFFER__ACTIVEPROFILE", null);
        }
    }

    [Theory]
    [InlineData("--profile", "TestProfile", "Profile", "TestProfile")]
    [InlineData("--device", "eth0", "Device", "eth0")]
    [InlineData("--queue-provider", "Redis", "QueueProvider", "Redis")]
    [InlineData("--enable-metrics", "true", "EnableMetrics", "true")]
    public void Configuration_VariousCliArguments_BindCorrectly(
        string argName, string argValue, string configKey, string expectedValue)
    {
        // Arrange
        var args = new[] { argName, argValue };
        
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddCommandLine(args, new Dictionary<string, string>
                {
                    { "--profile", "Profile" },
                    { "--device", "Device" },
                    { "--queue-provider", "QueueProvider" },
                    { "--enable-metrics", "EnableMetrics" }
                });
            })
            .Build();

        // Act
        var configuration = host.Services.GetRequiredService<IConfiguration>();
        var value = configuration[configKey];

        // Assert
        value.Should().Be(expectedValue);
    }

    [Fact]
    public void Configuration_PrecedenceOrder_WorksCorrectly()
    {
        // Arrange
        var args = new[] { "--profile", "CliProfile" };
        Environment.SetEnvironmentVariable("SNIFFER_PROFILE", "EnvProfile");
        
        try
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    // Ordem: File -> Env -> CLI
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Profile"] = "FileProfile"
                    });
                    
                    config.AddEnvironmentVariables("SNIFFER_");
                    config.AddCommandLine(args);
                })
                .Build();

            // Act
            var configuration = host.Services.GetRequiredService<IConfiguration>();
            var profile = configuration["profile"];

            // Assert - CLI deve ter precedência máxima
            profile.Should().Be("CliProfile");
            
            // Teste sem CLI argument
            host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Profile"] = "FileProfile"
                    });
                    
                    config.AddEnvironmentVariables("SNIFFER_");
                })
                .Build();
            
            configuration = host.Services.GetRequiredService<IConfiguration>();
            profile = configuration["Profile"];
            
            // Env deve ter precedência sobre File
            profile.Should().Be("EnvProfile");
        }
        finally
        {
            Environment.SetEnvironmentVariable("SNIFFER_PROFILE", null);
        }
    }

    [Fact]
    public void Configuration_ComplexNestedSettings_BindCorrectly()
    {
        // Arrange
        var args = new[]
        {
            "--PacketCaptureSettings:Enabled", "true",
            "--PacketCaptureSettings:DeviceIndex", "1",
            "--QueueSettings:RabbitMQ:Host", "localhost",
            "--QueueSettings:RabbitMQ:Port", "5672"
        };
        
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddCommandLine(args);
            })
            .Build();

        // Act
        var configuration = host.Services.GetRequiredService<IConfiguration>();
        
        // Assert
        configuration["PacketCaptureSettings:Enabled"].Should().Be("true");
        configuration["PacketCaptureSettings:DeviceIndex"].Should().Be("1");
        configuration["QueueSettings:RabbitMQ:Host"].Should().Be("localhost");
        configuration["QueueSettings:RabbitMQ:Port"].Should().Be("5672");
        
        // Teste binding para objeto
        var captureSettings = configuration.GetSection("PacketCaptureSettings").Get<PacketCaptureSettings>();
        captureSettings.Should().NotBeNull();
        captureSettings!.Enabled.Should().BeTrue();
        captureSettings.DeviceIndex.Should().Be(1);
    }

    [Fact]
    public void Configuration_ProfileSpecificSettings_LoadCorrectly()
    {
        // Arrange
        var profiles = new[] { "Development", "Production", "Testing" };
        
        foreach (var profile in profiles)
        {
            var host = Host.CreateDefaultBuilder()
                .UseEnvironment(profile)
                .ConfigureAppConfiguration((context, config) =>
                {
                    // Simula diferentes configurações por profile
                    var settings = profile switch
                    {
                        "Development" => new Dictionary<string, string?>
                        {
                            ["LogLevel"] = "Debug",
                            ["EnableDetailedErrors"] = "true"
                        },
                        "Production" => new Dictionary<string, string?>
                        {
                            ["LogLevel"] = "Warning",
                            ["EnableDetailedErrors"] = "false"
                        },
                        "Testing" => new Dictionary<string, string?>
                        {
                            ["LogLevel"] = "Information",
                            ["EnableDetailedErrors"] = "true"
                        },
                        _ => new Dictionary<string, string?>()
                    };
                    
                    config.AddInMemoryCollection(settings);
                })
                .Build();

            // Act
            var configuration = host.Services.GetRequiredService<IConfiguration>();
            var logLevel = configuration["LogLevel"];
            var detailedErrors = configuration["EnableDetailedErrors"];

            // Assert
            switch (profile)
            {
                case "Development":
                    logLevel.Should().Be("Debug");
                    detailedErrors.Should().Be("true");
                    break;
                case "Production":
                    logLevel.Should().Be("Warning");
                    detailedErrors.Should().Be("false");
                    break;
                case "Testing":
                    logLevel.Should().Be("Information");
                    detailedErrors.Should().Be("true");
                    break;
            }
            
            host.Dispose();
        }
    }

    // Classes auxiliares para binding
    private class PacketCaptureSettings
    {
        public bool Enabled { get; set; }
        public int DeviceIndex { get; set; }
        public string? Filter { get; set; }
    }

    private class QueueSettings
    {
        public string Provider { get; set; } = "InMemory";
        public RabbitMQSettings? RabbitMQ { get; set; }
        public RedisSettings? Redis { get; set; }
    }

    private class RabbitMQSettings
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string Username { get; set; } = "guest";
        public string Password { get; set; } = "guest";
    }

    private class RedisSettings
    {
        public string ConnectionString { get; set; } = "localhost:6379";
        public int Database { get; set; } = 0;
    }
}