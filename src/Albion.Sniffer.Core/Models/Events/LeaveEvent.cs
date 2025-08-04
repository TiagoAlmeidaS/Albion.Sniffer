using Albion.Network;
using Albion.Sniffer.Core.Services;

namespace Albion.Sniffer.Core.Models.Events
{
    public class LeaveEvent : BaseEvent
    {
        byte[] offsets = PacketOffsetsLoader.GlobalPacketOffsets?.Leave;

        public LeaveEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            Id = parameters.ContainsKey(offsets[0]) ? Convert.ToInt32(parameters[offsets[0]]) : 0;
        }

        public int Id { get; }
    }
}
