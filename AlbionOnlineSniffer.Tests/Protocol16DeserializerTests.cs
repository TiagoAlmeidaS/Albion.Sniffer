using Xunit;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace AlbionOnlineSniffer.Tests
{
    public class Protocol16DeserializerTests
    {
        [Fact]
        public void Deserialize_Byte_ReturnsCorrectValue()
        {
            byte expected = 123;
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(expected);
                stream.Position = 0;
                using (var reader = new BinaryReader(stream))
                {
                    var result = AlbionOnlineSniffer.Core.Services.Protocol16Deserializer.Deserialize(reader, (byte)AlbionOnlineSniffer.Core.Enums.Protocol16Type.Byte);
                    Assert.Equal(expected, (byte)result);
                }
            }
        }

        [Fact]
        public void Deserialize_Boolean_ReturnsCorrectValue()
        {
            bool expected = true;
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(expected);
                stream.Position = 0;
                using (var reader = new BinaryReader(stream))
                {
                    var result = AlbionOnlineSniffer.Core.Services.Protocol16Deserializer.Deserialize(reader, (byte)AlbionOnlineSniffer.Core.Enums.Protocol16Type.Boolean);
                    Assert.Equal(expected, (bool)result);
                }
            }
        }

        [Fact]
        public void Deserialize_Short_ReturnsCorrectValue()
        {
            short expected = 32767;
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(expected);
                stream.Position = 0;
                using (var reader = new BinaryReader(stream))
                {
                    var result = AlbionOnlineSniffer.Core.Services.Protocol16Deserializer.Deserialize(reader, (byte)AlbionOnlineSniffer.Core.Enums.Protocol16Type.Short);
                    Assert.Equal(expected, (short)result);
                }
            }
        }

        [Fact]
        public void Deserialize_Integer_ReturnsCorrectValue()
        {
            int expected = 2147483647;
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(expected);
                stream.Position = 0;
                using (var reader = new BinaryReader(stream))
                {
                    var result = AlbionOnlineSniffer.Core.Services.Protocol16Deserializer.Deserialize(reader, (byte)AlbionOnlineSniffer.Core.Enums.Protocol16Type.Integer);
                    Assert.Equal(expected, (int)result);
                }
            }
        }

        [Fact]
        public void Deserialize_Long_ReturnsCorrectValue()
        {
            long expected = 9223372036854775807L;
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(expected);
                stream.Position = 0;
                using (var reader = new BinaryReader(stream))
                {
                    var result = AlbionOnlineSniffer.Core.Services.Protocol16Deserializer.Deserialize(reader, (byte)AlbionOnlineSniffer.Core.Enums.Protocol16Type.Long);
                    Assert.Equal(expected, (long)result);
                }
            }
        }

        [Fact]
        public void Deserialize_Float_ReturnsCorrectValue()
        {
            float expected = 123.45f;
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(expected);
                stream.Position = 0;
                using (var reader = new BinaryReader(stream))
                {
                    var result = AlbionOnlineSniffer.Core.Services.Protocol16Deserializer.Deserialize(reader, (byte)AlbionOnlineSniffer.Core.Enums.Protocol16Type.Float);
                    Assert.Equal(expected, (float)result);
                }
            }
        }

        [Fact]
        public void Deserialize_Double_ReturnsCorrectValue()
        {
            double expected = 123.456789;
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(expected);
                stream.Position = 0;
                using (var reader = new BinaryReader(stream))
                {
                    var result = AlbionOnlineSniffer.Core.Services.Protocol16Deserializer.Deserialize(reader, (byte)AlbionOnlineSniffer.Core.Enums.Protocol16Type.Double);
                    Assert.Equal(expected, (double)result);
                }
            }
        }

        [Fact]
        public void Deserialize_String_ReturnsCorrectValue()
        {
            string expected = "Hello World";
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write((short)Encoding.UTF8.GetByteCount(expected));
                writer.Write(Encoding.UTF8.GetBytes(expected));
                stream.Position = 0;
                using (var reader = new BinaryReader(stream))
                {
                    var result = AlbionOnlineSniffer.Core.Services.Protocol16Deserializer.Deserialize(reader, (byte)AlbionOnlineSniffer.Core.Enums.Protocol16Type.String);
                    Assert.Equal(expected, (string)result);
                }
            }
        }

        [Fact]
        public void Deserialize_String_NullReturnsNull()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write((short)-1);
                stream.Position = 0;
                using (var reader = new BinaryReader(stream))
                {
                    var result = AlbionOnlineSniffer.Core.Services.Protocol16Deserializer.Deserialize(reader, (byte)AlbionOnlineSniffer.Core.Enums.Protocol16Type.String);
                    Assert.Null(result);
                }
            }
        }

        [Fact]
        public void Deserialize_ByteArray_ReturnsCorrectValue()
        {
            byte[] expected = { 1, 2, 3, 4, 5 };
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(expected.Length);
                writer.Write(expected);
                stream.Position = 0;
                using (var reader = new BinaryReader(stream))
                {
                    var result = AlbionOnlineSniffer.Core.Services.Protocol16Deserializer.Deserialize(reader, (byte)AlbionOnlineSniffer.Core.Enums.Protocol16Type.ByteArray);
                    Assert.Equal(expected, (byte[])result);
                }
            }
        }

        [Fact]
        public void Deserialize_IntegerArray_ReturnsCorrectValue()
        {
            int[] expected = { 10, 20, 30 };
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(expected.Length);
                foreach (var item in expected)
                {
                    writer.Write(item);
                }
                stream.Position = 0;
                using (var reader = new BinaryReader(stream))
                {
                    var result = AlbionOnlineSniffer.Core.Services.Protocol16Deserializer.Deserialize(reader, (byte)AlbionOnlineSniffer.Core.Enums.Protocol16Type.IntegerArray);
                    Assert.Equal(expected, (int[])result);
                }
            }
        }

        [Fact]
        public void Deserialize_StringArray_ReturnsCorrectValue()
        {
            string[] expected = { "one", "two", "three" };
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write((short)expected.Length);
                foreach (var item in expected)
                {
                    writer.Write((short)Encoding.UTF8.GetByteCount(item));
                    writer.Write(Encoding.UTF8.GetBytes(item));
                }
                stream.Position = 0;
                using (var reader = new BinaryReader(stream))
                {
                    var result = AlbionOnlineSniffer.Core.Services.Protocol16Deserializer.Deserialize(reader, (byte)AlbionOnlineSniffer.Core.Enums.Protocol16Type.StringArray);
                    Assert.Equal(expected, (string[])result);
                }
            }
        }

        [Fact]
        public void Deserialize_ObjectArray_ReturnsCorrectValue()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write((short)2); // Length
                writer.Write((byte)AlbionOnlineSniffer.Core.Enums.Protocol16Type.Integer);
                writer.Write(123);
                writer.Write((byte)AlbionOnlineSniffer.Core.Enums.Protocol16Type.String);
                writer.Write((short)Encoding.UTF8.GetByteCount("test"));
                writer.Write(Encoding.UTF8.GetBytes("test"));
                stream.Position = 0;
                using (var reader = new BinaryReader(stream))
                {
                    var result = AlbionOnlineSniffer.Core.Services.Protocol16Deserializer.Deserialize(reader, (byte)AlbionOnlineSniffer.Core.Enums.Protocol16Type.ObjectArray);
                    var resultArray = (object[])result;
                    Assert.Equal(2, resultArray.Length);
                    Assert.Equal(123, (int)resultArray[0]);
                    Assert.Equal("test", (string)resultArray[1]);
                }
            }
        }

        [Fact]
        public void Deserialize_Hashtable_ReturnsCorrectValue()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write((short)1); // Size
                writer.Write((byte)AlbionOnlineSniffer.Core.Enums.Protocol16Type.String); // Key type
                writer.Write((short)Encoding.UTF8.GetByteCount("key"));
                writer.Write(Encoding.UTF8.GetBytes("key"));
                writer.Write((byte)AlbionOnlineSniffer.Core.Enums.Protocol16Type.Integer); // Value type
                writer.Write(456);
                stream.Position = 0;
                using (var reader = new BinaryReader(stream))
                {
                    var result = AlbionOnlineSniffer.Core.Services.Protocol16Deserializer.Deserialize(reader, (byte)AlbionOnlineSniffer.Core.Enums.Protocol16Type.Hashtable);
                    var resultDict = (Dictionary<object, object>)result;
                    Assert.Single(resultDict);
                    Assert.Equal(456, (int)resultDict["key"]);
                }
            }
        }

        [Fact]
        public void Deserialize_Dictionary_ReturnsCorrectValue()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write((byte)AlbionOnlineSniffer.Core.Enums.Protocol16Type.String); // Key type
                writer.Write((byte)AlbionOnlineSniffer.Core.Enums.Protocol16Type.Integer); // Value type
                writer.Write((short)1); // Size
                writer.Write((short)Encoding.UTF8.GetByteCount("key"));
                writer.Write(Encoding.UTF8.GetBytes("key"));
                writer.Write(789);
                stream.Position = 0;
                using (var reader = new BinaryReader(stream))
                {
                    var result = AlbionOnlineSniffer.Core.Services.Protocol16Deserializer.Deserialize(reader, (byte)AlbionOnlineSniffer.Core.Enums.Protocol16Type.Dictionary);
                    var resultDict = (Dictionary<object, object>)result;
                    Assert.Single(resultDict);
                    Assert.Equal(789, (int)resultDict["key"]);
                }
            }
        }

        [Fact]
        public void Deserialize_UnsupportedType_ThrowsException()
        {
            using (var stream = new MemoryStream())
            using (var reader = new BinaryReader(stream))
            {
                Assert.Throws<NotSupportedException>(() => AlbionOnlineSniffer.Core.Services.Protocol16Deserializer.Deserialize(reader, 99)); // Um tipo não mapeado
            }
        }
    }
}

