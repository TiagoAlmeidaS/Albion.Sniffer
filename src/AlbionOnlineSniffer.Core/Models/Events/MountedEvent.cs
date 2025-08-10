using Albion.Network;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Models.ResponseObj;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class MountedEvent : BaseEvent
    {
        private readonly byte[] offsets;

        public MountedEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            offsets = packetOffsets?.Mounted;
            
            Id = Convert.ToInt32(parameters[offsets[0]]);

            IsMounted = parameters.ContainsKey(offsets[1]);
        }

        public int Id { get; }

        public bool IsMounted { get; }
    }
}
