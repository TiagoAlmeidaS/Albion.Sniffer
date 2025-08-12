using Albion.Network;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Models.ResponseObj;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class MobChangeStateEvent : BaseEvent
    {
        private readonly byte[] offsets;

        // Compat constructor for tests using provider-based offsets
        public MobChangeStateEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            var packetOffsets = PacketOffsetsProvider.GetOffsets();
            offsets = packetOffsets?.MobChangeState ?? new byte[] { 0, 1 };
            Id = parameters.TryGetValue(offsets[0], out var idVal) ? Convert.ToInt32(idVal) : 0;
            Charge = parameters.TryGetValue(offsets[1], out var chVal) ? Convert.ToInt32(chVal) : 0;
        }

        public MobChangeStateEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            offsets = packetOffsets?.MobChangeState ?? new byte[] { 0, 1 };
            Id = parameters.TryGetValue(offsets[0], out var idVal) ? Convert.ToInt32(idVal) : 0;
            Charge = parameters.TryGetValue(offsets[1], out var chVal) ? Convert.ToInt32(chVal) : 0;
        }

        public int Id { get; }

        public int Charge { get; }
    }
}
