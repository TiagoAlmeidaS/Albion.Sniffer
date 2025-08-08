using System.Numerics;
using Albion.Network;
using AlbionOnlineSniffer.Core.Utility;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class NewDungeonEvent : BaseEvent, IHasPosition
    {
        byte[] offsets = PacketOffsetsLoader.GlobalPacketOffsets?.NewDungeonExit;

        public NewDungeonEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            Id = Convert.ToInt32(parameters[offsets[0]]);

            Position = Additions.fromFArray((float[])parameters[offsets[1]]);

            Type = parameters.ContainsKey(offsets[2]) ? parameters[offsets[2]] as string : "NULL";

            Charges = Convert.ToInt32(parameters[offsets[3]]);
        }

        public int Id { get; }

        public Vector2 Position { get; }

        public string Type { get; }

        public int Charges { get; }
    }
}
