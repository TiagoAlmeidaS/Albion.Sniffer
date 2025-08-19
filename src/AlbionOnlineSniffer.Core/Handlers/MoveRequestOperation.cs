using System.Numerics;
using Albion.Network;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Utility;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class MoveRequestOperation : BaseOperation
    {
        byte[] offsets = PacketOffsetsLoader.GlobalPacketOffsets.MoveRequest;
        
        public MoveRequestOperation(Dictionary<byte, object> parameters) : base(parameters)
        {
            var positionArray = parameters[offsets[0]] as float[];
            var newPositionArray = parameters[offsets[1]] as float[];
            
            Position = positionArray != null ? Additions.fromFArray(positionArray) : Vector2.Zero;
            NewPosition = newPositionArray != null ? Additions.fromFArray(newPositionArray) : Vector2.Zero;
            Speed = parameters.ContainsKey(offsets[2]) ? (float)parameters[offsets[2]] : 0f;
            Time = DateTime.UtcNow;
        }

        public Vector2 Position { get; }
        public Vector2 NewPosition { get; }
        public float Speed { get; }
        public DateTime Time { get; }
    }
}
