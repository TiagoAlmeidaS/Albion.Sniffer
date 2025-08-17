using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Xunit;
using FsCheck;
using FsCheck.Xunit;
using Albion.Sniffer.Core.Tests.Helpers;

namespace Albion.Sniffer.Core.Tests.Normalization;

public class EventNormalizerTests
{
    private readonly EventNormalizer _normalizer;

    public EventNormalizerTests()
    {
        _normalizer = new EventNormalizer();
    }

    [Fact]
    public void Normalize_SetsCorrectDirection_BasedOnPort()
    {
        // Arrange
        var inboundPacket = TestDataBuilder.BuildPacketMetadata();
        inboundPacket.SourcePort = 5056; // Game server port
        inboundPacket.DestinationPort = 45678;

        var outboundPacket = TestDataBuilder.BuildPacketMetadata();
        outboundPacket.SourcePort = 45678;
        outboundPacket.DestinationPort = 5056; // Game server port

        // Act
        var inboundEvent = _normalizer.Normalize(inboundPacket);
        var outboundEvent = _normalizer.Normalize(outboundPacket);

        // Assert
        inboundEvent.Direction.Should().Be("Inbound");
        outboundEvent.Direction.Should().Be("Outbound");
    }

    [Fact]
    public void Normalize_CorrelationId_IsStable_ForSameInput()
    {
        // Arrange
        var packet1 = TestDataBuilder.BuildPacketMetadata("route.key", new byte[] { 1, 2, 3 });
        var packet2 = TestDataBuilder.BuildPacketMetadata("route.key", new byte[] { 1, 2, 3 });

        // Act
        var event1 = _normalizer.Normalize(packet1);
        var event2 = _normalizer.Normalize(packet2);

        // Assert
        event1.CorrelationId.Should().Be(event2.CorrelationId);
    }

    [Fact]
    public void Normalize_CorrelationId_IsDifferent_ForDifferentInput()
    {
        // Arrange
        var packet1 = TestDataBuilder.BuildPacketMetadata("route.key1", new byte[] { 1, 2, 3 });
        var packet2 = TestDataBuilder.BuildPacketMetadata("route.key2", new byte[] { 4, 5, 6 });

        // Act
        var event1 = _normalizer.Normalize(packet1);
        var event2 = _normalizer.Normalize(packet2);

        // Assert
        event1.CorrelationId.Should().NotBe(event2.CorrelationId);
    }

    [Fact]
    public void Normalize_Size_EqualsPayloadLength()
    {
        // Arrange
        var payload = TestDataBuilder.GenerateRandomBytes(100);
        var packet = TestDataBuilder.BuildPacketMetadata(payload: payload);

        // Act
        var gameEvent = _normalizer.Normalize(packet);

        // Assert
        gameEvent.Size.Should().Be(payload.Length);
    }

    [Fact]
    public void Normalize_Timestamp_IsMonotonic_WithinSession()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
        var packets = Enumerable.Range(0, 100)
            .Select(i => 
            {
                var p = TestDataBuilder.BuildPacketMetadata();
                p.CapturedAt = DateTime.UtcNow.AddMilliseconds(i * 10);
                return p;
            })
            .ToList();

        // Act
        var events = packets.Select(p => _normalizer.Normalize(p, sessionId)).ToList();

        // Assert
        for (int i = 1; i < events.Count; i++)
        {
            events[i].Timestamp.Should().BeOnOrAfter(events[i - 1].Timestamp);
        }
    }

    [Fact]
    public void Normalize_PreservesOriginalPayload()
    {
        // Arrange
        var payload = TestDataBuilder.GenerateRandomBytes(50);
        var packet = TestDataBuilder.BuildPacketMetadata(payload: payload);

        // Act
        var gameEvent = _normalizer.Normalize(packet);

        // Assert
        gameEvent.Payload.Should().BeEquivalentTo(payload);
    }

    [Fact]
    public void Normalize_HandlesNullPayload_Gracefully()
    {
        // Arrange
        var packet = TestDataBuilder.BuildPacketMetadata(payload: null);

        // Act
        var gameEvent = _normalizer.Normalize(packet);

        // Assert
        gameEvent.Payload.Should().BeEmpty();
        gameEvent.Size.Should().Be(0);
    }

    [Fact]
    public void Normalize_HandlesEmptyPayload_Correctly()
    {
        // Arrange
        var packet = TestDataBuilder.BuildPacketMetadata(payload: Array.Empty<byte>());

        // Act
        var gameEvent = _normalizer.Normalize(packet);

        // Assert
        gameEvent.Payload.Should().BeEmpty();
        gameEvent.Size.Should().Be(0);
    }

    [Fact]
    public void Normalize_SessionId_RemainsConsistent()
    {
        // Arrange
        var sessionId = "test-session-123";
        var packet = TestDataBuilder.BuildPacketMetadata();

        // Act
        var gameEvent = _normalizer.Normalize(packet, sessionId);

        // Assert
        gameEvent.SessionId.Should().Be(sessionId);
    }

    [Fact]
    public void Normalize_GeneratesSessionId_WhenNotProvided()
    {
        // Arrange
        var packet = TestDataBuilder.BuildPacketMetadata();

        // Act
        var gameEvent = _normalizer.Normalize(packet);

        // Assert
        gameEvent.SessionId.Should().NotBeNullOrWhiteSpace();
        Guid.TryParse(gameEvent.SessionId, out _).Should().BeTrue();
    }

    [Theory]
    [InlineData("game.move", "Move")]
    [InlineData("game.chat", "Chat")]
    [InlineData("game.newcharacter", "NewCharacter")]
    [InlineData("unknown.route", "Unknown")]
    public void Normalize_MapsRoutingKey_ToEventType(string routingKey, string expectedEventType)
    {
        // Arrange
        var packet = TestDataBuilder.BuildPacketMetadata(routingKey);

        // Act
        var gameEvent = _normalizer.Normalize(packet);

        // Assert
        gameEvent.EventType.Should().Be(expectedEventType);
    }

    [Property(MaxTest = 100)]
    public void Normalize_AlwaysProduces_ValidCorrelationId(byte[] payload, string routingKey)
    {
        // Arrange
        if (string.IsNullOrEmpty(routingKey)) routingKey = "test.route";
        var packet = TestDataBuilder.BuildPacketMetadata(routingKey, payload);

        // Act
        var gameEvent = _normalizer.Normalize(packet);

        // Assert
        gameEvent.CorrelationId.Should().NotBeNullOrWhiteSpace();
        gameEvent.CorrelationId.Should().MatchRegex("^[a-fA-F0-9]{8,}$"); // Hex string
    }

    [Property(MaxTest = 50)]
    public void Normalize_CorrelationId_HasLowCollisionProbability(NonEmptyString route1, NonEmptyString route2)
    {
        // Generate many packets with different payloads
        var correlationIds = new HashSet<string>();
        var collisions = 0;

        for (int i = 0; i < 1000; i++)
        {
            var payload = TestDataBuilder.GenerateRandomBytes(20);
            var routingKey = i % 2 == 0 ? route1.Get : route2.Get;
            var packet = TestDataBuilder.BuildPacketMetadata(routingKey, payload);
            
            var gameEvent = _normalizer.Normalize(packet);
            
            if (!correlationIds.Add(gameEvent.CorrelationId))
            {
                collisions++;
            }
        }

        // Assert collision rate is very low (< 0.1%)
        var collisionRate = collisions / 1000.0;
        collisionRate.Should().BeLessThan(0.001);
    }

    [Fact]
    public void Normalize_Idempotent_ForSameInput()
    {
        // Arrange
        var packet = TestDataBuilder.BuildPacketMetadata();

        // Act
        var event1 = _normalizer.Normalize(packet);
        var event2 = _normalizer.Normalize(packet);

        // Assert - All properties except timestamp should be identical
        event1.EventType.Should().Be(event2.EventType);
        event1.CorrelationId.Should().Be(event2.CorrelationId);
        event1.Size.Should().Be(event2.Size);
        event1.Direction.Should().Be(event2.Direction);
        event1.Payload.Should().BeEquivalentTo(event2.Payload);
    }

    [Fact]
    public void Normalize_HandlesLargePayloads_Efficiently()
    {
        // Arrange
        var largePayload = TestDataBuilder.GenerateRandomBytes(1024 * 1024); // 1MB
        var packet = TestDataBuilder.BuildPacketMetadata(payload: largePayload);

        // Act
        var startTime = DateTime.UtcNow;
        var gameEvent = _normalizer.Normalize(packet);
        var elapsed = DateTime.UtcNow - startTime;

        // Assert
        gameEvent.Size.Should().Be(largePayload.Length);
        elapsed.TotalMilliseconds.Should().BeLessThan(100); // Should be fast
    }
}

// Simplified EventNormalizer for testing
public class EventNormalizer
{
    private readonly Dictionary<string, DateTime> _sessionTimestamps = new();
    private readonly object _lock = new();

    public GameEvent Normalize(PacketMetadata packet, string sessionId = null)
    {
        var gameEvent = new GameEvent
        {
            Payload = packet.Payload ?? Array.Empty<byte>(),
            Size = packet.Payload?.Length ?? 0,
            SessionId = sessionId ?? Guid.NewGuid().ToString(),
            Timestamp = GetMonotonicTimestamp(packet.CapturedAt, sessionId),
            Direction = DetermineDirection(packet),
            EventType = MapRoutingKeyToEventType(packet.RoutingKey),
            CorrelationId = GenerateCorrelationId(packet)
        };

        return gameEvent;
    }

    private DateTime GetMonotonicTimestamp(DateTime capturedAt, string sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
            return capturedAt;

        lock (_lock)
        {
            if (!_sessionTimestamps.TryGetValue(sessionId, out var lastTimestamp))
            {
                _sessionTimestamps[sessionId] = capturedAt;
                return capturedAt;
            }

            // Ensure monotonic increase
            if (capturedAt <= lastTimestamp)
            {
                capturedAt = lastTimestamp.AddTicks(1);
            }

            _sessionTimestamps[sessionId] = capturedAt;
            return capturedAt;
        }
    }

    private string DetermineDirection(PacketMetadata packet)
    {
        const int gameServerPort = 5056;
        
        if (packet.SourcePort == gameServerPort)
            return "Inbound";
        else if (packet.DestinationPort == gameServerPort)
            return "Outbound";
        else
            return "Unknown";
    }

    private string MapRoutingKeyToEventType(string routingKey)
    {
        if (string.IsNullOrEmpty(routingKey))
            return "Unknown";

        var parts = routingKey.Split('.');
        if (parts.Length < 2)
            return "Unknown";

        return parts[1] switch
        {
            "move" => "Move",
            "chat" => "Chat",
            "newcharacter" => "NewCharacter",
            "changecluster" => "ChangeCluster",
            "newmob" => "NewMob",
            _ => "Unknown"
        };
    }

    private string GenerateCorrelationId(PacketMetadata packet)
    {
        using var sha256 = SHA256.Create();
        
        var input = $"{packet.RoutingKey}:{packet.Payload?.Length ?? 0}";
        if (packet.Payload != null && packet.Payload.Length > 0)
        {
            input += ":" + BitConverter.ToString(packet.Payload.Take(Math.Min(20, packet.Payload.Length)).ToArray());
        }
        
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToString(hash).Replace("-", "").Substring(0, 16);
    }
}