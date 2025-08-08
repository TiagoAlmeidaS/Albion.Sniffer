using System.Numerics;
using Albion.Network;
using AlbionOnlineSniffer.Core.Utility;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class NewFishingZoneEvent : BaseEvent, IHasPosition
    {
        byte[] offsets = PacketOffsetsLoader.GlobalPacketOffsets?.NewFishingZoneObject;

        public NewFishingZoneEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            Id = Convert.ToInt32(parameters[offsets[0]]);

            Position = Additions.fromFArray((float[])parameters[offsets[1]]);

            Size = parameters.ContainsKey(offsets[2]) ? Convert.ToInt32(parameters[offsets[2]]) : 0;
            RespawnCount = parameters.ContainsKey(offsets[3]) ? Convert.ToInt32(parameters[offsets[3]]) : 0;
        }

        public int Id { get; }

        public Vector2 Position { get; }

        public int Size { get; }
        public int RespawnCount { get; }
    }
}
