using Albion.Network;
using AlbionOnlineSniffer.Core.Models.ResponseObj;
using AlbionOnlineSniffer.Core.Utility;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class StartFishingEvent : BaseEvent
    {
        private readonly byte[] offsets;

        public StartFishingEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            var packetOffsets = PacketOffsetsLoader.GlobalPacketOffsets ?? PacketOffsetsProvider.GetOffsets();
            offsets = packetOffsets?.StartFishing ?? new byte[] { 0, 1, 2, 3 };
            Initialize(parameters);
        }

        public StartFishingEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            offsets = packetOffsets?.StartFishing ?? new byte[] { 0, 1, 2, 3 };
            Initialize(parameters);
        }

        private void Initialize(Dictionary<byte, object> parameters)
        {
            RodId = SafeParameterExtractor.GetInt32(parameters, offsets[0]);
            BaitId = SafeParameterExtractor.GetInt32(parameters, offsets[1]);
            TargetX = SafeParameterExtractor.GetFloat(parameters, offsets[2]);
            TargetY = SafeParameterExtractor.GetFloat(parameters, offsets[3]);
        }

        public int RodId { get; private set; }
        public int BaitId { get; private set; }
        public float TargetX { get; private set; }
        public float TargetY { get; private set; }
    }
}


