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
            offsets = packetOffsets?.MobChangeState ?? throw new NullReferenceException();
            Id = Convert.ToInt32(parameters[offsets[0]]);
            Charge = Convert.ToInt32(parameters[offsets[1]]);
        }

        public MobChangeStateEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            offsets = packetOffsets?.MobChangeState;
            
            Id = Convert.ToInt32(parameters[offsets[0]]);

            Charge = Convert.ToInt32(parameters[offsets[1]]);
        }

        public int Id { get; }

        public int Charge { get; }
    }
}
