using Albion.Network;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class MountedEvent : BaseEvent
    {
        byte[] offsets = PacketOffsetsLoader.GlobalPacketOffsets?.Mounted;

        public MountedEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            Id = Convert.ToInt32(parameters[offsets[0]]);

            IsMounted = parameters.ContainsKey(offsets[1]);
        }

        public int Id { get; }

        public bool IsMounted { get; }
    }
}
