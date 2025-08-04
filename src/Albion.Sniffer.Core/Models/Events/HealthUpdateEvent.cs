using Albion.Network;
using Albion.Sniffer.Core.Services;

namespace Albion.Sniffer.Core.Models.Events
{
    class HealthUpdateEvent : BaseEvent
    {
        byte[] offsets = PacketOffsetsLoader.GlobalPacketOffsets?.HealthUpdateEvent;

        public HealthUpdateEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            Id = Convert.ToInt32(parameters[offsets[0]]);

            Health = parameters.ContainsKey(offsets[1]) ? Convert.ToInt32(parameters[offsets[1]]) : 0;
        }

        public int Id { get; }

        public int Health { get; }
    }
}
