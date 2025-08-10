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
            
            Id = Convert.ToInt32(parameters[offsets[0]]);

            Count = parameters.ContainsKey(offsets[1]) ? Convert.ToInt32(parameters[offsets[1]]) : 0;
            Charge = parameters.ContainsKey(offsets[2]) ? Convert.ToInt32(parameters[offsets[2]]) : 0;
        }

        public int Id { get; }

        public int Count { get; }
        public int Charge { get; }
    }
}
