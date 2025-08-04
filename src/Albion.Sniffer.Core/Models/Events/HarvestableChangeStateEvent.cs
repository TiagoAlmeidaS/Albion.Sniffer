using Albion.Network;
using Albion.Sniffer.Core.Services;

namespace Albion.Sniffer.Core.Models.Events
{
    public class HarvestableChangeStateEvent : BaseEvent
    {
        byte[] offsets = PacketOffsetsLoader.GlobalPacketOffsets?.HarvestableChangeState;

        public HarvestableChangeStateEvent(Dictionary<byte, object> parameters): base(parameters)
        {
            Id = Convert.ToInt32(parameters[offsets[0]]);

            Count = parameters.ContainsKey(offsets[1]) ? Convert.ToInt32(parameters[offsets[1]]) : 0;
            Charge = parameters.ContainsKey(offsets[2]) ? Convert.ToInt32(parameters[offsets[2]]) : 0;
        }

        public int Id { get; }

        public int Count { get; }
        public int Charge { get; }
    }
}
