using System.ComponentModel.DataAnnotations;

namespace AlbionOnlineSniffer.Options;

/// <summary>
/// Configuration for automatic RabbitMQ topology provisioning
/// </summary>
public sealed class MessagingProvisioningOptions
{
    /// <summary>
    /// Enable automatic topology provisioning on startup
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// RabbitMQ connection string (uses Publishing.ConnectionString if not specified)
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Exchange configuration
    /// </summary>
    [Required]
    public ExchangeOptions Exchange { get; set; } = new();

    /// <summary>
    /// Dead letter exchange configuration
    /// </summary>
    public ExchangeOptions? DeadLetterExchange { get; set; }

    /// <summary>
    /// Queue definitions with bindings
    /// </summary>
    public List<QueueOptions> Queues { get; set; } = new();

    /// <summary>
    /// Dead letter queue configuration
    /// </summary>
    public QueueOptions? DeadLetterQueue { get; set; }

    /// <summary>
    /// Environments where provisioning is enabled
    /// </summary>
    public List<string> EnableInEnvironments { get; set; } = new() { "Development", "Testing" };

    /// <summary>
    /// Log level for provisioning operations
    /// </summary>
    public string LogLevel { get; set; } = "Information";
}

/// <summary>
/// Exchange configuration options
/// </summary>
public sealed class ExchangeOptions
{
    /// <summary>
    /// Exchange name
    /// </summary>
    [Required]
    public string Name { get; set; } = "albion.events";

    /// <summary>
    /// Exchange type (topic, direct, fanout, headers)
    /// </summary>
    public string Type { get; set; } = "topic";

    /// <summary>
    /// Make exchange durable (survives broker restart)
    /// </summary>
    public bool Durable { get; set; } = true;

    /// <summary>
    /// Auto-delete exchange when no longer used
    /// </summary>
    public bool AutoDelete { get; set; } = false;

    /// <summary>
    /// Additional exchange arguments
    /// </summary>
    public Dictionary<string, object> Arguments { get; set; } = new();
}

/// <summary>
/// Queue configuration options
/// </summary>
public sealed class QueueOptions
{
    /// <summary>
    /// Queue name
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Make queue durable (survives broker restart)
    /// </summary>
    public bool Durable { get; set; } = true;

    /// <summary>
    /// Exclusive queue (only accessible by current connection)
    /// </summary>
    public bool Exclusive { get; set; } = false;

    /// <summary>
    /// Auto-delete queue when no longer used
    /// </summary>
    public bool AutoDelete { get; set; } = false;

    /// <summary>
    /// Additional queue arguments (x-message-ttl, x-max-length, etc.)
    /// </summary>
    public Dictionary<string, object> Arguments { get; set; } = new();

    /// <summary>
    /// Routing keys to bind to this queue
    /// </summary>
    public List<string> Bindings { get; set; } = new();

    /// <summary>
    /// Exchange to bind to (defaults to main exchange)
    /// </summary>
    public string? Exchange { get; set; }
}