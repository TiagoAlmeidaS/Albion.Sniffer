using Albion.Network;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Models.ResponseObj;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class HealthUpdateEvent : BaseEvent
    {
        private readonly byte[] offsets;

        public HealthUpdateEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            offsets = packetOffsets?.HealthUpdateEvent;
            
            Id = Convert.ToInt32(parameters[offsets[0]]);
            Health = Convert.ToSingle(parameters[offsets[1]]);
            MaxHealth = Convert.ToSingle(parameters[offsets[2]]);
            // Pode ter energia/mana em alguns casos
            if (parameters.ContainsKey(offsets[3]))
            {
                Energy = Convert.ToSingle(parameters[offsets[3]]);
            }
        }

        public int Id { get; }

        public int Health { get; }
    }
}
