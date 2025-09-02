using Albion.Network;
using AlbionOnlineSniffer.Core.Models.ResponseObj;
using AlbionOnlineSniffer.Core.Utility;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class FishingFinishEvent : BaseEvent
    {
        private readonly byte[] offsets;

        public FishingFinishEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            var packetOffsets = PacketOffsetsLoader.GlobalPacketOffsets ?? PacketOffsetsProvider.GetOffsets();
            offsets = packetOffsets?.FishingFinish ?? new byte[] { 0, 1, 2, 3 };
            Initialize(parameters);
        }

        public FishingFinishEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            offsets = packetOffsets?.FishingFinish ?? new byte[] { 0, 1, 2, 3 };
            Initialize(parameters);
        }

        private void Initialize(Dictionary<byte, object> parameters)
        {
            Result = SafeParameterExtractor.GetInt32(parameters, offsets[0]);
            ItemId = SafeParameterExtractor.GetValue<long>(parameters, offsets[1], 0L);
            Quantity = SafeParameterExtractor.GetInt32(parameters, offsets[2]);
            Rarity = SafeParameterExtractor.GetInt32(parameters, offsets[3]);
        }

        public int Result { get; private set; }
        public long ItemId { get; private set; }
        public int Quantity { get; private set; }
        public int Rarity { get; private set; }
    }
}


