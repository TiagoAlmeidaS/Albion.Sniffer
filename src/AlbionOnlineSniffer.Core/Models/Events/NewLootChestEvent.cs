using System.Numerics;
using Albion.Network;
using AlbionOnlineSniffer.Core.Utility;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class NewLootChestEvent : BaseEvent
    {
        byte[] offsets = PacketOffsetsLoader.GlobalPacketOffsets?.NewLootChest;

        public NewLootChestEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            Id = Convert.ToInt32(parameters[offsets[0]]);
            Position = Additions.fromFArray((float[])parameters[offsets[1]]);
            Name = (string)parameters[offsets[2]];
            EnchLvl = 0;
        }

        public int Id { get; }
        public Vector2 Position { get; }
        public string Name { get; }
        public int EnchLvl { get; }
    }
}
