using System.Numerics;
using Albion.Network;
using AlbionOnlineSniffer.Core.Utility;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Models.ResponseObj;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class NewGatedWispEvent : BaseEvent
    {
        private readonly byte[] offsets;

        public NewGatedWispEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            offsets = packetOffsets?.NewWispGate;
            
            Id = Convert.ToInt32(parameters[offsets[0]]);

            if (parameters.ContainsKey(offsets[1]) && parameters[offsets[1]] is byte[] positionBytes)
            {
                PositionBytes = positionBytes;
            }
        }

        public int Id { get; }

        public Vector2 Position { get; }

        public bool isCollected { get; }
    }
}
