using System;
using System.IO;
using System.Text;
using FluentAssertions;
using Xunit;
using FsCheck;
using FsCheck.Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Albion.Sniffer.Core.Tests.Helpers;
using Albion.Sniffer.Core.Tests.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Albion.Sniffer.Core.Tests.Protocol16;

public class Protocol16DeserializerTests
{
    private readonly Mock<ILogger<Protocol16Deserializer>> _loggerMock;
    private readonly Protocol16Deserializer _deserializer;

    public Protocol16DeserializerTests()
    {
        _loggerMock = new Mock<ILogger<Protocol16Deserializer>>();
        _deserializer = new Protocol16Deserializer(_loggerMock.Object);
    }

    [Fact]
    public void Parse_NewCharacter_Returns_Expected_Event()
    {
        // Arrange
        var raw = TestDataBuilder.BuildNewCharacterPacket("Deatheye", 42);
        
        // Act
        var result = _deserializer.TryParse(raw, out var gameEvent);

        // Assert
        result.Should().BeTrue();
        gameEvent.Should().NotBeNull();
        gameEvent.EventType.Should().Be("NewCharacter");
        gameEvent.ParseStatus.Should().Be(ParseStatus.Ok);
        
        // Verify specific fields were parsed correctly
        var reader = new ByteReader(gameEvent.Payload);
        reader.ReadUInt16(); // Skip opcode
        var playerId = reader.ReadUInt32();
        playerId.Should().Be(42);
        
        var playerName = reader.ReadString();
        playerName.Should().Be("Deatheye");
    }

    [Fact]
    public void Parse_MovePosition_Returns_Expected_Event()
    {
        // Arrange
        var raw = TestDataBuilder.BuildMovePacket(100.5f, 200.3f, 50.0f, 12345);
        
        // Act
        var result = _deserializer.TryParse(raw, out var gameEvent);

        // Assert
        result.Should().BeTrue();
        gameEvent.Should().NotBeNull();
        gameEvent.EventType.Should().Be("Move");
        gameEvent.Size.Should().Be(raw.Length);
        gameEvent.ParseStatus.Should().Be(ParseStatus.Ok);
    }

    [Fact]
    public void Parse_Chat_WithUTF8_Returns_Expected_Event()
    {
        // Arrange
        var message = "Hello Albion! 你好 мир ✓";
        var raw = TestDataBuilder.BuildChatPacket(message, "TestPlayer", 1);
        
        // Act
        var result = _deserializer.TryParse(raw, out var gameEvent);

        // Assert
        result.Should().BeTrue();
        gameEvent.Should().NotBeNull();
        gameEvent.EventType.Should().Be("Chat");
        
        // Verify message was preserved correctly
        var reader = new ByteReader(gameEvent.Payload);
        reader.ReadUInt16(); // Skip opcode
        reader.ReadByte(); // Skip channel
        reader.ReadString(); // Skip sender
        var parsedMessage = reader.ReadString();
        parsedMessage.Should().Be(message);
    }

    [Fact]
    public void Parse_UnknownOpcode_Returns_UnknownEvent()
    {
        // Arrange
        var raw = TestDataBuilder.BuildUnknownOpcodePacket();
        
        // Act
        var result = _deserializer.TryParse(raw, out var gameEvent);

        // Assert
        result.Should().BeTrue();
        gameEvent.Should().NotBeNull();
        gameEvent.EventType.Should().Be("Unknown");
        gameEvent.ParseStatus.Should().Be(ParseStatus.Unknown);
        gameEvent.Payload.Should().BeEquivalentTo(raw); // Raw payload preserved
    }

    [Fact]
    public void Parse_TruncatedBuffer_Returns_TruncatedStatus()
    {
        // Arrange
        var raw = TestDataBuilder.BuildTruncatedPacket();
        
        // Act
        var result = _deserializer.TryParse(raw, out var gameEvent);

        // Assert
        result.Should().BeFalse();
        gameEvent.Should().NotBeNull();
        gameEvent.ParseStatus.Should().Be(ParseStatus.Truncated);
    }

    [Fact]
    public void Parse_EmptyBuffer_Returns_Invalid()
    {
        // Arrange
        var raw = Array.Empty<byte>();
        
        // Act
        var result = _deserializer.TryParse(raw, out var gameEvent);

        // Assert
        result.Should().BeFalse();
        gameEvent.Should().NotBeNull();
        gameEvent.ParseStatus.Should().Be(ParseStatus.Invalid);
    }

    [Fact]
    public void Parse_NullBuffer_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => _deserializer.TryParse(null!, out _);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Parse_BufferTooSmallForOpcode_Returns_Invalid()
    {
        // Arrange
        var raw = new byte[] { 0x01 }; // Only 1 byte, need at least 2 for opcode
        
        // Act
        var result = _deserializer.TryParse(raw, out var gameEvent);

        // Assert
        result.Should().BeFalse();
        gameEvent.ParseStatus.Should().Be(ParseStatus.Invalid);
    }

    [Theory]
    [InlineData(0x0001, "NewCharacter")]
    [InlineData(0x0002, "Move")]
    [InlineData(0x0003, "Chat")]
    [InlineData(0x0004, "ChangeCluster")]
    [InlineData(0x0005, "NewMob")]
    public void Parse_KnownOpcodes_MapsToCorrectEventType(ushort opcode, string expectedEventType)
    {
        // Arrange
        var raw = TestDataBuilder.BuildPacket(opcode, new byte[] { 1, 2, 3 });
        
        // Act
        var result = _deserializer.TryParse(raw, out var gameEvent);

        // Assert
        result.Should().BeTrue();
        gameEvent.EventType.Should().Be(expectedEventType);
    }

    [Property(MaxTest = 100)]
    public void Parse_RandomPayload_NeverThrowsUnexpectedly(byte[] payload)
    {
        // Skip null payloads
        if (payload == null) return;

        // Act
        Action act = () => _deserializer.TryParse(payload, out _);

        // Assert - Should handle gracefully without unexpected exceptions
        act.Should().NotThrow<Exception>();
    }

    [Property(MaxTest = 50)]
    public void Parse_ValidStructure_AlwaysPreservesPayloadSize(PositiveInt size)
    {
        // Arrange
        var payloadSize = Math.Min(size.Get, 1000); // Limit size for test performance
        var payload = TestDataBuilder.GenerateRandomBytes(payloadSize);
        var packet = TestDataBuilder.BuildPacket(0x0001, payload);

        // Act
        _deserializer.TryParse(packet, out var gameEvent);

        // Assert
        if (gameEvent != null)
        {
            gameEvent.Size.Should().Be(packet.Length);
        }
    }

    [Fact]
    public void Parse_LengthPrefixedField_ExceedsBuffer_Returns_Truncated()
    {
        // Arrange
        var packet = new List<byte>();
        packet.AddRange(BitConverter.GetBytes((ushort)0x0001)); // Opcode
        packet.AddRange(BitConverter.GetBytes((ushort)1000)); // Claims 1000 byte string
        packet.AddRange(new byte[] { 1, 2, 3 }); // But only has 3 bytes

        // Act
        var result = _deserializer.TryParse(packet.ToArray(), out var gameEvent);

        // Assert
        result.Should().BeFalse();
        gameEvent.ParseStatus.Should().Be(ParseStatus.Truncated);
    }

    [Fact]
    public void Parse_MultipleConsecutivePackets_ParsesIndependently()
    {
        // Arrange
        var packet1 = TestDataBuilder.BuildNewCharacterPacket("Player1", 1);
        var packet2 = TestDataBuilder.BuildNewCharacterPacket("Player2", 2);

        // Act
        var result1 = _deserializer.TryParse(packet1, out var event1);
        var result2 = _deserializer.TryParse(packet2, out var event2);

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();
        event1.Should().NotBeSameAs(event2);
    }
}

// Simplified Protocol16Deserializer for testing
public class Protocol16Deserializer
{
    private readonly ILogger<Protocol16Deserializer> _logger;
    private readonly Dictionary<ushort, string> _opcodeMap = new()
    {
        { 0x0001, "NewCharacter" },
        { 0x0002, "Move" },
        { 0x0003, "Chat" },
        { 0x0004, "ChangeCluster" },
        { 0x0005, "NewMob" }
    };

    public Protocol16Deserializer(ILogger<Protocol16Deserializer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool TryParse(byte[] buffer, out GameEvent gameEvent)
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));

        gameEvent = new GameEvent
        {
            Payload = buffer,
            Size = buffer.Length,
            Timestamp = DateTime.UtcNow,
            SessionId = Guid.NewGuid().ToString()
        };

        if (buffer.Length < 2)
        {
            gameEvent.ParseStatus = ParseStatus.Invalid;
            return false;
        }

        try
        {
            var reader = new ByteReader(buffer);
            var opcode = reader.ReadUInt16();

            if (_opcodeMap.TryGetValue(opcode, out var eventType))
            {
                gameEvent.EventType = eventType;
                gameEvent.ParseStatus = ParseStatus.Ok;
                
                // Validate remaining buffer has expected data
                if (!ValidatePacketStructure(reader, eventType))
                {
                    gameEvent.ParseStatus = ParseStatus.Truncated;
                    return false;
                }
                
                return true;
            }
            else
            {
                gameEvent.EventType = "Unknown";
                gameEvent.ParseStatus = ParseStatus.Unknown;
                return true;
            }
        }
        catch (EndOfStreamException)
        {
            gameEvent.ParseStatus = ParseStatus.Truncated;
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing packet");
            gameEvent.ParseStatus = ParseStatus.Invalid;
            return false;
        }
    }

    private bool ValidatePacketStructure(ByteReader reader, string eventType)
    {
        try
        {
            switch (eventType)
            {
                case "NewCharacter":
                    if (!reader.CanRead(4)) return false; // Need at least ID
                    reader.ReadUInt32(); // ID
                    if (!reader.CanRead(2)) return false; // String length
                    var nameLen = reader.ReadUInt16();
                    if (!reader.CanRead(nameLen)) return false;
                    break;

                case "Move":
                    if (!reader.CanRead(16)) return false; // timestamp + 3 floats
                    break;

                case "Chat":
                    if (!reader.CanRead(1)) return false; // Channel
                    reader.ReadByte();
                    // Check sender string
                    if (!reader.CanRead(2)) return false;
                    var senderLen = reader.ReadUInt16();
                    if (!reader.CanRead(senderLen)) return false;
                    reader.ReadBytes(senderLen);
                    // Check message string
                    if (!reader.CanRead(2)) return false;
                    var msgLen = reader.ReadUInt16();
                    if (!reader.CanRead(msgLen)) return false;
                    break;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}