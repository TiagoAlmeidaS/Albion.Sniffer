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
            offsets = packetOffsets?.NewMobEvent ?? Array.Empty<byte>();
            
            InitializeProperties(parameters);
        }

        // Construtor para injeção de dependência direta (se necessário no futuro)
        public NewMobEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            offsets = packetOffsets?.NewMobEvent ?? Array.Empty<byte>();
            
            InitializeProperties(parameters);
        }

        private void InitializeProperties(Dictionary<byte, object> parameters)
        {
            // ✅ SEGURO: Usar SafeParameterExtractor para evitar KeyNotFoundException
            Id = SafeParameterExtractor.GetInt32(parameters, offsets[0]);
            TypeId = SafeParameterExtractor.GetInt32(parameters, offsets[1]);
            PositionBytes = SafeParameterExtractor.GetByteArray(parameters, offsets[2]);
            Health = SafeParameterExtractor.GetFloat(parameters, offsets[3]);
            MaxHealth = SafeParameterExtractor.GetFloat(parameters, offsets[4]);
            EnchantmentLevel = SafeParameterExtractor.GetByte(parameters, offsets[5]);
        }

        public int Id { get; private set; }
        public int TypeId { get; private set; }
        public byte[] PositionBytes { get; private set; } = Array.Empty<byte>();
        public float Health { get; private set; }
        public float MaxHealth { get; private set; }
        public byte EnchantmentLevel { get; private set; }
    }
}
