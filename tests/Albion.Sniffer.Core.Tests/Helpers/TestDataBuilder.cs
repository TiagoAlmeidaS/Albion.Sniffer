using System;
using System.Collections.Generic;
using System.Text;
using AutoFixture;

namespace Albion.Sniffer.Core.Tests.Helpers;

public static class TestDataBuilder
{
    private static readonly Fixture _fixture = new();

    public static byte[] BuildLengthPrefixedString(string value)
    {
        if (string.IsNullOrEmpty(value))
            return new byte[] { 0, 0 };

        var bytes = Encoding.UTF8.GetBytes(value);
        var length = (ushort)bytes.Length;
        var result = new byte[2 + bytes.Length];
        
        // Write length as little-endian ushort
        result[0] = (byte)(length & 0xFF);
        result[1] = (byte)((length >> 8) & 0xFF);
        
        // Write string bytes
        Array.Copy(bytes, 0, result, 2, bytes.Length);
        
        return result;
    }

    public static byte[] BuildPacket(ushort opcode, byte[] payload)
    {
        var result = new byte[2 + payload.Length];
        
        // Write opcode as little-endian
        result[0] = (byte)(opcode & 0xFF);
        result[1] = (byte)((opcode >> 8) & 0xFF);
        
        // Write payload
        Array.Copy(payload, 0, result, 2, payload.Length);
        
        return result;
    }

    public static byte[] BuildNewCharacterPacket(string name = "TestPlayer", uint id = 12345)
    {
        var nameBytes = BuildLengthPrefixedString(name);
        var packet = new List<byte>();
        
        // Opcode for NEW_CHARACTER (example: 0x0001)
        packet.AddRange(BitConverter.GetBytes((ushort)0x0001));
        
        // Player ID
        packet.AddRange(BitConverter.GetBytes(id));
        
        // Player Name
        packet.AddRange(nameBytes);
        
        // Additional fields (simplified)
        packet.AddRange(BitConverter.GetBytes((int)100)); // Health
        packet.AddRange(BitConverter.GetBytes((int)50));  // Mana
        
        return packet.ToArray();
    }

    public static byte[] BuildMovePacket(float x, float y, float z, uint timestamp = 0)
    {
        var packet = new List<byte>();
        
        // Opcode for MOVE (example: 0x0002)
        packet.AddRange(BitConverter.GetBytes((ushort)0x0002));
        
        // Timestamp
        packet.AddRange(BitConverter.GetBytes(timestamp));
        
        // Position
        packet.AddRange(BitConverter.GetBytes(x));
        packet.AddRange(BitConverter.GetBytes(y));
        packet.AddRange(BitConverter.GetBytes(z));
        
        return packet.ToArray();
    }

    public static byte[] BuildChatPacket(string message, string sender = "TestSender", byte channel = 0)
    {
        var packet = new List<byte>();
        
        // Opcode for CHAT (example: 0x0003)
        packet.AddRange(BitConverter.GetBytes((ushort)0x0003));
        
        // Channel
        packet.Add(channel);
        
        // Sender
        packet.AddRange(BuildLengthPrefixedString(sender));
        
        // Message
        packet.AddRange(BuildLengthPrefixedString(message));
        
        return packet.ToArray();
    }

    public static byte[] BuildTruncatedPacket()
    {
        // Start building a packet but truncate it
        var packet = new List<byte>();
        
        // Opcode
        packet.AddRange(BitConverter.GetBytes((ushort)0x0004));
        
        // Start of a string length but no content
        packet.AddRange(BitConverter.GetBytes((ushort)100)); // Claims 100 bytes but has none
        
        return packet.ToArray();
    }

    public static byte[] BuildUnknownOpcodePacket()
    {
        var packet = new List<byte>();
        
        // Unknown opcode
        packet.AddRange(BitConverter.GetBytes((ushort)0xFFFF));
        
        // Some random payload
        packet.AddRange(_fixture.Create<byte[]>());
        
        return packet.ToArray();
    }

    public static T CreateValid<T>() where T : class
    {
        return _fixture.Create<T>();
    }

    public static byte[] GenerateRandomBytes(int length)
    {
        var bytes = new byte[length];
        new System.Random().NextBytes(bytes);
        return bytes;
    }

    public static GameEvent BuildGameEvent(
        string eventType = "TestEvent",
        byte[] payload = null,
        DateTime? timestamp = null,
        string sessionId = null)
    {
        return new GameEvent
        {
            EventType = eventType,
            Payload = payload ?? GenerateRandomBytes(10),
            Timestamp = timestamp ?? DateTime.UtcNow,
            SessionId = sessionId ?? Guid.NewGuid().ToString(),
            Size = payload?.Length ?? 10,
            Direction = "Inbound"
        };
    }

    public static PacketMetadata BuildPacketMetadata(
        string routingKey = "test.route",
        byte[] payload = null,
        DateTime? capturedAt = null)
    {
        return new PacketMetadata
        {
            RoutingKey = routingKey,
            Payload = payload ?? GenerateRandomBytes(20),
            CapturedAt = capturedAt ?? DateTime.UtcNow,
            SourcePort = 5056,
            DestinationPort = 5056,
            Protocol = "UDP"
        };
    }
}

// Simplified models for testing
public class GameEvent
{
    public string EventType { get; set; }
    public byte[] Payload { get; set; }
    public DateTime Timestamp { get; set; }
    public string SessionId { get; set; }
    public int Size { get; set; }
    public string Direction { get; set; }
    public string CorrelationId { get; set; }
    public ParseStatus ParseStatus { get; set; }
}

public class PacketMetadata
{
    public string RoutingKey { get; set; }
    public byte[] Payload { get; set; }
    public DateTime CapturedAt { get; set; }
    public int SourcePort { get; set; }
    public int DestinationPort { get; set; }
    public string Protocol { get; set; }
}

public enum ParseStatus
{
    Ok,
    Truncated,
    Invalid,
    Unknown
}