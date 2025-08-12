using Albion.Network;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Models.ResponseObj;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class LeaveEvent : BaseEvent
    {
        private readonly byte[] offsets;

        // Compat: construtor antigo
        public LeaveEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            var packetOffsets = PacketOffsetsProvider.GetOffsets();
            offsets = packetOffsets?.Leave;
            Id = Convert.ToInt32(parameters[offsets[0]]);
        }

        public LeaveEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            offsets = packetOffsets?.Leave;
            
            Id = Convert.ToInt32(parameters[offsets[0]]);
        }

        public int Id { get; }
    }
}
