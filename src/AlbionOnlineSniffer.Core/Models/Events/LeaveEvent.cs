using Albion.Network;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Models.ResponseObj;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class LeaveEvent : BaseEvent
    {
        private readonly byte[] offsets;

        public LeaveEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            offsets = packetOffsets?.Leave;
            if (offsets == null || offsets.Length == 0)
            {
                // Para cenários com PacketOffsets passado explicitamente como null/sem dados
                throw new NullReferenceException("Offsets para LeaveEvent não configurados");
            }
            
            Id = Convert.ToInt32(parameters[offsets[0]]);
        }

        // Construtor para compatibilidade com framework Albion.Network
        public LeaveEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            var packetOffsets = PacketOffsetsProvider.GetOffsets();
            offsets = packetOffsets?.Leave;
            if (offsets == null || offsets.Length == 0)
            {
                // Provider existe, mas offsets não configurados
                throw new NullReferenceException("Offsets para LeaveEvent não configurados");
            }
            Id = Convert.ToInt32(parameters[offsets[0]]);
        }

        public int Id { get; }
    }
}
