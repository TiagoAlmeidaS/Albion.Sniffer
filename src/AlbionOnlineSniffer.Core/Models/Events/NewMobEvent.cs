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

        // Construtor para compatibilidade com framework Albion.Network
        public NewMobEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            var packetOffsets = PacketOffsetsProvider.GetOffsets();
            offsets = packetOffsets?.NewMobEvent;
            
            InitializeProperties(parameters);
        }

        // Construtor para injeção de dependência direta (se necessário no futuro)
        public NewMobEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            offsets = packetOffsets?.NewMobEvent;
            
            InitializeProperties(parameters);
        }

        private void InitializeProperties(Dictionary<byte, object> parameters)
        {
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

        public int Id { get; private set; }
        public int TypeId { get; private set; }
        public byte[] PositionBytes { get; private set; }
        public float Health { get; private set; }
        public float MaxHealth { get; private set; }
        public byte EnchantmentLevel { get; private set; }
    }
}
