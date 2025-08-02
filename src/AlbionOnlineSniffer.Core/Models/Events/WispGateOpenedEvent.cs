using Albion.Network;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class WispGateOpenedEvent : BaseEvent
    {
        byte[] offsets = PacketOffsetsLoader.GlobalPacketOffsets?.WispGateOpened;
        
        public WispGateOpenedEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            Id = Convert.ToInt32(parameters[offsets[0]]);
            isCollected = parameters.ContainsKey(offsets[1]) && parameters[offsets[1]].ToString() == "2";
        }

        public int Id { get; }
        
        public bool isCollected { get; }
    }
}
