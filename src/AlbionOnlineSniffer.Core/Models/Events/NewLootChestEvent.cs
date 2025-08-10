using System.Numerics;
using Albion.Network;
using AlbionOnlineSniffer.Core.Utility;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Models.ResponseObj;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class NewLootChestEvent : BaseEvent
    {
        private readonly byte[] offsets;

        public NewLootChestEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            offsets = packetOffsets?.NewLootChest;
            
            Id = Convert.ToInt32(parameters[offsets[0]]);

            if (parameters.ContainsKey(offsets[1]) && parameters[offsets[1]] is byte[] positionBytes)
            {
                PositionBytes = positionBytes;
            }
        }

        public int Id { get; }
        public byte[] PositionBytes { get; private set; }
        public Vector2 Position { get; }
        public string Name { get; }
        public int EnchLvl { get; }
    }
}
