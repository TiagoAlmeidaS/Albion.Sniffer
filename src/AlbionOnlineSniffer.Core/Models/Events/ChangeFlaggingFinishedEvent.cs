using Albion.Network;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Models.ResponseObj;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class ChangeFlaggingFinishedEvent : BaseEvent
    {
        private readonly byte[] offsets;

        public ChangeFlaggingFinishedEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            offsets = packetOffsets?.ChangeFlaggingFinished ?? new byte[] { 0 };
            
            Id = Convert.ToInt32(parameters[offsets[0]]);
        }
        
        public int Id { get; }
        public Faction Faction { get; }
    }
}
