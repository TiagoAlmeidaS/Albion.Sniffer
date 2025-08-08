using System.Numerics;
using Albion.Network;
using AlbionOnlineSniffer.Core.Utility;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class NewGatedWispEvent : BaseEvent, IHasPosition
    {
        byte[] offsets = PacketOffsetsLoader.GlobalPacketOffsets?.NewWispGate;

        public NewGatedWispEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            Id = Convert.ToInt32(parameters[offsets[0]]);
            Position = Additions.fromFArray((float[])parameters[offsets[1]]);
            isCollected = parameters.ContainsKey(offsets[2]) && parameters[offsets[2]].ToString() == "2";
        }

        public int Id { get; }

        public Vector2 Position { get; }

        public bool isCollected { get; }
    }
}
