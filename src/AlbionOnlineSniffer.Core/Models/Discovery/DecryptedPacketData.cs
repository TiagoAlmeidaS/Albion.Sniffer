using System;
using System.Collections.Generic;

namespace AlbionOnlineSniffer.Core.Models.Discovery
{
    /// <summary>
    /// Dados de pacotes interceptados para descoberta automática de offsets
    /// </summary>
    public class DecryptedPacketData
    {
        public string EventName { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new();
        public object RawPacket { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsDecrypted { get; set; } = true;
        public string PacketType { get; set; } = string.Empty;
        public int? PacketCode { get; set; }
        public int ParameterCount { get; set; }
    }
}
