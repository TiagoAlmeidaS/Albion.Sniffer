using Albion.Network;
using AlbionOnlineSniffer.Core.Models.ResponseObj;
using AlbionOnlineSniffer.Core.Utility;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class FishingMiniGameUpdateEvent : BaseEvent
    {
        private readonly byte[] offsets;

        public FishingMiniGameUpdateEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            var packetOffsets = PacketOffsetsLoader.GlobalPacketOffsets ?? PacketOffsetsProvider.GetOffsets();
            offsets = packetOffsets?.FishingMiniGameUpdate ?? new byte[] { 0, 1, 2 };
            Initialize(parameters);
        }

        public FishingMiniGameUpdateEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            offsets = packetOffsets?.FishingMiniGameUpdate ?? new byte[] { 0, 1, 2 };
            Initialize(parameters);
        }

        private void Initialize(Dictionary<byte, object> parameters)
        {
            BobPosition = SafeParameterExtractor.GetFloat(parameters, offsets[0]);
            BarSpeed = SafeParameterExtractor.GetFloat(parameters, offsets[1]);
            Direction = SafeParameterExtractor.GetInt32(parameters, offsets[2]);
        }

        public float BobPosition { get; private set; }
        public float BarSpeed { get; private set; }
        public int Direction { get; private set; }
    }
}


