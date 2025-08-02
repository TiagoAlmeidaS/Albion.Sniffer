using Albion.Network;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Utility;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class ChangeFlaggingFinishedEvent : BaseEvent
    {
        byte[] offsets = PacketOffsetsLoader.GlobalPacketOffsets?.ChangeFlaggingFinished ?? new byte[] { 0 };

        public ChangeFlaggingFinishedEvent(Dictionary<byte, object> parameters): base(parameters)
        {
            Id = Convert.ToInt32(parameters[offsets[0]]);
            Faction = (Faction)parameters[offsets[1]];
        }
        
        public int Id { get; }
        public Faction Faction { get; }
    }
}
