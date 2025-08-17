using System;
using System.Text;
using FluentAssertions;
using Xunit;
using FsCheck;
using FsCheck.Xunit;

namespace Albion.Sniffer.Core.Tests.Utils;

public class HexDumpTests
{
    [Fact]
    public void HexDump_EmptyBuffer_ReturnsEmptyString()
    {
        // Arrange
        var data = Array.Empty<byte>();

        // Act
        var result = HexDump.Format(data);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void HexDump_NullBuffer_ReturnsEmptyString()
    {
        // Act
        var result = HexDump.Format(null);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void HexDump_SingleByte_FormatsCorrectly()
    {
        // Arrange
        var data = new byte[] { 0x42 };

        // Act
        var result = HexDump.Format(data);

        // Assert
        result.Should().Contain("00000000  42");
        result.Should().Contain("B"); // ASCII representation of 0x42
    }

    [Fact]
    public void HexDump_16Bytes_FormatsInSingleLine()
    {
        // Arrange
        var data = new byte[] 
        { 
            0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x20, 0x57, 0x6F,  // "Hello Wo"
            0x72, 0x6C, 0x64, 0x21, 0x00, 0x01, 0x02, 0x03   // "rld!...."
        };

        // Act
        var result = HexDump.Format(data);

        // Assert
        result.Should().Contain("00000000");
        result.Should().Contain("48 65 6C 6C 6F 20 57 6F  72 6C 64 21 00 01 02 03");
        result.Should().Contain("Hello World!");
    }

    [Fact]
    public void HexDump_MultipleLines_FormatsCorrectly()
    {
        // Arrange
        var data = new byte[32];
        for (int i = 0; i < 32; i++)
        {
            data[i] = (byte)i;
        }

        // Act
        var result = HexDump.Format(data);
        var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Assert
        lines.Should().HaveCount(2);
        lines[0].Should().StartWith("00000000");
        lines[1].Should().StartWith("00000010");
    }

    [Fact]
    public void HexDump_NonPrintableCharacters_ShowsDot()
    {
        // Arrange
        var data = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 };

        // Act
        var result = HexDump.Format(data);

        // Assert
        result.Should().Contain("00 01 02 03 04 05 06 07");
        result.Should().Contain("........"); // All non-printable shown as dots
    }

    [Fact]
    public void HexDump_MixedPrintableAndNonPrintable_FormatsCorrectly()
    {
        // Arrange
        var data = new byte[] { 0x41, 0x00, 0x42, 0x01, 0x43, 0xFF }; // A.B.C.

        // Act
        var result = HexDump.Format(data);

        // Assert
        result.Should().Contain("41 00 42 01 43 FF");
        result.Should().Contain("A.B.C.");
    }

    [Fact]
    public void HexDump_PartialLastLine_PadsCorrectly()
    {
        // Arrange
        var data = new byte[20]; // 1 full line + 4 bytes
        for (int i = 0; i < 20; i++)
        {
            data[i] = (byte)(0x41 + i); // A, B, C, ...
        }

        // Act
        var result = HexDump.Format(data);
        var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Assert
        lines.Should().HaveCount(2);
        lines[1].Should().Contain("00000010");
        lines[1].Should().Contain("51 52 53 54"); // Q R S T
    }

    [Theory]
    [InlineData(1)]
    [InlineData(15)]
    [InlineData(16)]
    [InlineData(17)]
    [InlineData(31)]
    [InlineData(32)]
    [InlineData(100)]
    public void HexDump_VariousSizes_AlwaysAligned(int size)
    {
        // Arrange
        var data = new byte[size];
        new System.Random(42).NextBytes(data);

        // Act
        var result = HexDump.Format(data);
        var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Assert
        foreach (var line in lines)
        {
            line.Should().MatchRegex(@"^[0-9A-F]{8}\s\s.*\|.*\|$");
        }
    }

    [Property(MaxTest = 100)]
    public void HexDump_RandomData_NeverThrows(byte[] data)
    {
        // Act
        Action act = () => HexDump.Format(data);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void HexDump_WithOffset_StartsAtCorrectPosition()
    {
        // Arrange
        var data = new byte[] { 0x41, 0x42, 0x43 };
        var offset = 0x1000;

        // Act
        var result = HexDump.Format(data, offset);

        // Assert
        result.Should().Contain("00001000");
        result.Should().NotContain("00000000");
    }

    [Fact]
    public void HexDump_LargeBuffer_PerformsEfficiently()
    {
        // Arrange
        var data = new byte[1024 * 1024]; // 1MB
        new System.Random(42).NextBytes(data);

        // Act
        var startTime = DateTime.UtcNow;
        var result = HexDump.Format(data);
        var elapsed = DateTime.UtcNow - startTime;

        // Assert
        result.Should().NotBeEmpty();
        elapsed.TotalSeconds.Should().BeLessThan(1); // Should be fast even for 1MB
    }
}

// HexDump utility implementation
public static class HexDump
{
    public static string Format(byte[] data, int offset = 0)
    {
        if (data == null || data.Length == 0)
            return string.Empty;

        var sb = new StringBuilder();
        const int bytesPerLine = 16;

        for (int i = 0; i < data.Length; i += bytesPerLine)
        {
            // Address
            sb.AppendFormat("{0:X8}  ", offset + i);

            // Hex bytes
            for (int j = 0; j < bytesPerLine; j++)
            {
                if (i + j < data.Length)
                {
                    sb.AppendFormat("{0:X2} ", data[i + j]);
                }
                else
                {
                    sb.Append("   ");
                }

                // Add extra space in the middle
                if (j == 7)
                    sb.Append(" ");
            }

            sb.Append(" |");

            // ASCII representation
            for (int j = 0; j < bytesPerLine && i + j < data.Length; j++)
            {
                byte b = data[i + j];
                char c = (b >= 0x20 && b < 0x7F) ? (char)b : '.';
                sb.Append(c);
            }

            // Pad ASCII if needed
            int remaining = Math.Min(bytesPerLine, data.Length - i);
            for (int j = remaining; j < bytesPerLine; j++)
            {
                sb.Append(' ');
            }

            sb.Append('|');

            if (i + bytesPerLine < data.Length)
                sb.AppendLine();
        }

        return sb.ToString();
    }
}

public class BinaryUtilsTests
{
    [Fact]
    public void SwapEndian_UInt16_ReturnsSwapped()
    {
        // Arrange
        ushort value = 0x1234;

        // Act
        var result = BinaryUtils.SwapEndian(value);

        // Assert
        result.Should().Be(0x3412);
    }

    [Fact]
    public void SwapEndian_UInt32_ReturnsSwapped()
    {
        // Arrange
        uint value = 0x12345678;

        // Act
        var result = BinaryUtils.SwapEndian(value);

        // Assert
        result.Should().Be(0x78563412);
    }

    [Fact]
    public void SwapEndian_UInt64_ReturnsSwapped()
    {
        // Arrange
        ulong value = 0x123456789ABCDEF0;

        // Act
        var result = BinaryUtils.SwapEndian(value);

        // Assert
        result.Should().Be(0xF0DEBC9A78563412);
    }

    [Property]
    public void SwapEndian_TwiceReturnsOriginal_UInt16(ushort value)
    {
        // Act
        var swapped = BinaryUtils.SwapEndian(value);
        var result = BinaryUtils.SwapEndian(swapped);

        // Assert
        result.Should().Be(value);
    }

    [Property]
    public void SwapEndian_TwiceReturnsOriginal_UInt32(uint value)
    {
        // Act
        var swapped = BinaryUtils.SwapEndian(value);
        var result = BinaryUtils.SwapEndian(swapped);

        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public void CalculateChecksum_EmptyArray_ReturnsZero()
    {
        // Arrange
        var data = Array.Empty<byte>();

        // Act
        var result = BinaryUtils.CalculateChecksum(data);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void CalculateChecksum_ConsistentForSameData()
    {
        // Arrange
        var data = new byte[] { 1, 2, 3, 4, 5 };

        // Act
        var checksum1 = BinaryUtils.CalculateChecksum(data);
        var checksum2 = BinaryUtils.CalculateChecksum(data);

        // Assert
        checksum1.Should().Be(checksum2);
    }

    [Fact]
    public void CalculateChecksum_DifferentForDifferentData()
    {
        // Arrange
        var data1 = new byte[] { 1, 2, 3, 4, 5 };
        var data2 = new byte[] { 5, 4, 3, 2, 1 };

        // Act
        var checksum1 = BinaryUtils.CalculateChecksum(data1);
        var checksum2 = BinaryUtils.CalculateChecksum(data2);

        // Assert
        checksum1.Should().NotBe(checksum2);
    }

    [Property]
    public void CalculateChecksum_NeverThrows(byte[] data)
    {
        // Act
        Action act = () => BinaryUtils.CalculateChecksum(data);

        // Assert
        act.Should().NotThrow();
    }
}

// BinaryUtils implementation
public static class BinaryUtils
{
    public static ushort SwapEndian(ushort value)
    {
        return (ushort)((value >> 8) | (value << 8));
    }

    public static uint SwapEndian(uint value)
    {
        return ((value & 0x000000FF) << 24) |
               ((value & 0x0000FF00) << 8) |
               ((value & 0x00FF0000) >> 8) |
               ((value & 0xFF000000) >> 24);
    }

    public static ulong SwapEndian(ulong value)
    {
        return ((value & 0x00000000000000FF) << 56) |
               ((value & 0x000000000000FF00) << 40) |
               ((value & 0x0000000000FF0000) << 24) |
               ((value & 0x00000000FF000000) << 8) |
               ((value & 0x000000FF00000000) >> 8) |
               ((value & 0x0000FF0000000000) >> 24) |
               ((value & 0x00FF000000000000) >> 40) |
               ((value & 0xFF00000000000000) >> 56);
    }

    public static uint CalculateChecksum(byte[] data)
    {
        if (data == null || data.Length == 0)
            return 0;

        uint checksum = 0;
        foreach (var b in data)
        {
            checksum = (checksum << 1) + (checksum >> 31) + b;
        }
        return checksum;
    }
}