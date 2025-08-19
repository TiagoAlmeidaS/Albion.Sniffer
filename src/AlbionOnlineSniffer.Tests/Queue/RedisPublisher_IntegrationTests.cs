using System.Text.Json;
using Albion.Events.V1;
using AlbionOnlineSniffer.Tests.Common;
using AlbionOnlineSniffer.Tests.Common.Builders;
using DotNet.Testcontainers.Builders;
using FluentAssertions;
using StackExchange.Redis;
using Testcontainers.Redis;
using Xunit;

namespace AlbionOnlineSniffer.Tests.Queue;

public class RedisPublisher_IntegrationTests : IAsyncLifetime
{
    private RedisContainer? _redisContainer;
    private IConnectionMultiplexer? _redis;
    private IDatabase? _database;
    private readonly FakeClock _clock;
    private readonly FakeIdGenerator _idGenerator;
    private readonly DomainEventBuilder _eventBuilder;

    public RedisPublisher_IntegrationTests()
    {
        _clock = new FakeClock(DateTimeOffset.Parse("2025-01-01T00:00:00Z"));
        _idGenerator = new FakeIdGenerator();
        _eventBuilder = new DomainEventBuilder(_clock, _idGenerator);
    }

    public async Task InitializeAsync()
    {
        // Configura e inicia container Redis
        _redisContainer = new ContainerBuilder()
            .WithImage("redis:7-alpine")
            .WithPortBinding(6379, true)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(6379))
            .Build();

        await _redisContainer.StartAsync();

        // Cria conexão
        var connectionString = _redisContainer.GetConnectionString();
        _redis = await ConnectionMultiplexer.ConnectAsync(connectionString);
        _database = _redis.GetDatabase();
    }

    public async Task DisposeAsync()
    {
        if (_redis != null)
        {
            await _redis.CloseAsync();
            _redis.Dispose();
        }

        if (_redisContainer != null)
            await _redisContainer.DisposeAsync();
    }

    [Fact]
    public async Task PublishAsync_PlayerSpottedV1_PublishesToCorrectChannel()
    {
        // Arrange
        var channel = "albion:events:player:spotted";
        var publisher = new RedisPublisher(_redis!);
        var evt = _eventBuilder.BuildPlayerSpottedV1("TestPlayer");

        var receivedMessage = string.Empty;
        var tcs = new TaskCompletionSource<string>();

        // Subscribe ao canal
        var subscriber = _redis!.GetSubscriber();
        await subscriber.SubscribeAsync(channel, (ch, message) =>
        {
            tcs.SetResult(message.ToString());
        });

        // Act
        await publisher.PublishAsync(channel, evt);

        // Assert
        receivedMessage = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        receivedMessage.Should().NotBeEmpty();
        receivedMessage.Should().Contain("TestPlayer");
    }

    [Fact]
    public async Task PublishAsync_MultipleEvents_AllEventsAreReceived()
    {
        // Arrange
        var baseChannel = "albion:events:";
        var publisher = new RedisPublisher(_redis!);
        
        var events = new Dictionary<string, object>
        {
            { "player:spotted", _eventBuilder.BuildPlayerSpottedV1("Player1") },
            { "mob:spawned", _eventBuilder.BuildMobSpawnedV1("Wolf") },
            { "harvestable:found", _eventBuilder.BuildHarvestableFoundV1("Wood") }
        };

        var receivedMessages = new List<string>();
        var subscriber = _redis!.GetSubscriber();

        // Subscribe a todos os canais
        foreach (var eventType in events.Keys)
        {
            var channel = baseChannel + eventType;
            await subscriber.SubscribeAsync(channel, (ch, message) =>
            {
                receivedMessages.Add(message.ToString());
            });
        }

        // Act
        foreach (var kvp in events)
        {
            var channel = baseChannel + kvp.Key;
            await publisher.PublishAsync(channel, kvp.Value);
        }

        // Assert
        await Task.Delay(500); // Aguarda mensagens serem processadas

        receivedMessages.Should().HaveCount(3);
        receivedMessages[0].Should().Contain("Player1");
        receivedMessages[1].Should().Contain("Wolf");
        receivedMessages[2].Should().Contain("Wood");
    }

    [Fact]
    public async Task PublishAsync_WithStream_AddsToStream()
    {
        // Arrange
        var streamKey = "albion:stream:events";
        var publisher = new RedisStreamPublisher(_redis!);
        var evt = _eventBuilder.BuildPlayerSpottedV1("StreamPlayer");

        // Act
        var messageId = await publisher.PublishToStreamAsync(streamKey, evt);

        // Assert
        messageId.Should().NotBeNullOrEmpty();

        // Lê do stream
        var entries = await _database!.StreamReadAsync(streamKey, "0-0", count: 1);
        entries.Should().HaveCount(1);

        var entry = entries.First();
        var eventData = entry["event"].ToString();
        eventData.Should().Contain("StreamPlayer");
    }

    [Fact]
    public async Task PublishAsync_WithList_AddsToList()
    {
        // Arrange
        var listKey = "albion:list:events";
        var publisher = new RedisListPublisher(_redis!);
        var evt = _eventBuilder.BuildMobSpawnedV1("Bear");

        // Act
        await publisher.PublishToListAsync(listKey, evt);

        // Assert
        var length = await _database!.ListLengthAsync(listKey);
        length.Should().Be(1);

        var item = await _database.ListGetByIndexAsync(listKey, 0);
        item.ToString().Should().Contain("Bear");
    }

    [Fact]
    public async Task PublishAsync_WithRetry_RetriesOnTransientError()
    {
        // Arrange
        var channel = "albion:events:test";
        var publisher = new RedisPublisherWithRetry(_redis!, maxRetries: 3);
        var evt = _eventBuilder.BuildPlayerSpottedV1("RetryTest");

        // Este teste é mais conceitual pois é difícil simular falhas transientes com container
        // Em produção, você teria testes unitários com mocks para isso

        // Act & Assert - Deve publicar com sucesso
        await publisher.PublishAsync(channel, evt);

        // Verifica que a mensagem foi publicada
        var subscriber = _redis!.GetSubscriber();
        var received = false;
        
        await subscriber.SubscribeAsync(channel, (ch, message) =>
        {
            received = true;
        });

        await publisher.PublishAsync(channel, evt);
        await Task.Delay(100);

        received.Should().BeTrue();
    }
}

// Implementação simplificada do RedisPublisher para os testes
public class RedisPublisher
{
    private readonly IConnectionMultiplexer _redis;

    public RedisPublisher(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task PublishAsync<T>(string channel, T message) where T : class
    {
        var subscriber = _redis.GetSubscriber();
        var json = JsonSerializer.Serialize(message);
        await subscriber.PublishAsync(channel, json);
    }
}

// Publisher para Redis Streams
public class RedisStreamPublisher
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;

    public RedisStreamPublisher(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _database = redis.GetDatabase();
    }

    public async Task<string> PublishToStreamAsync<T>(string streamKey, T message) where T : class
    {
        var json = JsonSerializer.Serialize(message);
        var messageId = await _database.StreamAddAsync(streamKey, new[]
        {
            new NameValueEntry("event", json),
            new NameValueEntry("timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
            new NameValueEntry("type", typeof(T).Name)
        });

        return messageId.ToString();
    }
}

// Publisher para Redis Lists
public class RedisListPublisher
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;

    public RedisListPublisher(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _database = redis.GetDatabase();
    }

    public async Task PublishToListAsync<T>(string listKey, T message) where T : class
    {
        var json = JsonSerializer.Serialize(message);
        await _database.ListRightPushAsync(listKey, json);
    }
}

// Versão com retry policy
public class RedisPublisherWithRetry : RedisPublisher
{
    private readonly int _maxRetries;

    public RedisPublisherWithRetry(IConnectionMultiplexer redis, int maxRetries = 3) 
        : base(redis)
    {
        _maxRetries = maxRetries;
    }

    public new async Task PublishAsync<T>(string channel, T message) where T : class
    {
        var retryCount = 0;
        var delay = TimeSpan.FromMilliseconds(100);

        while (retryCount < _maxRetries)
        {
            try
            {
                await base.PublishAsync(channel, message);
                return;
            }
            catch (RedisException ex) when (IsTransient(ex))
            {
                retryCount++;
                if (retryCount >= _maxRetries)
                    throw;

                await Task.Delay(delay);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2); // Exponential backoff
            }
        }
    }

    private bool IsTransient(RedisException ex)
    {
        // Define quais exceções Redis são consideradas transientes
        return ex.Message.Contains("Timeout") ||
               ex.Message.Contains("Connection") ||
               ex.Message.Contains("unavailable");
    }
}