using Albion.Events.V1;
using AlbionOnlineSniffer.Tests.Common;
using AlbionOnlineSniffer.Tests.Common.Builders;
using AlbionOnlineSniffer.Tests.Common.Fakes;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace AlbionOnlineSniffer.Tests.E2E;

public class Pipeline_EndToEnd_Tests : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private readonly FakeClock _clock;
    private readonly FakeIdGenerator _idGenerator;
    private readonly FakePacketCaptureService _fakeCapture;
    private readonly InMemoryPublisher _inMemoryPublisher;
    private readonly PhotonPacketBuilder _packetBuilder;
    private IHost? _host;

    public Pipeline_EndToEnd_Tests(ITestOutputHelper output)
    {
        _output = output;
        _clock = new FakeClock(DateTimeOffset.Parse("2025-01-01T00:00:00Z"));
        _idGenerator = new FakeIdGenerator();
        _fakeCapture = new FakePacketCaptureService();
        _inMemoryPublisher = new InMemoryPublisher();
        _packetBuilder = new PhotonPacketBuilder();
    }

    public async Task InitializeAsync()
    {
        // Configura o host com serviços fake
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Registra serviços fake
                services.AddSingleton<IClock>(_clock);
                services.AddSingleton<IIdGenerator>(_idGenerator);
                services.AddSingleton<IPacketCaptureService>(_fakeCapture);
                services.AddSingleton<IEventPublisher>(_inMemoryPublisher);
                
                // Registra parser e pipeline
                services.AddSingleton<PhotonParser>();
                services.AddSingleton<EventEnricher>();
                services.AddSingleton<SnifferPipeline>();
                
                // Registra hosted service
                services.AddHostedService<SnifferHostedService>();
                
                // Logging
                services.AddLogging(builder =>
                {
                    builder.AddXUnit(_output);
                    builder.SetMinimumLevel(LogLevel.Debug);
                });
            })
            .Build();

        await _host.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
    }

    [Fact]
    public async Task Pipeline_PlayerSpottedPacket_ProducesEnrichedEvent()
    {
        // Arrange
        var packet = _packetBuilder
            .WithPlayerSpottedData("E2EPlayer", tier: 6, x: 100f, y: 200f, z: 0f)
            .Build();

        // Act
        _fakeCapture.EnqueuePacket(packet);
        await _fakeCapture.StartCaptureAsync();

        // Aguarda processamento
        await Task.Delay(500);

        // Assert
        _inMemoryPublisher.PublishedMessages.Should().NotBeEmpty();
        
        var publishedEvent = _inMemoryPublisher.GetLastPublishedMessage<PlayerSpottedV1>("albion.events.player.spotted.v1");
        publishedEvent.Should().NotBeNull();
        publishedEvent!.Name.Should().Be("E2EPlayer");
        publishedEvent.Tier.Should().Be(6);
        publishedEvent.X.Should().Be(100f);
        publishedEvent.Y.Should().Be(200f);
        
        // Verifica enriquecimento
        publishedEvent.EventId.Should().NotBeEmpty();
        publishedEvent.Timestamp.Should().Be(_clock.UtcNow);
    }

    [Fact]
    public async Task Pipeline_MultiplePackets_ProcessesAllInOrder()
    {
        // Arrange
        var packets = new[]
        {
            _packetBuilder.WithPlayerSpottedData("Player1", 5, 100f, 100f, 0f).Build(),
            _packetBuilder.WithMobSpawnedData("Wolf", 3, 200f, 200f, 0f, 1000).Build(),
            _packetBuilder.WithHarvestableData("Wood", 4, 300f, 300f, 0f, 5).Build()
        };

        // Act
        foreach (var packet in packets)
        {
            _fakeCapture.EnqueuePacket(packet);
        }
        await _fakeCapture.StartCaptureAsync();

        // Aguarda processamento
        await Task.Delay(1000);

        // Assert
        _inMemoryPublisher.PublishedMessages.Should().HaveCount(3);

        var playerEvent = _inMemoryPublisher.GetAllPublishedMessages<PlayerSpottedV1>().FirstOrDefault();
        playerEvent.Should().NotBeNull();
        playerEvent!.Name.Should().Be("Player1");

        var mobEvent = _inMemoryPublisher.GetAllPublishedMessages<MobSpawnedV1>().FirstOrDefault();
        mobEvent.Should().NotBeNull();
        mobEvent!.MobType.Should().Be("Wolf");

        var harvestableEvent = _inMemoryPublisher.GetAllPublishedMessages<HarvestableFoundV1>().FirstOrDefault();
        harvestableEvent.Should().NotBeNull();
        harvestableEvent!.ResourceType.Should().Be("Wood");
    }

    [Fact]
    public async Task Pipeline_InvalidPacket_ContinuesProcessing()
    {
        // Arrange
        var packets = new[]
        {
            _packetBuilder.WithPlayerSpottedData("ValidPlayer1", 5, 100f, 100f, 0f).Build(),
            PhotonPacketBuilder.CreateInvalidPacket(), // Pacote inválido
            _packetBuilder.WithPlayerSpottedData("ValidPlayer2", 6, 200f, 200f, 0f).Build()
        };

        // Act
        foreach (var packet in packets)
        {
            _fakeCapture.EnqueuePacket(packet);
        }
        await _fakeCapture.StartCaptureAsync();

        // Aguarda processamento
        await Task.Delay(1000);

        // Assert - Deve processar apenas os pacotes válidos
        var playerEvents = _inMemoryPublisher.GetAllPublishedMessages<PlayerSpottedV1>();
        playerEvents.Should().HaveCount(2);
        playerEvents[0].Name.Should().Be("ValidPlayer1");
        playerEvents[1].Name.Should().Be("ValidPlayer2");
    }

    [Fact]
    public async Task Pipeline_PublisherFailure_RetriesAndRecovers()
    {
        // Arrange
        var packet = _packetBuilder
            .WithPlayerSpottedData("RetryPlayer", 5, 100f, 100f, 0f)
            .Build();

        // Simula falha temporária no publisher
        _inMemoryPublisher.SimulateFailure(new IOException("Temporary failure"));

        // Act
        _fakeCapture.EnqueuePacket(packet);
        await _fakeCapture.StartCaptureAsync();

        // Aguarda primeira tentativa falhar
        await Task.Delay(200);

        // Remove a falha
        _inMemoryPublisher.ResetFailure();

        // Aguarda retry
        await Task.Delay(500);

        // Assert - Evento deve ter sido publicado após retry
        var publishedEvent = _inMemoryPublisher.GetLastPublishedMessage<PlayerSpottedV1>("albion.events.player.spotted.v1");
        publishedEvent.Should().NotBeNull();
        publishedEvent!.Name.Should().Be("RetryPlayer");
    }

    [Fact]
    public async Task Pipeline_HighVolume_ProcessesAllEvents()
    {
        // Arrange - Cria muitos pacotes
        var packetCount = 100;
        var packets = new List<byte[]>();
        
        for (int i = 0; i < packetCount; i++)
        {
            var packet = _packetBuilder
                .WithPlayerSpottedData($"Player{i}", tier: i % 8 + 1, x: i * 10f, y: i * 20f, z: 0f)
                .Build();
            packets.Add(packet);
        }

        // Act
        foreach (var packet in packets)
        {
            _fakeCapture.EnqueuePacket(packet);
        }
        await _fakeCapture.StartCaptureAsync();

        // Aguarda processamento
        await Task.Delay(3000);

        // Assert
        var publishedEvents = _inMemoryPublisher.GetAllPublishedMessages<PlayerSpottedV1>();
        publishedEvents.Should().HaveCount(packetCount);

        // Verifica que todos os eventos foram processados corretamente
        for (int i = 0; i < packetCount; i++)
        {
            var evt = publishedEvents.FirstOrDefault(e => e.Name == $"Player{i}");
            evt.Should().NotBeNull($"Player{i} should exist");
            evt!.Tier.Should().Be(i % 8 + 1);
            evt.X.Should().Be(i * 10f);
            evt.Y.Should().Be(i * 20f);
        }
    }
}

// Componentes simplificados para o pipeline E2E

public class EventEnricher
{
    private readonly IClock _clock;
    private readonly IIdGenerator _idGenerator;

    public EventEnricher(IClock clock, IIdGenerator idGenerator)
    {
        _clock = clock;
        _idGenerator = idGenerator;
    }

    public object Enrich(object domainEvent)
    {
        // Enriquece o evento com metadados
        return domainEvent switch
        {
            PlayerSpottedEvent e => new PlayerSpottedV1(
                _idGenerator.NewGuid(), _clock.UtcNow, e.Name, 
                "Guild", "Alliance", e.Tier, e.X, e.Y, e.Z, 100.0),
            
            MobSpawnedEvent e => new MobSpawnedV1(
                _idGenerator.NewGuid(), _clock.UtcNow, e.MobType,
                e.Tier, e.X, e.Y, e.Z, e.Health, e.Health),
            
            HarvestableEvent e => new HarvestableFoundV1(
                _idGenerator.NewGuid(), _clock.UtcNow, e.ResourceType,
                e.Tier, e.X, e.Y, e.Z, e.Charges),
            
            _ => domainEvent
        };
    }
}

public class SnifferPipeline
{
    private readonly IPacketCaptureService _capture;
    private readonly PhotonParser _parser;
    private readonly EventEnricher _enricher;
    private readonly IEventPublisher _publisher;
    private readonly ILogger<SnifferPipeline> _logger;

    public SnifferPipeline(
        IPacketCaptureService capture,
        PhotonParser parser,
        EventEnricher enricher,
        IEventPublisher publisher,
        ILogger<SnifferPipeline> logger)
    {
        _capture = capture;
        _parser = parser;
        _enricher = enricher;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting sniffer pipeline");

        await foreach (var domainEvent in _parser.ParseAsync(_capture.ReadPacketsAsync(cancellationToken)))
        {
            try
            {
                // Enriquece o evento
                var enrichedEvent = _enricher.Enrich(domainEvent);

                // Determina o tópico baseado no tipo do evento
                var topic = GetTopicForEvent(enrichedEvent);

                // Publica o evento
                await _publisher.PublishAsync(topic, enrichedEvent, cancellationToken);

                _logger.LogDebug("Published event {EventType} to {Topic}", 
                    enrichedEvent.GetType().Name, topic);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event {EventType}", 
                    domainEvent.GetType().Name);
                
                // Continua processando outros eventos
            }
        }
    }

    private string GetTopicForEvent(object evt)
    {
        return evt switch
        {
            PlayerSpottedV1 => "albion.events.player.spotted.v1",
            MobSpawnedV1 => "albion.events.mob.spawned.v1",
            HarvestableFoundV1 => "albion.events.harvestable.found.v1",
            ClusterChangedV1 => "albion.events.cluster.changed.v1",
            _ => "albion.events.unknown"
        };
    }
}

public class SnifferHostedService : BackgroundService
{
    private readonly SnifferPipeline _pipeline;
    private readonly ILogger<SnifferHostedService> _logger;

    public SnifferHostedService(SnifferPipeline pipeline, ILogger<SnifferHostedService> logger)
    {
        _pipeline = pipeline;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Sniffer hosted service starting");

        try
        {
            await _pipeline.RunAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Sniffer hosted service stopping");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sniffer hosted service crashed");
            throw;
        }
    }
}