using Albion.Network;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Models.ResponseObj;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class HarvestableChangeStateEvent : BaseEvent
    {
        private readonly byte[] offsets;

        // Compat constructor for tests using provider-based offsets
        public HarvestableChangeStateEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            PacketOffsets? packetOffsets = null;
            try { packetOffsets = PacketOffsetsProvider.GetOffsets(); } catch { }
            packetOffsets ??= PacketOffsetsLoader.GlobalPacketOffsets;
            offsets = packetOffsets?.HarvestableChangeState ?? Array.Empty<byte>();

            if (offsets.Length < 1)
                throw new IndexOutOfRangeException("HarvestableChangeState offsets must contain at least the Id index");

            Id = Convert.ToInt32(parameters[offsets[0]]);

            int count = 0;
            if (offsets.Length > 1)
            {
                var idx = offsets[1];
                if (parameters.ContainsKey(idx))
                {
                    count = Convert.ToInt32(parameters[idx]);
                }
            }
            Count = count;

            int charge = 0;
            if (offsets.Length > 2)
            {
                var idx = offsets[2];
                if (parameters.ContainsKey(idx))
                {
                    charge = Convert.ToInt32(parameters[idx]);
                }
            }
            Charge = charge;
        }

        public HarvestableChangeStateEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            offsets = packetOffsets?.HarvestableChangeState ?? Array.Empty<byte>();

            if (offsets.Length < 1)
                throw new IndexOutOfRangeException("HarvestableChangeState offsets must contain at least the Id index");

            Id = Convert.ToInt32(parameters[offsets[0]]);

            int count = 0;
            if (offsets.Length > 1)
            {
                var idx = offsets[1];
                if (parameters.ContainsKey(idx))
                {
                    count = Convert.ToInt32(parameters[idx]);
                }
            }
            Count = count;

            int charge = 0;
            if (offsets.Length > 2)
            {
                var idx = offsets[2];
                if (parameters.ContainsKey(idx))
                {
                    charge = Convert.ToInt32(parameters[idx]);
                }
            }
            Charge = charge;
        }

        public int Id { get; }

        public int Count { get; }
        public int Charge { get; }
    }
}
