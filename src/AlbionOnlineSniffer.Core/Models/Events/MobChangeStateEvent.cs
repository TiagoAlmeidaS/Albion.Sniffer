using Albion.Network;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class MobChangeStateEvent : BaseEvent
    {
        byte[] offsets = PacketOffsetsLoader.GlobalPacketOffsets?.MobChangeState;

        public MobChangeStateEvent(Dictionary<byte, object> parameters): base(parameters)
        {
            Id = Convert.ToInt32(parameters[offsets[0]]);

            Charge = Convert.ToInt32(parameters[offsets[1]]);
        }

        public int Id { get; }

        public int Charge { get; }
    }
}
