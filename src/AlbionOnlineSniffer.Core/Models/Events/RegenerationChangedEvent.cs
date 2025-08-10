using Albion.Network;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Models.ResponseObj;
using AlbionOnlineSniffer.Core.Models.GameObjects.Players;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class RegenerationChangedEvent : BaseEvent
    {
        private readonly byte[] offsets;

        public RegenerationChangedEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            offsets = packetOffsets?.RegenerationHealthChangedEvent;
            
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
