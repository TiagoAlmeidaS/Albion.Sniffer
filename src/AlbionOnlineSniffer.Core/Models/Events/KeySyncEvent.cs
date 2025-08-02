using Albion.Network;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class KeySyncEvent : BaseEvent
    {
        byte[] offsets = PacketOffsetsLoader.GlobalPacketOffsets?.KeySync;

        public KeySyncEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            Code = parameters.ContainsKey(offsets[0]) ? parameters[offsets[0]] as byte[] : null;
        }

        public byte[] Code { get; }
    }
}
