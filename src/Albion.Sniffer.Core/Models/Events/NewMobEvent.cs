using System.Numerics;
using Albion.Network;
using Albion.Sniffer.Core.Models.GameObjects.Players;
using Albion.Sniffer.Core.Utility;
using Albion.Sniffer.Core.Services;

namespace Albion.Sniffer.Core.Models.Events
{
    class NewMobEvent : BaseEvent
    {
        byte[] offsets = PacketOffsetsLoader.GlobalPacketOffsets?.NewMobEvent;

        public NewMobEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            Id = Convert.ToInt32(parameters[offsets[0]]);
            TypeId = Convert.ToInt32(parameters[offsets[1]]) - 15;
            Position = Additions.fromFArray((float[])parameters[offsets[2]]);

            Health = parameters.ContainsKey(offsets[3]) ? 
                new Health(Convert.ToInt32(parameters[offsets[3]]), Convert.ToInt32(parameters[offsets[4]])) 
                : new Health(Convert.ToInt32(parameters[offsets[4]]));

            Charge = (byte)(parameters.ContainsKey(offsets[5]) ? Convert.ToInt32(parameters[offsets[5]]) : 0);
        }

        public int Id { get; }

        public int TypeId { get; }
        public Vector2 Position { get; }

        public Health Health { get; }

        public byte Charge { get; }
    }
}
