using Albion.Network;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Models.ResponseObj;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class WispGateOpenedEvent : BaseEvent
    {
        private readonly byte[] offsets;

        public WispGateOpenedEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            offsets = packetOffsets?.WispGateOpened;
            
            Id = Convert.ToInt32(parameters[offsets[0]]);
            isCollected = parameters.ContainsKey(offsets[1]) && parameters[offsets[1]].ToString() == "2";
        }

        public int Id { get; }
        
        public bool isCollected { get; }
    }
}
