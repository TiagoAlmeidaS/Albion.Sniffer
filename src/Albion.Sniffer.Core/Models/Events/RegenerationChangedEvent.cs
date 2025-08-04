using Albion.Network;
using Albion.Sniffer.Core.Models.GameObjects.Players;
using Albion.Sniffer.Core.Services;

namespace Albion.Sniffer.Core.Models.Events
{
    class RegenerationChangedEvent : BaseEvent
    {
        byte[] offsets = PacketOffsetsLoader.GlobalPacketOffsets?.RegenerationHealthChangedEvent;

        public RegenerationChangedEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            Id = Convert.ToInt32(parameters[offsets[0]]);

            if (parameters.ContainsKey(offsets[1]))
            {
                Health = new Health(parameters.ContainsKey(offsets[2]) ? Convert.ToInt32(parameters[offsets[2]]) : 0, parameters.ContainsKey(offsets[3]) ? Convert.ToInt32(parameters[offsets[3]]) : 0, (float)parameters[offsets[4]]);
            }
            else
            {
                Health = new Health(parameters.ContainsKey(offsets[2]) ? Convert.ToInt32(parameters[offsets[2]]) : 0, parameters.ContainsKey(offsets[3]) ? Convert.ToInt32(parameters[offsets[3]]) : 0);
            }
        }

        public int Id { get; }

        public Health Health { get; }
    }
}
