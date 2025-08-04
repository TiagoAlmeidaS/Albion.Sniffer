using System.Numerics;
using Albion.Network;
using Albion.Sniffer.Core.Services;
using Albion.Sniffer.Core.Utility;

namespace Albion.Sniffer.Core.Handlers
{
    public class MoveRequestOperation : BaseOperation
    {
        byte[] offsets = PacketOffsetsLoader.GlobalPacketOffsets.MoveRequest;
        
        public MoveRequestOperation(Dictionary<byte, object> parameters) : base(parameters)
        {
            Position = Additions.fromFArray((float[])parameters[offsets[0]]);
            NewPosition = Additions.fromFArray((float[])parameters[offsets[1]]);
            Speed = parameters.ContainsKey(offsets[2]) ? (float)parameters[offsets[2]] : 0f;
            Time = DateTime.UtcNow;
        }

        public Vector2 Position { get; }
        public Vector2 NewPosition { get; }
        public float Speed { get; }
        public DateTime Time { get; }
    }
}
