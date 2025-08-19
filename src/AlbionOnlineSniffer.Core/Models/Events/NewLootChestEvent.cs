using Albion.Network;
using AlbionOnlineSniffer.Core.Utility;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Models.ResponseObj;
using System.Numerics;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class NewLootChestEvent : BaseEvent
    {
        private readonly byte[] offsets;

        public NewLootChestEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            offsets = packetOffsets?.NewLootChest ?? new byte[] { 0, 1 };
            
            Id = Convert.ToInt32(parameters[offsets[0]]);

            if (parameters.ContainsKey(offsets[1]) && parameters[offsets[1]] is byte[] positionBytes)
            {
                PositionBytes = positionBytes;
            }
            else
            {
                PositionBytes = Array.Empty<byte>();
            }
        }

        public int Id { get; private set; }
        public byte[] PositionBytes { get; private set; }
    }
}
