using Albion.Network;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Models.ResponseObj;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class HarvestableChangeStateEvent : BaseEvent
    {
        private readonly byte[] offsets;

        public HarvestableChangeStateEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            offsets = packetOffsets?.HarvestableChangeState;
            if (offsets == null || offsets.Length == 0)
            {
                throw new InvalidOperationException("Offsets para HarvestableChangeStateEvent não configurados");
            }
            
            Id = Convert.ToInt32(parameters[offsets[0]]);

            if (offsets.Length > 1 && parameters.ContainsKey(offsets[1]))
                Count = Convert.ToInt32(parameters[offsets[1]]);
            if (offsets.Length > 2 && parameters.ContainsKey(offsets[2]))
                Charge = Convert.ToInt32(parameters[offsets[2]]);
        }

        // Construtor para compatibilidade com framework Albion.Network
        public HarvestableChangeStateEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            var packetOffsets = PacketOffsetsProvider.GetOffsets();
            offsets = packetOffsets?.HarvestableChangeState;
            if (offsets == null || offsets.Length == 0)
            {
                throw new InvalidOperationException("Offsets para HarvestableChangeStateEvent não configurados");
            }
            Id = Convert.ToInt32(parameters[offsets[0]]);
            if (offsets.Length > 1 && parameters.ContainsKey(offsets[1]))
                Count = Convert.ToInt32(parameters[offsets[1]]);
            if (offsets.Length > 2 && parameters.ContainsKey(offsets[2]))
                Charge = Convert.ToInt32(parameters[offsets[2]]);
        }

        public int Id { get; }

        public int Count { get; }
        public int Charge { get; }
    }
}
