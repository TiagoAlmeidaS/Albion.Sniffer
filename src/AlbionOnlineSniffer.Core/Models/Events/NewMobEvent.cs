using System.Numerics;
using Albion.Network;
using AlbionOnlineSniffer.Core.Models.GameObjects.Players;
using AlbionOnlineSniffer.Core.Utility;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Models.ResponseObj;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class NewMobEvent : BaseEvent
    {
        private readonly byte[] offsets;

        public NewMobEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            offsets = packetOffsets?.NewMobEvent;
            
            Id = Convert.ToInt32(parameters[offsets[0]]);
            TypeId = Convert.ToInt32(parameters[offsets[1]]);

            if (parameters.ContainsKey(offsets[2]) && parameters[offsets[2]] is byte[] positionBytes)
            {
                PositionBytes = positionBytes;
            }

            Health = Convert.ToSingle(parameters[offsets[3]]);
            MaxHealth = Convert.ToSingle(parameters[offsets[4]]);

            // Pode não existir
            if (parameters.ContainsKey(offsets[5]))
            {
                EnchantmentLevel = Convert.ToByte(parameters[offsets[5]]);
            }
        }

        public int Id { get; }

        public int TypeId { get; }
        public Vector2 Position { get; }

        public Health Health { get; }

        public byte Charge { get; }
    }
}
