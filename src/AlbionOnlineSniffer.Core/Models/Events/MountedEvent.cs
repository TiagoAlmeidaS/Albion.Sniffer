using Albion.Network;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Models.ResponseObj;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class MountedEvent : BaseEvent
    {
        private readonly byte[] offsets;

        // Compat constructor for tests using provider-based offsets
        public MountedEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            var packetOffsets = PacketOffsetsLoader.GlobalPacketOffsets ?? PacketOffsetsProvider.GetOffsets();
            offsets = packetOffsets?.Mounted ?? new byte[] { 0, 1 };
            Id = Convert.ToInt32(parameters[offsets[0]]);
            IsMounted = parameters.TryGetValue(offsets[1], out var v) ? Convert.ToBoolean(v) : parameters.ContainsKey(offsets[1]);
        }

        public MountedEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            offsets = packetOffsets?.Mounted ?? new byte[] { 0, 1 };
            
            Id = Convert.ToInt32(parameters[offsets[0]]);

            IsMounted = parameters.TryGetValue(offsets[1], out var v) ? Convert.ToBoolean(v) : parameters.ContainsKey(offsets[1]);
        }

        public int Id { get; }

        public bool IsMounted { get; }
    }
}
