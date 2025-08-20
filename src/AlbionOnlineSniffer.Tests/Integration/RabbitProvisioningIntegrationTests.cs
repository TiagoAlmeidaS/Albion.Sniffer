using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Testcontainers.RabbitMq;
using Xunit;
using AlbionOnlineSniffer.Options;
using AlbionOnlineSniffer.Queue.Services;

namespace AlbionOnlineSniffer.Tests.Integration;

[Collection("RabbitMQ Integration")]
public class RabbitProvisioningIntegrationTests : IAsyncLifetime
{
    private RabbitMqContainer? _rabbitContainer;
    private string? _connectionString;

    public async Task InitializeAsync()
    {
        _rabbitContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3.12-management-alpine")
            .WithUsername("test")
            .WithPassword("test")
            .Build();

        await _rabbitContainer.StartAsync();
        _connectionString = _rabbitContainer.GetConnectionString();
    }

    public async Task DisposeAsync()
    {
        if (_rabbitContainer != null)
        {
            await _rabbitContainer.DisposeAsync();
        }
    }

    [Fact]
    public async Task Provisioner_CreatesExchangeAndQueues_WhenEnabled()
    {
        // Skip if not in CI or if RabbitMQ is not available
        if (string.IsNullOrEmpty(_connectionString))
        {
            return;
        }

        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        
        var provisioningOptions = new MessagingProvisioningOptions
        {
            Enabled = true,
            ConnectionString = _connectionString,
            Exchange = new ExchangeOptions
            {
                Name = "test.events",
                Type = "topic",
                Durable = true
            },
            Queues = new List<QueueOptions>
            {
                new QueueOptions
                {
                    Name = "test.queue1",
                    Durable = true,
                    Bindings = new List<string> { "test.event.*.v1" }
                },
                new QueueOptions
                {
                    Name = "test.queue2",
                    Durable = true,
                    Arguments = new Dictionary<string, object>
                    {
                        ["x-message-ttl"] = 60000,
                        ["x-max-length"] = 100
                    },
                    Bindings = new List<string> { "test.event.#" }
                }
            },
            EnableInEnvironments = new List<string>() // Empty to allow all environments
        };

        var publishingSettings = new PublishingSettings
        {
            ConnectionString = _connectionString!
        };

        services.AddSingleton(Options.Create(provisioningOptions));
        services.AddSingleton(Options.Create(publishingSettings));
        services.AddSingleton<IHostEnvironment>(new TestHostEnvironment());
        services.AddHostedService<RabbitTopologyProvisioner>();

        var serviceProvider = services.BuildServiceProvider();
        var provisioner = serviceProvider.GetRequiredService<IHostedService>();

        // Act
        await provisioner.StartAsync(CancellationToken.None);

        // Assert - Verify the topology was created
        var factory = new ConnectionFactory { Uri = new Uri(_connectionString!) };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        // Exchange should exist - this will throw if it doesn't exist with the same parameters
        channel.ExchangeDeclarePassive("test.events");

        // Queues should exist
        var queue1 = channel.QueueDeclarePassive("test.queue1");
        Assert.NotNull(queue1);
        
        var queue2 = channel.QueueDeclarePassive("test.queue2");
        Assert.NotNull(queue2);

        // Cleanup
        await provisioner.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Provisioner_HandlesExistingTopology_Gracefully()
    {
        // Skip if not in CI or if RabbitMQ is not available
        if (string.IsNullOrEmpty(_connectionString))
        {
            return;
        }

        // Arrange - Pre-create some topology
        var factory = new ConnectionFactory { Uri = new Uri(_connectionString!) };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.ExchangeDeclare("preexisting.exchange", "topic", durable: true);
            channel.QueueDeclare("preexisting.queue", durable: true, exclusive: false, autoDelete: false);
            channel.QueueBind("preexisting.queue", "preexisting.exchange", "test.#");
        }

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        
        var provisioningOptions = new MessagingProvisioningOptions
        {
            Enabled = true,
            ConnectionString = _connectionString,
            Exchange = new ExchangeOptions
            {
                Name = "preexisting.exchange",
                Type = "topic",
                Durable = true
            },
            Queues = new List<QueueOptions>
            {
                new QueueOptions
                {
                    Name = "preexisting.queue",
                    Durable = true,
                    Bindings = new List<string> { "test.#", "another.#" }
                }
            },
            EnableInEnvironments = new List<string>()
        };

        var publishingSettings = new PublishingSettings
        {
            ConnectionString = _connectionString!
        };

        services.AddSingleton(Options.Create(provisioningOptions));
        services.AddSingleton(Options.Create(publishingSettings));
        services.AddSingleton<IHostEnvironment>(new TestHostEnvironment());
        services.AddHostedService<RabbitTopologyProvisioner>();

        var serviceProvider = services.BuildServiceProvider();
        var provisioner = serviceProvider.GetRequiredService<IHostedService>();

        // Act - Should not throw even though topology exists
        await provisioner.StartAsync(CancellationToken.None);

        // Assert - Verify the topology still exists
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.ExchangeDeclarePassive("preexisting.exchange");
            var queue = channel.QueueDeclarePassive("preexisting.queue");
            Assert.NotNull(queue);
        }

        // Cleanup
        await provisioner.StopAsync(CancellationToken.None);
    }

    private class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Testing";
        public string ApplicationName { get; set; } = "TestApp";
        public string ContentRootPath { get; set; } = "/test";
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}