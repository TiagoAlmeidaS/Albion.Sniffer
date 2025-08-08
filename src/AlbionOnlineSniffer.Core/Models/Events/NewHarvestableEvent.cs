using System.Numerics;
using Albion.Network;
using AlbionOnlineSniffer.Core.Utility;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class NewHarvestableEvent : BaseEvent, IHasPosition
    {
        byte[] offsets = PacketOffsetsLoader.GlobalPacketOffsets?.NewHarvestableObject;

        public NewHarvestableEvent(Dictionary<byte, object> parameters): base(parameters)
        {
            Id = Convert.ToInt32(parameters[offsets[0]]);

            Type = Convert.ToInt32(parameters[offsets[1]]);
            Tier = Convert.ToInt32(parameters[offsets[2]]);

            Position = Additions.fromFArray((float[])parameters[offsets[3]]);

            Count = parameters.ContainsKey(offsets[4]) ? Convert.ToInt32(parameters[offsets[4]]) : 0;
            Charge = parameters.ContainsKey(offsets[5]) ? Convert.ToInt32(parameters[offsets[5]]) : 0;
        }

        public int Id { get; }

        public int Type { get; }
        public int Tier { get; }

        public Vector2 Position { get; }

        public int Count { get; }
        public int Charge { get; }
    }
}
