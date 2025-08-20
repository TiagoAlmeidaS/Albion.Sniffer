using Albion.Network;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Models.ResponseObj;
using System.Numerics;
using AlbionOnlineSniffer.Core.Utility;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class NewHarvestableEvent : BaseEvent
    {
        private readonly byte[] offsets;

        // Construtor para compatibilidade com framework Albion.Network
        public NewHarvestableEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            var packetOffsets = PacketOffsetsLoader.GlobalPacketOffsets ?? PacketOffsetsProvider.GetOffsets();
            offsets = packetOffsets?.NewHarvestableObject ?? new byte[] { 0, 1, 2, 3, 4 };
            
            InitializeProperties(parameters);
        }

        // Construtor para injeção de dependência direta (se necessário no futuro)
        public NewHarvestableEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            offsets = packetOffsets?.NewHarvestableObject ?? new byte[] { 0, 1, 2, 3, 4 };
            
            InitializeProperties(parameters);
        }

        private void InitializeProperties(Dictionary<byte, object> parameters)
        {
            // ✅ SEGURO: Usar SafeParameterExtractor para evitar KeyNotFoundException
            Id = SafeParameterExtractor.GetInt32(parameters, offsets[0]);
            TypeId = SafeParameterExtractor.GetInt32(parameters, offsets[1]);
            
            // ✅ SEGURO: Usar SafeParameterExtractor para arrays
            PositionBytes = SafeParameterExtractor.GetByteArray(parameters, offsets[2]);
            
            // ✅ SEGURO: Usar SafeParameterExtractor para bytes
            Tier = SafeParameterExtractor.GetByte(parameters, offsets[3]);
            Charges = SafeParameterExtractor.GetByte(parameters, offsets[4]);
        }

        public int Id { get; private set; }
        public int TypeId { get; private set; }
        public byte[] PositionBytes { get; private set; } = Array.Empty<byte>();
        public byte Tier { get; private set; }
        public byte Charges { get; private set; }
    }
}
