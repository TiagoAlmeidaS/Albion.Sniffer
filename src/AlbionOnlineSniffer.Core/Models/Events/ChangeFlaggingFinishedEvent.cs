using Albion.Network;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Models.ResponseObj;
using AlbionOnlineSniffer.Core.Utility;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class ChangeFlaggingFinishedEvent : BaseEvent
    {
        private readonly byte[] offsets;

        public ChangeFlaggingFinishedEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            offsets = packetOffsets?.ChangeFlaggingFinished ?? new byte[] { 0 };
            
            Id = Convert.ToInt32(parameters[offsets[0]]);
            
            // Alguns pacotes podem incluir a facção no próximo índice
            if (offsets.Length > 1 && parameters.ContainsKey(offsets[1]))
            {
                Faction = (Faction)Convert.ToByte(parameters[offsets[1]]);
            }
            else
            {
                Faction = Faction.NoPVP;
            }
        }
        
        // Construtor para compatibilidade com framework Albion.Network
        public ChangeFlaggingFinishedEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            var packetOffsets = PacketOffsetsProvider.GetOffsets();
            offsets = packetOffsets?.ChangeFlaggingFinished ?? new byte[] { 0 };
            Id = Convert.ToInt32(parameters[offsets[0]]);
            if (offsets.Length > 1 && parameters.ContainsKey(offsets[1]))
            {
                Faction = (Faction)Convert.ToByte(parameters[offsets[1]]);
            }
            else
            {
                Faction = Faction.NoPVP;
            }
        }
        
        public int Id { get; }
        public Faction Faction { get; }
    }
}
