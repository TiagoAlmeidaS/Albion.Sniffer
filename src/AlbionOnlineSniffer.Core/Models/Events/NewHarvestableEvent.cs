using System.Numerics;
using Albion.Network;
using AlbionOnlineSniffer.Core.Utility;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Models.ResponseObj;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class NewHarvestableEvent : BaseEvent
    {
        private readonly byte[] offsets;

        // Construtor para compatibilidade com framework Albion.Network
        public NewHarvestableEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            var packetOffsets = PacketOffsetsProvider.GetOffsets();
            offsets = packetOffsets?.NewHarvestableObject;
            
            InitializeProperties(parameters);
        }

        // Construtor para injeção de dependência direta (se necessário no futuro)
        public NewHarvestableEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            offsets = packetOffsets?.NewHarvestableObject;
            
            InitializeProperties(parameters);
        }

        private void InitializeProperties(Dictionary<byte, object> parameters)
        {
            Id = Convert.ToInt32(parameters[offsets[0]]);
            TypeId = Convert.ToInt32(parameters[offsets[1]]);

            if (parameters.ContainsKey(offsets[2]) && parameters[offsets[2]] is byte[] positionBytes)
            {
                PositionBytes = positionBytes;
            }

            // Tier e Charges podem estar em diferentes offsets dependendo do tipo
            if (parameters.ContainsKey(offsets[3]))
            {
                Tier = Convert.ToByte(parameters[offsets[3]]);
            }

            if (parameters.ContainsKey(offsets[4]))
            {
                Charges = Convert.ToByte(parameters[offsets[4]]);
            }
        }

        public int Id { get; }

        public int Type { get; }
        public int Tier { get; }

        public Vector2 Position { get; }

        public int Count { get; }
        public int Charge { get; }
    }
}
