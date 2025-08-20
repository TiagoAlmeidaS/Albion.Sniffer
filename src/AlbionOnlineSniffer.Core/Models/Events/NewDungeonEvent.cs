using Albion.Network;
using AlbionOnlineSniffer.Core.Utility;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Models.ResponseObj;
using System.Numerics;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class NewDungeonEvent : BaseEvent
    {
        private readonly byte[] offsets;

        public NewDungeonEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            offsets = packetOffsets?.NewDungeonExit ?? new byte[] { 0, 1, 2, 3 };
            
            // ✅ SEGURO: Usar SafeParameterExtractor para evitar KeyNotFoundException
            Id = SafeParameterExtractor.GetInt32(parameters, offsets[0]);
            PositionBytes = SafeParameterExtractor.GetByteArray(parameters, offsets[1]);
            Type = SafeParameterExtractor.GetString(parameters, offsets[2], "NULL");
            Charges = SafeParameterExtractor.GetInt32(parameters, offsets[3]);
        }

        public int Id { get; private set; }
        public byte[] PositionBytes { get; private set; }
        public string Type { get; private set; }
        public int Charges { get; private set; }
    }
}
