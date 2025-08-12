using Albion.Network;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Models.ResponseObj;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class KeySyncEvent : BaseEvent
    {
        private readonly byte[] offsets;

        // Construtor para compatibilidade com framework Albion.Network
        public KeySyncEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            var packetOffsets = PacketOffsetsProvider.GetOffsets();
            offsets = packetOffsets?.KeySync ?? Array.Empty<byte>();
            
            InitializeProperties(parameters);
        }

        // Construtor para injeção de dependência direta (se necessário no futuro)
        public KeySyncEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            offsets = packetOffsets?.KeySync ?? Array.Empty<byte>();
            
            InitializeProperties(parameters);
        }

        private void InitializeProperties(Dictionary<byte, object> parameters)
        {
            if (offsets.Length == 0)
            {
                Code = Array.Empty<byte>();
                Key = 0UL;
                return;
            }

            var keyIndex = offsets[0];
            if (!parameters.TryGetValue(keyIndex, out var raw))
            {
                Code = Array.Empty<byte>();
                Key = 0UL;
                return;
            }

            // This event can receive either a byte[] XOR code or a numeric key in the same slot, depending on context
            switch (raw)
            {
                case byte[] bytes:
                    Code = bytes;
                    Key = 0UL;
                    break;
                case sbyte[] sbytes:
                    Code = sbytes.Select(b => unchecked((byte)b)).ToArray();
                    Key = 0UL;
                    break;
                case IEnumerable<byte> enumerableBytes:
                    Code = enumerableBytes.ToArray();
                    Key = 0UL;
                    break;
                case short[] shorts:
                    Code = shorts.Select(s => unchecked((byte)s)).ToArray();
                    Key = 0UL;
                    break;
                case int[] ints:
                    Code = ints.Select(i => unchecked((byte)i)).ToArray();
                    Key = 0UL;
                    break;
                case ulong ulongVal:
                    Key = ulongVal;
                    Code = Array.Empty<byte>();
                    break;
                case long longVal:
                    Key = unchecked((ulong)longVal);
                    Code = Array.Empty<byte>();
                    break;
                case uint uintVal:
                    Key = uintVal;
                    Code = Array.Empty<byte>();
                    break;
                case int intVal:
                    Key = unchecked((ulong)intVal);
                    Code = Array.Empty<byte>();
                    break;
                case string str:
                    // Try parse as Base64 -> XOR bytes; if not, try parse as unsigned number
                    var asBytes = TryParseStringToBytes(str);
                    if (asBytes != null)
                    {
                        Code = asBytes;
                        Key = 0UL;
                    }
                    else if (ulong.TryParse(str, out var parsed))
                    {
                        Key = parsed;
                        Code = Array.Empty<byte>();
                    }
                    else
                    {
                        Code = Array.Empty<byte>();
                        Key = 0UL;
                    }
                    break;
                default:
                    // Fallback: attempt convertible to ulong
                    try
                    {
                        Key = Convert.ToUInt64(raw);
                        Code = Array.Empty<byte>();
                    }
                    catch
                    {
                        Code = Array.Empty<byte>();
                        Key = 0UL;
                    }
                    break;
            }
        }

        public byte[] Code { get; private set; } = Array.Empty<byte>();
        public ulong Key { get; private set; }

        private static byte[]? TryParseStringToBytes(string str)
        {
            // Try Base64
            try
            {
                return Convert.FromBase64String(str);
            }
            catch { }

            // Try HEX without separators
            try
            {
                if (str.Length % 2 == 0 && str.All(Uri.IsHexDigit))
                {
                    var result = new byte[str.Length / 2];
                    for (int i = 0; i < result.Length; i++)
                    {
                        result[i] = Convert.ToByte(str.Substring(i * 2, 2), 16);
                    }
                    return result;
                }
            }
            catch { }

            return null;
        }
    }
}
