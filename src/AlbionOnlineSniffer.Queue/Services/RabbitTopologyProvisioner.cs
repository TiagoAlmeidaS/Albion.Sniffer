using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using AlbionOnlineSniffer.Options;

namespace AlbionOnlineSniffer.Queue.Services;

/// <summary>
/// Hosted service that provisions RabbitMQ topology on startup
/// </summary>
public sealed class RabbitTopologyProvisioner : IHostedService
{
    private readonly MessagingProvisioningOptions _options;
    private readonly PublishingSettings _publishingSettings;
    private readonly ILogger<RabbitTopologyProvisioner> _logger;
    private readonly IHostEnvironment _environment;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitTopologyProvisioner(
        IOptions<MessagingProvisioningOptions> options,
        IOptions<PublishingSettings> publishingSettings,
        ILogger<RabbitTopologyProvisioner> logger,
        IHostEnvironment environment)
    {
        _options = options.Value;
        _publishingSettings = publishingSettings.Value;
        _logger = logger;
        _environment = environment;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!ShouldProvision())
        {
            _logger.LogInformation("RabbitMQ topology provisioning is disabled");
            return Task.CompletedTask;
        }

        try
        {
            ProvisionTopology();
            _logger.LogInformation("RabbitMQ topology provisioning completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to provision RabbitMQ topology");
            // Don't throw - allow application to start even if provisioning fails
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error while closing RabbitMQ provisioning connection");
        }

        return Task.CompletedTask;
    }

    private bool ShouldProvision()
    {
        if (!_options.Enabled)
        {
            _logger.LogDebug("Provisioning is disabled in configuration");
            return false;
        }

        if (_options.EnableInEnvironments?.Any() == true)
        {
            var currentEnvironment = _environment.EnvironmentName;
            if (!_options.EnableInEnvironments.Contains(currentEnvironment, StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogDebug("Provisioning is not enabled for environment: {Environment}", currentEnvironment);
                return false;
            }
        }

        return true;
    }

    private void ProvisionTopology()
    {
        var connectionString = _options.ConnectionString ?? _publishingSettings.ConnectionString;
        
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("No connection string configured for RabbitMQ provisioning");
        }

        _logger.LogInformation("Starting RabbitMQ topology provisioning");

        // Create connection
        var factory = new ConnectionFactory
        {
            Uri = new Uri(connectionString),
            AutomaticRecoveryEnabled = false // We're just provisioning, not maintaining connection
        };

        _connection = factory.CreateConnection("AlbionSniffer-Provisioner");
        _channel = _connection.CreateModel();

        // Provision main exchange
        if (_options.Exchange != null)
        {
            ProvisionExchange(_options.Exchange);
        }

        // Provision dead letter exchange
        if (_options.DeadLetterExchange != null)
        {
            ProvisionExchange(_options.DeadLetterExchange);
        }

        // Provision dead letter queue first (if configured)
        if (_options.DeadLetterQueue != null)
        {
            ProvisionQueue(_options.DeadLetterQueue, _options.DeadLetterExchange?.Name);
        }

        // Provision regular queues
        foreach (var queueOptions in _options.Queues ?? Enumerable.Empty<QueueOptions>())
        {
            var exchangeName = queueOptions.Exchange ?? _options.Exchange?.Name;
            ProvisionQueue(queueOptions, exchangeName);
        }

        _logger.LogInformation("RabbitMQ topology provisioning completed");
    }

    private void ProvisionExchange(ExchangeOptions exchange)
    {
        try
        {
            _logger.LogDebug("Declaring exchange: {Exchange} (type: {Type}, durable: {Durable})",
                exchange.Name, exchange.Type, exchange.Durable);

            _channel!.ExchangeDeclare(
                exchange: exchange.Name,
                type: exchange.Type,
                durable: exchange.Durable,
                autoDelete: exchange.AutoDelete,
                arguments: exchange.Arguments);

            _logger.LogInformation("Exchange provisioned: {Exchange}", exchange.Name);
        }
        catch (OperationInterruptedException ex) when (ex.ShutdownReason?.ReplyCode == 406)
        {
            _logger.LogWarning("Exchange {Exchange} already exists with different parameters", exchange.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to provision exchange: {Exchange}", exchange.Name);
            throw;
        }
    }

    private void ProvisionQueue(QueueOptions queue, string? exchangeName)
    {
        try
        {
            // Add dead letter exchange if configured globally and not already present
            var arguments = new Dictionary<string, object>(queue.Arguments ?? new Dictionary<string, object>());
            
            if (_options.DeadLetterExchange != null && 
                !arguments.ContainsKey("x-dead-letter-exchange"))
            {
                arguments["x-dead-letter-exchange"] = _options.DeadLetterExchange.Name;
            }

            _logger.LogDebug("Declaring queue: {Queue} (durable: {Durable}, exclusive: {Exclusive})",
                queue.Name, queue.Durable, queue.Exclusive);

            _channel!.QueueDeclare(
                queue: queue.Name,
                durable: queue.Durable,
                exclusive: queue.Exclusive,
                autoDelete: queue.AutoDelete,
                arguments: arguments);

            _logger.LogInformation("Queue provisioned: {Queue}", queue.Name);

            // Create bindings
            if (!string.IsNullOrWhiteSpace(exchangeName) && queue.Bindings?.Any() == true)
            {
                foreach (var routingKey in queue.Bindings)
                {
                    try
                    {
                        _logger.LogDebug("Creating binding: {Queue} -> {Exchange} (routing key: {RoutingKey})",
                            queue.Name, exchangeName, routingKey);

                        _channel.QueueBind(
                            queue: queue.Name,
                            exchange: exchangeName,
                            routingKey: routingKey);

                        _logger.LogInformation("Binding created: {Queue} -> {RoutingKey}", queue.Name, routingKey);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to create binding: {Queue} -> {RoutingKey}", 
                            queue.Name, routingKey);
                    }
                }
            }
        }
        catch (OperationInterruptedException ex) when (ex.ShutdownReason?.ReplyCode == 406)
        {
            _logger.LogWarning("Queue {Queue} already exists with different parameters", queue.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to provision queue: {Queue}", queue.Name);
            throw;
        }
    }
}

/// <summary>
/// Extension methods for configuring RabbitMQ topology provisioning
/// </summary>
public static class RabbitTopologyProvisionerExtensions
{
    /// <summary>
    /// Add RabbitMQ topology provisioning to the service collection
    /// </summary>
    public static IServiceCollection AddRabbitTopologyProvisioning(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MessagingProvisioningOptions>(
            configuration.GetSection("MessagingProvisioning"));
        
        services.AddHostedService<RabbitTopologyProvisioner>();
        
        return services;
    }

    /// <summary>
    /// Add RabbitMQ topology provisioning with custom configuration
    /// </summary>
    public static IServiceCollection AddRabbitTopologyProvisioning(
        this IServiceCollection services,
        Action<MessagingProvisioningOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddHostedService<RabbitTopologyProvisioner>();
        
        return services;
    }
}