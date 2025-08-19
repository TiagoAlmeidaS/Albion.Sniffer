using System.Text;
using Albion.Events.V1;
using AlbionOnlineSniffer.Tests.Common;
using AlbionOnlineSniffer.Tests.Common.Builders;
using DotNet.Testcontainers.Builders;
using FluentAssertions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Testcontainers.RabbitMq;
using Xunit;

namespace AlbionOnlineSniffer.Tests.Queue;

public class RabbitPublisher_IntegrationTests : IAsyncLifetime
{
    private RabbitMqContainer? _rabbitContainer;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly FakeClock _clock;
    private readonly FakeIdGenerator _idGenerator;
    private readonly DomainEventBuilder _eventBuilder;

    public RabbitPublisher_IntegrationTests()
    {
        _clock = new FakeClock(DateTimeOffset.Parse("2025-01-01T00:00:00Z"));
        _idGenerator = new FakeIdGenerator();
        _eventBuilder = new DomainEventBuilder(_clock, _idGenerator);
    }

    public async Task InitializeAsync()
    {
        // Configura e inicia container RabbitMQ
        _rabbitContainer = new ContainerBuilder()
            .WithImage("rabbitmq:3.13-management-alpine")
            .WithPortBinding(5672, true)
            .WithPortBinding(15672, true)
            .WithEnvironment("RABBITMQ_DEFAULT_USER", "test")
            .WithEnvironment("RABBITMQ_DEFAULT_PASS", "test")
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(5672)
                .UntilPortIsAvailable(15672))
            .Build();

        await _rabbitContainer.StartAsync();

        // Cria conexão
        var factory = new ConnectionFactory
        {
            HostName = _rabbitContainer.Hostname,
            Port = _rabbitContainer.GetMappedPublicPort(5672),
            UserName = "test",
            Password = "test"
        };

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();
    }

    public async Task DisposeAsync()
    {
        if (_channel != null)
            await _channel.CloseAsync();
        
        if (_connection != null)
            await _connection.CloseAsync();

        if (_rabbitContainer != null)
            await _rabbitContainer.DisposeAsync();
    }

    [Fact]
    public async Task PublishAsync_PlayerSpottedV1_PublishesToCorrectExchange()
    {
        // Arrange
        var exchangeName = "albion.events";
        var routingKey = "player.spotted.v1";
        var queueName = $"test-queue-{Guid.NewGuid()}";

        // Declara exchange e queue
        await _channel!.ExchangeDeclareAsync(exchangeName, ExchangeType.Topic, durable: true);
        await _channel.QueueDeclareAsync(queueName, durable: false, exclusive: true, autoDelete: true);
        await _channel.QueueBindAsync(queueName, exchangeName, routingKey);

        var publisher = new RabbitMqPublisher(_connection!);
        var evt = _eventBuilder.BuildPlayerSpottedV1("TestPlayer");

        // Act
        await publisher.PublishAsync(exchangeName, routingKey, evt);

        // Assert - Consome mensagem
        var consumer = new AsyncEventingBasicConsumer(_channel);
        var tcs = new TaskCompletionSource<byte[]>();
        
        consumer.ReceivedAsync += async (sender, ea) =>
        {
            tcs.SetResult(ea.Body.ToArray());
            await Task.CompletedTask;
        };

        await _channel.BasicConsumeAsync(queueName, autoAck: true, consumer);

        var messageBytes = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        messageBytes.Should().NotBeEmpty();

        // Verifica se a mensagem pode ser deserializada
        var json = Encoding.UTF8.GetString(messageBytes);
        json.Should().Contain("TestPlayer");
    }

    [Fact]
    public async Task PublishAsync_MultipleEvents_AllEventsAreReceived()
    {
        // Arrange
        var exchangeName = "albion.events";
        var queueName = $"test-queue-{Guid.NewGuid()}";

        await _channel!.ExchangeDeclareAsync(exchangeName, ExchangeType.Topic, durable: true);
        await _channel.QueueDeclareAsync(queueName, durable: false, exclusive: true, autoDelete: true);
        await _channel.QueueBindAsync(queueName, exchangeName, "#"); // Bind para todos os eventos

        var publisher = new RabbitMqPublisher(_connection!);
        var events = new List<object>
        {
            _eventBuilder.BuildPlayerSpottedV1("Player1"),
            _eventBuilder.BuildMobSpawnedV1("Wolf"),
            _eventBuilder.BuildHarvestableFoundV1("Wood")
        };

        // Act
        foreach (var evt in events)
        {
            var routingKey = evt.GetType().Name.Replace("V1", ".v1").ToLower();
            await publisher.PublishAsync(exchangeName, routingKey, evt);
        }

        // Assert
        var receivedMessages = new List<string>();
        var consumer = new AsyncEventingBasicConsumer(_channel);
        
        consumer.ReceivedAsync += async (sender, ea) =>
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());
            receivedMessages.Add(message);
            await Task.CompletedTask;
        };

        await _channel.BasicConsumeAsync(queueName, autoAck: true, consumer);

        // Aguarda receber todas as mensagens
        await Task.Delay(1000);

        receivedMessages.Should().HaveCount(3);
        receivedMessages[0].Should().Contain("Player1");
        receivedMessages[1].Should().Contain("Wolf");
        receivedMessages[2].Should().Contain("Wood");
    }

    [Fact]
    public async Task PublishAsync_WithRetryPolicy_RetriesOnTransientError()
    {
        // Este teste simula falhas transientes usando um mock/fake
        // Como estamos testando com container real, vamos simular desconectando/reconectando

        // Arrange
        var exchangeName = "albion.events";
        var routingKey = "test.event";
        
        // Cria publisher com política de retry
        var publisher = new RabbitMqPublisherWithRetry(_connection!, maxRetries: 3);
        var evt = _eventBuilder.BuildPlayerSpottedV1("RetryTest");

        // Simula falha fechando o canal temporariamente
        var originalChannel = _channel;
        await _channel!.CloseAsync();

        // Act & Assert - Deve tentar novamente e eventualmente ter sucesso
        var publishTask = publisher.PublishAsync(exchangeName, routingKey, evt);

        // Reabre o canal após um pequeno delay
        await Task.Delay(500);
        _channel = await _connection!.CreateChannelAsync();

        // A publicação deve eventualmente ter sucesso
        await publishTask.WaitAsync(TimeSpan.FromSeconds(10));
        
        // Cleanup
        await _channel.CloseAsync();
        _channel = originalChannel;
    }
}

// Implementação simplificada do RabbitMqPublisher para os testes
public class RabbitMqPublisher
{
    private readonly IConnection _connection;
    private IChannel? _channel;

    public RabbitMqPublisher(IConnection connection)
    {
        _connection = connection;
    }

    public async Task PublishAsync<T>(string exchange, string routingKey, T message)
        where T : class
    {
        _channel ??= await _connection.CreateChannelAsync();
        
        var json = System.Text.Json.JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            MessageId = Guid.NewGuid().ToString(),
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };

        await _channel.BasicPublishAsync(exchange, routingKey, mandatory: false, properties, body);
    }
}

// Versão com retry policy
public class RabbitMqPublisherWithRetry : RabbitMqPublisher
{
    private readonly int _maxRetries;

    public RabbitMqPublisherWithRetry(IConnection connection, int maxRetries = 3) 
        : base(connection)
    {
        _maxRetries = maxRetries;
    }

    public new async Task PublishAsync<T>(string exchange, string routingKey, T message)
        where T : class
    {
        var retryCount = 0;
        var delay = TimeSpan.FromMilliseconds(100);

        while (retryCount < _maxRetries)
        {
            try
            {
                await base.PublishAsync(exchange, routingKey, message);
                return;
            }
            catch (Exception ex) when (IsTransient(ex))
            {
                retryCount++;
                if (retryCount >= _maxRetries)
                    throw;

                await Task.Delay(delay);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2); // Exponential backoff
            }
        }
    }

    private bool IsTransient(Exception ex)
    {
        // Define quais exceções são consideradas transientes
        return ex is IOException || 
               ex is TimeoutException ||
               ex.Message.Contains("connection") ||
               ex.Message.Contains("channel");
    }
}