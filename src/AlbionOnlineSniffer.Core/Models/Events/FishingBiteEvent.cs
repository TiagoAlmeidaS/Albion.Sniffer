using Albion.Network;
using AlbionOnlineSniffer.Core.Models.ResponseObj;
using AlbionOnlineSniffer.Core.Utility;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class FishingBiteEvent : BaseEvent
    {
        private readonly byte[] offsets;

        public FishingBiteEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            var packetOffsets = PacketOffsetsLoader.GlobalPacketOffsets ?? PacketOffsetsProvider.GetOffsets();
            offsets = packetOffsets?.FishingBiteEvent ?? new byte[] { 0, 1, 2 };
            Initialize(parameters);
        }

        public FishingBiteEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            offsets = packetOffsets?.FishingBiteEvent ?? new byte[] { 0, 1, 2 };
            Initialize(parameters);
        }

        private void Initialize(Dictionary<byte, object> parameters)
        {
            FishId = SafeParameterExtractor.GetValue<long>(parameters, offsets[0], 0L);
            Difficulty = SafeParameterExtractor.GetFloat(parameters, offsets[1]);
            BiteTime = SafeParameterExtractor.GetFloat(parameters, offsets[2]);
        }

        public long FishId { get; private set; }
        public float Difficulty { get; private set; }
        public float BiteTime { get; private set; }
    }
}


