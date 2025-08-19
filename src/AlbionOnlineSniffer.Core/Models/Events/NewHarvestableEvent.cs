using Albion.Network;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Models.ResponseObj;
using System.Numerics;

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
            Id = Convert.ToInt32(parameters[offsets[0]]);
            TypeId = Convert.ToInt32(parameters[offsets[1]]);

            if (offsets.Length > 2 && parameters.ContainsKey(offsets[2]) && parameters[offsets[2]] is byte[] positionBytes)
            {
                PositionBytes = positionBytes;
            }
            else
            {
                PositionBytes = Array.Empty<byte>();
            }

            // Tier e Charges podem estar em diferentes offsets dependendo do tipo
            if (offsets.Length > 3 && parameters.ContainsKey(offsets[3]))
            {
                Tier = Convert.ToByte(parameters[offsets[3]]);
            }

            if (offsets.Length > 4 && parameters.ContainsKey(offsets[4]))
            {
                Charges = Convert.ToByte(parameters[offsets[4]]);
            }
        }

        public int Id { get; private set; }
        public int TypeId { get; private set; }
        public byte[] PositionBytes { get; private set; } = Array.Empty<byte>();
        public byte Tier { get; private set; }
        public byte Charges { get; private set; }
    }
}
