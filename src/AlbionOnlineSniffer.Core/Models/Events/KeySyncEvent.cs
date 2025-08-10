using Albion.Network;
using AlbionOnlineSniffer.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class KeySyncEvent : BaseEvent
    {
        byte[] offsets = PacketOffsetsLoader.GlobalPacketOffsets?.KeySync;

        public KeySyncEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            Code = ExtractXorCode(parameters);
        }

        public byte[] Code { get; }

        private byte[]? ExtractXorCode(Dictionary<byte, object> parameters)
        {
            try
            {
                if (offsets == null || offsets.Length == 0)
                {
                    return null;
                }

                var key = offsets[0];
                if (!parameters.ContainsKey(key))
                {
                    return null;
                }

                var value = parameters[key];
                return ConvertToByteArray(value);
            }
            catch
            {
                return null;
            }
        }

        private static byte[]? ConvertToByteArray(object value)
        {
            if (value == null)
            {
                return null;
            }

            switch (value)
            {
                case byte[] bytes:
                    return bytes;
                case sbyte[] sbytes:
                    return sbytes.Select(b => unchecked((byte)b)).ToArray();
                case short[] shorts:
                    return shorts.Select(s => unchecked((byte)s)).ToArray();
                case int[] ints:
                    return ints.Select(i => unchecked((byte)i)).ToArray();
                case IEnumerable<byte> enumerableBytes:
                    return enumerableBytes.ToArray();
                case string str:
                    // Tentar Base64
                    try
                    {
                        return Convert.FromBase64String(str);
                    }
                    catch
                    {
                        // Tentar HEX sem separadores
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
                        catch
                        {
                            // Ignorar
                        }
                    }
                    return null;
                default:
                    return null;
            }
        }
    }
}
