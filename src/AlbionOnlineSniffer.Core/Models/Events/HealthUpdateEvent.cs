using Albion.Network;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Models.ResponseObj;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class HealthUpdateEvent : BaseEvent
    {
        private readonly byte[] offsets;

        // Compat: construtor antigo
        public HealthUpdateEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            var packetOffsets = PacketOffsetsLoader.GlobalPacketOffsets ?? PacketOffsetsProvider.GetOffsets();
            offsets = packetOffsets?.HealthUpdateEvent ?? new byte[] { 0, 1, 2, 3 };
            Id = Convert.ToInt32(parameters[offsets[0]]);
            Health = Convert.ToSingle(parameters[offsets[1]]);
            MaxHealth = Convert.ToSingle(parameters[offsets[2]]);
            if (offsets.Length > 3 && parameters.ContainsKey(offsets[3]))
            {
                Energy = Convert.ToSingle(parameters[offsets[3]]);
            }
        }

        public HealthUpdateEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            offsets = packetOffsets?.HealthUpdateEvent ?? new byte[] { 0, 1, 2, 3 };
            
            Id = Convert.ToInt32(parameters[offsets[0]]);
            Health = Convert.ToSingle(parameters[offsets[1]]);
            MaxHealth = Convert.ToSingle(parameters[offsets[2]]);
            // Pode ter energia/mana em alguns casos
            if (offsets.Length > 3 && parameters.ContainsKey(offsets[3]))
            {
                Energy = Convert.ToSingle(parameters[offsets[3]]);
            }
        }

        public int Id { get; private set; }
        public float Health { get; private set; }
        public float MaxHealth { get; private set; }
        public float Energy { get; private set; }
    }
}
