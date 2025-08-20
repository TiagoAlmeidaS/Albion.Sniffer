using Albion.Network;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Models.ResponseObj;
using AlbionOnlineSniffer.Core.Utility;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class HealthUpdateEvent : BaseEvent
    {
        private readonly byte[] offsets;

        // Compat: construtor antigo
        public HealthUpdateEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            var packetOffsets = PacketOffsetsLoader.GlobalPacketOffsets ?? PacketOffsetsProvider.GetOffsets();
            offsets = packetOffsets?.HealthUpdateEvent ?? new byte[] { 0, 3 };
            
            // ✅ SEGURO: Usar SafeParameterExtractor para evitar KeyNotFoundException
            // offsets.json tem apenas [0, 3], então usamos valores padrão para Health e MaxHealth
            Id = SafeParameterExtractor.GetInt32(parameters, offsets[0]);
            Health = SafeParameterExtractor.GetFloat(parameters, offsets[1], 100f); // Valor padrão se offset[1] existir
            MaxHealth = 100f; // Valor padrão fixo
            Energy = SafeParameterExtractor.GetFloat(parameters, offsets[1], 0f); // Usa offset[1] para Energy
        }

        public HealthUpdateEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            offsets = packetOffsets?.HealthUpdateEvent ?? new byte[] { 0, 3 };
            
            // ✅ SEGURO: Usar SafeParameterExtractor para evitar KeyNotFoundException
            // offsets.json tem apenas [0, 3], então usamos valores padrão para Health e MaxHealth
            Id = SafeParameterExtractor.GetInt32(parameters, offsets[0]);
            Health = SafeParameterExtractor.GetFloat(parameters, offsets[1], 100f); // Valor padrão se offset[1] existir
            MaxHealth = 100f; // Valor padrão fixo
            Energy = SafeParameterExtractor.GetFloat(parameters, offsets[1], 0f); // Usa offset[1] para Energy
        }

        public int Id { get; private set; }
        public float Health { get; private set; }
        public float MaxHealth { get; private set; }
        public float Energy { get; private set; }
    }
}
