using System;
using System.Text;
using FluentAssertions;
using Xunit;
using FsCheck;
using FsCheck.Xunit;
using Albion.Sniffer.Core.Tests.Helpers;

namespace Albion.Sniffer.Core.Tests.Utils;

public class ByteReaderTests
{
    [Fact]
    public void Constructor_WithNullBuffer_ThrowsArgumentNullException()
    {
        // Arrange & Act
        Action act = () => new ByteReader(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithEmptyBuffer_CreatesValidReader()
    {
        // Arrange
        var data = Array.Empty<byte>();

        // Act
        var reader = new ByteReader(data);

        // Assert
        reader.Position.Should().Be(0);
        reader.Length.Should().Be(0);
        reader.CanRead(1).Should().BeFalse();
    }

    [Fact]
    public void ReadByte_AdvancesPosition()
    {
        // Arrange
        var data = new byte[] { 0x42 };
        var reader = new ByteReader(data);

        // Act
        var value = reader.ReadByte();

        // Assert
        value.Should().Be(0x42);
        reader.Position.Should().Be(1);
    }

    [Fact]
    public void ReadUInt16_LittleEndian_ReturnsCorrectValue()
    {
        // Arrange
        var data = new byte[] { 0x34, 0x12 }; // Little-endian 0x1234
        var reader = new ByteReader(data);

        // Act
        var value = reader.ReadUInt16();

        // Assert
        value.Should().Be(0x1234);
        reader.Position.Should().Be(2);
    }

    [Fact]
    public void ReadUInt32_LittleEndian_ReturnsCorrectValue()
    {
        // Arrange
        var data = new byte[] { 0x78, 0x56, 0x34, 0x12 }; // Little-endian 0x12345678
        var reader = new ByteReader(data);

        // Act
        var value = reader.ReadUInt32();

        // Assert
        value.Should().Be(0x12345678u);
        reader.Position.Should().Be(4);
    }

    [Fact]
    public void ReadString_LengthPrefixed_ReturnsCorrectString()
    {
        // Arrange
        var testString = "Albion✓";
        var bytes = TestDataBuilder.BuildLengthPrefixedString(testString);
        var reader = new ByteReader(bytes);

        // Act
        var result = reader.ReadString();

        // Assert
        result.Should().Be(testString);
        reader.Position.Should().Be(bytes.Length);
    }

    [Fact]
    public void ReadString_WithEmptyString_ReturnsEmpty()
    {
        // Arrange
        var bytes = TestDataBuilder.BuildLengthPrefixedString("");
        var reader = new ByteReader(bytes);

        // Act
        var result = reader.ReadString();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ReadBytes_ReturnsCorrectData()
    {
        // Arrange
        var data = new byte[] { 1, 2, 3, 4, 5 };
        var reader = new ByteReader(data);

        // Act
        var result = reader.ReadBytes(3);

        // Assert
        result.Should().BeEquivalentTo(new byte[] { 1, 2, 3 });
        reader.Position.Should().Be(3);
    }

    [Fact]
    public void ReadBeyondBuffer_ThrowsEndOfStreamException()
    {
        // Arrange
        var data = new byte[] { 1, 2 };
        var reader = new ByteReader(data);
        reader.ReadBytes(2); // Read all data

        // Act
        Action act = () => reader.ReadByte();

        // Assert
        act.Should().Throw<EndOfStreamException>();
    }

    [Fact]
    public void Seek_ToValidPosition_UpdatesPosition()
    {
        // Arrange
        var data = new byte[] { 1, 2, 3, 4, 5 };
        var reader = new ByteReader(data);

        // Act
        reader.Seek(3);

        // Assert
        reader.Position.Should().Be(3);
        reader.ReadByte().Should().Be(4);
    }

    [Fact]
    public void Seek_BeyondBuffer_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var data = new byte[] { 1, 2, 3 };
        var reader = new ByteReader(data);

        // Act
        Action act = () => reader.Seek(10);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void CanRead_WithAvailableData_ReturnsTrue()
    {
        // Arrange
        var data = new byte[] { 1, 2, 3 };
        var reader = new ByteReader(data);

        // Assert
        reader.CanRead(1).Should().BeTrue();
        reader.CanRead(3).Should().BeTrue();
        reader.CanRead(4).Should().BeFalse();
    }

    [Fact]
    public void Reset_ResetsPositionToZero()
    {
        // Arrange
        var data = new byte[] { 1, 2, 3 };
        var reader = new ByteReader(data);
        reader.ReadBytes(2);

        // Act
        reader.Reset();

        // Assert
        reader.Position.Should().Be(0);
    }

    [Property(MaxTest = 200)]
    public void ReadWrite_RoundTrip_PreservesData(byte[] data)
    {
        // Skip null or empty arrays
        if (data == null || data.Length == 0) return;

        // Arrange
        var reader = new ByteReader(data);

        // Act
        var result = reader.ReadBytes(data.Length);

        // Assert
        result.Should().BeEquivalentTo(data);
        reader.Position.Should().Be(data.Length);
    }

    [Property(MaxTest = 100)]
    public void Position_NeverExceedsLength(byte[] data, PositiveInt readCount)
    {
        // Skip null arrays
        if (data == null) return;

        // Arrange
        var reader = new ByteReader(data ?? Array.Empty<byte>());
        var bytesToRead = Math.Min(readCount.Get, data?.Length ?? 0);

        // Act
        if (bytesToRead > 0)
        {
            reader.ReadBytes(bytesToRead);
        }

        // Assert
        reader.Position.Should().BeLessOrEqualTo(reader.Length);
    }

    [Property(MaxTest = 100)]
    public void Reader_NeverReadsPastEnd(byte[] data)
    {
        // Arrange
        var reader = new ByteReader(data ?? Array.Empty<byte>());

        // Act & Assert
        while (reader.CanRead(1))
        {
            reader.ReadByte();
        }

        // Attempt to read beyond should fail gracefully
        reader.Invoking(r => r.ReadByte())
            .Should().Throw<EndOfStreamException>();
    }

    [Theory]
    [InlineData(new byte[] { 0x01 }, 1)]
    [InlineData(new byte[] { 0xFF, 0x00 }, 255)]
    [InlineData(new byte[] { 0x7F, 0x01 }, 255)] // Varint encoding test
    public void ReadVarint_ReturnsCorrectValue(byte[] data, int expected)
    {
        // Arrange
        var reader = new ByteReader(data);

        // Act
        var result = reader.ReadVarint();

        // Assert
        result.Should().Be(expected);
    }
}

// ByteReader implementation for testing
public class ByteReader
{
    private readonly byte[] _buffer;
    private int _position;

    public ByteReader(byte[] buffer)
    {
        _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        _position = 0;
    }

    public int Position => _position;
    public int Length => _buffer.Length;

    public bool CanRead(int count) => _position + count <= _buffer.Length;

    public byte ReadByte()
    {
        if (!CanRead(1))
            throw new EndOfStreamException("Attempted to read beyond buffer end");

        return _buffer[_position++];
    }

    public ushort ReadUInt16()
    {
        if (!CanRead(2))
            throw new EndOfStreamException("Attempted to read beyond buffer end");

        var value = BitConverter.ToUInt16(_buffer, _position);
        _position += 2;
        return value;
    }

    public uint ReadUInt32()
    {
        if (!CanRead(4))
            throw new EndOfStreamException("Attempted to read beyond buffer end");

        var value = BitConverter.ToUInt32(_buffer, _position);
        _position += 4;
        return value;
    }

    public byte[] ReadBytes(int count)
    {
        if (!CanRead(count))
            throw new EndOfStreamException("Attempted to read beyond buffer end");

        var result = new byte[count];
        Array.Copy(_buffer, _position, result, 0, count);
        _position += count;
        return result;
    }

    public string ReadString()
    {
        var length = ReadUInt16();
        if (length == 0) return string.Empty;

        var bytes = ReadBytes(length);
        return Encoding.UTF8.GetString(bytes);
    }

    public int ReadVarint()
    {
        var value = 0;
        var shift = 0;
        byte b;

        do
        {
            b = ReadByte();
            value |= (b & 0x7F) << shift;
            shift += 7;
        } while ((b & 0x80) != 0);

        return value;
    }

    public void Seek(int position)
    {
        if (position < 0 || position > _buffer.Length)
            throw new ArgumentOutOfRangeException(nameof(position));

        _position = position;
    }

    public void Reset() => _position = 0;
}