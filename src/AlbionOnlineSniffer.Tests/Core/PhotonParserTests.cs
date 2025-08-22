using AlbionOnlineSniffer.Tests.Common.Builders;
using AlbionOnlineSniffer.Tests.Common.Fakes;
using FluentAssertions;
using Xunit;

namespace AlbionOnlineSniffer.Tests.Core;

public class PhotonParserTests
{
    private readonly PhotonPacketBuilder _packetBuilder;
    private readonly FakePacketCaptureService _fakeCapture;

    public PhotonParserTests()
    {
        _packetBuilder = new PhotonPacketBuilder();
        _fakeCapture = new FakePacketCaptureService();
    }

    [Fact]
    public async Task ParseAsync_ValidPlayerSpottedPacket_ReturnsPlayerSpottedEvent()
    {
        // Arrange
        var packet = _packetBuilder
            .WithPlayerSpottedData("TestPlayer", 5, 100f, 200f, 0f)
            .Build();
        
        _fakeCapture.EnqueuePacket(packet);
        await _fakeCapture.StartCaptureAsync();

        var parser = new PhotonParser();

        // Act
        var events = new List<object>();
        await foreach (var evt in parser.ParseAsync(_fakeCapture.ReadPacketsAsync()))
        {
            events.Add(evt);
            if (events.Count >= 1) break; // Para após processar um evento
        }

        // Assert
        events.Should().HaveCount(1);
        events[0].Should().BeOfType<PlayerSpottedEvent>();
        
        var playerEvent = events[0] as PlayerSpottedEvent;
        playerEvent!.Name.Should().Be("TestPlayer");
        playerEvent.Tier.Should().Be(5);
        playerEvent.X.Should().Be(100f);
        playerEvent.Y.Should().Be(200f);
        playerEvent.Z.Should().Be(0f);

        await _fakeCapture.StopCaptureAsync();
    }

    [Fact]
    public async Task ParseAsync_ValidMobSpawnedPacket_ReturnsMobSpawnedEvent()
    {
        // Arrange
        var packet = _packetBuilder
            .WithMobSpawnedData("Wolf", 3, 150f, 250f, 0f, 1000)
            .Build();
        
        _fakeCapture.EnqueuePacket(packet);
        await _fakeCapture.StartCaptureAsync();

        var parser = new PhotonParser();

        // Act
        var events = new List<object>();
        await foreach (var evt in parser.ParseAsync(_fakeCapture.ReadPacketsAsync()))
        {
            events.Add(evt);
            if (events.Count >= 1) break;
        }

        // Assert
        events.Should().HaveCount(1);
        events[0].Should().BeOfType<MobSpawnedEvent>();
        
        var mobEvent = events[0] as MobSpawnedEvent;
        mobEvent!.MobType.Should().Be("Wolf");
        mobEvent.Tier.Should().Be(3);
        mobEvent.X.Should().Be(150f);
        mobEvent.Y.Should().Be(250f);
        mobEvent.Health.Should().Be(1000);

        await _fakeCapture.StopCaptureAsync();
    }

    [Fact]
    public async Task ParseAsync_InvalidPacket_IgnoresAndContinues()
    {
        // Arrange
        var invalidPacket = PhotonPacketBuilder.CreateInvalidPacket();
        var validPacket = _packetBuilder
            .WithPlayerSpottedData("ValidPlayer", 4, 50f, 60f, 0f)
            .Build();
        
        _fakeCapture.EnqueuePackets(invalidPacket, validPacket);
        await _fakeCapture.StartCaptureAsync();

        var parser = new PhotonParser();

        // Act
        var events = new List<object>();
        await foreach (var evt in parser.ParseAsync(_fakeCapture.ReadPacketsAsync()))
        {
            events.Add(evt);
            if (events.Count >= 1) break;
        }

        // Assert
        events.Should().HaveCount(1);
        events[0].Should().BeOfType<PlayerSpottedEvent>();
        
        var playerEvent = events[0] as PlayerSpottedEvent;
        playerEvent!.Name.Should().Be("ValidPlayer");

        await _fakeCapture.StopCaptureAsync();
    }

    [Fact]
    public async Task ParseAsync_EmptyPacket_ReturnsNoEvents()
    {
        // Arrange
        var emptyPacket = PhotonPacketBuilder.CreateEmptyPacket();
        _fakeCapture.EnqueuePacket(emptyPacket);
        await _fakeCapture.StartCaptureAsync();

        var parser = new PhotonParser();

        // Act
        var events = new List<object>();
        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
        
        try
        {
            await foreach (var evt in parser.ParseAsync(_fakeCapture.ReadPacketsAsync(cts.Token)))
            {
                events.Add(evt);
            }
        }
        catch (OperationCanceledException)
        {
            // Esperado quando timeout
        }

        // Assert
        events.Should().BeEmpty();

        await _fakeCapture.StopCaptureAsync();
    }

    [Fact]
    public async Task ParseAsync_MultiplePackets_ReturnsAllEvents()
    {
        // Arrange
        var packet1 = new PhotonPacketBuilder()
            .WithPlayerSpottedData("Player1", 5, 100f, 100f, 0f)
            .Build();
        
        var packet2 = new PhotonPacketBuilder()
            .WithMobSpawnedData("Bear", 4, 200f, 200f, 0f, 2000)
            .Build();
        
        var packet3 = new PhotonPacketBuilder()
            .WithHarvestableData("Stone", 3, 300f, 300f, 0f, 7)
            .Build();
        
        _fakeCapture.EnqueuePackets(packet1, packet2, packet3);
        await _fakeCapture.StartCaptureAsync();

        var parser = new PhotonParser();

        // Act
        var events = new List<object>();
        await foreach (var evt in parser.ParseAsync(_fakeCapture.ReadPacketsAsync()))
        {
            events.Add(evt);
            if (events.Count >= 3) break;
        }

        // Assert
        events.Should().HaveCount(3);
        events[0].Should().BeOfType<PlayerSpottedEvent>();
        events[1].Should().BeOfType<MobSpawnedEvent>();
        events[2].Should().BeOfType<HarvestableEvent>();

        await _fakeCapture.StopCaptureAsync();
    }
}

// Tipos temporários para os eventos do parser (serão substituídos pelos reais)
public record PlayerSpottedEvent(string Name, int Tier, float X, float Y, float Z);
public record MobSpawnedEvent(string MobType, int Tier, float X, float Y, float Z, int Health);
public record HarvestableEvent(string ResourceType, int Tier, float X, float Y, float Z, int Charges);

// Implementação básica do PhotonParser para os testes
public class PhotonParser
{
    public async IAsyncEnumerable<object> ParseAsync(IAsyncEnumerable<byte[]> packets)
    {
        await foreach (var packet in packets)
        {
            if (packet.Length < 3) continue;
            
            // Verifica magic bytes
            if (packet[0] != 0xF3 || packet[1] != 0x01) continue;
            
            // Parse simples baseado no event code
            if (packet.Length > 3 && packet[2] == 0x02) // Event type
            {
                var eventCode = packet[3];
                
                var evt = eventCode switch
                {
                    0x10 => ParsePlayerSpotted(packet),
                    0x11 => ParseMobSpawned(packet),
                    0x12 => ParseHarvestable(packet),
                    _ => null
                };
                
                if (evt != null)
                    yield return evt;
            }
        }
    }
    
    private object? ParsePlayerSpotted(byte[] packet)
    {
        try
        {
            var offset = 4;
            var nameLength = packet[offset++];
            var name = System.Text.Encoding.UTF8.GetString(packet, offset, nameLength);
            offset += nameLength;
            
            var tier = packet[offset++];
            var x = BitConverter.ToSingle(packet, offset); offset += 4;
            var y = BitConverter.ToSingle(packet, offset); offset += 4;
            var z = BitConverter.ToSingle(packet, offset);
            
            return new PlayerSpottedEvent(name, tier, x, y, z);
        }
        catch
        {
            return null;
        }
    }
    
    private object? ParseMobSpawned(byte[] packet)
    {
        try
        {
            var offset = 4;
            var typeLength = packet[offset++];
            var mobType = System.Text.Encoding.UTF8.GetString(packet, offset, typeLength);
            offset += typeLength;
            
            var tier = packet[offset++];
            var x = BitConverter.ToSingle(packet, offset); offset += 4;
            var y = BitConverter.ToSingle(packet, offset); offset += 4;
            var z = BitConverter.ToSingle(packet, offset); offset += 4;
            var health = BitConverter.ToInt32(packet, offset);
            
            return new MobSpawnedEvent(mobType, tier, x, y, z, health);
        }
        catch
        {
            return null;
        }
    }
    
    private object? ParseHarvestable(byte[] packet)
    {
        try
        {
            var offset = 4;
            var typeLength = packet[offset++];
            var resourceType = System.Text.Encoding.UTF8.GetString(packet, offset, typeLength);
            offset += typeLength;
            
            var tier = packet[offset++];
            var x = BitConverter.ToSingle(packet, offset); offset += 4;
            var y = BitConverter.ToSingle(packet, offset); offset += 4;
            var z = BitConverter.ToSingle(packet, offset); offset += 4;
            var charges = BitConverter.ToInt32(packet, offset);
            
            return new HarvestableEvent(resourceType, tier, x, y, z, charges);
        }
        catch
        {
            return null;
        }
    }
}