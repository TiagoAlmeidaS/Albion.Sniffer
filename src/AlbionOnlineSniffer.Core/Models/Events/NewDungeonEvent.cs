using System.Numerics;
using Albion.Network;
using AlbionOnlineSniffer.Core.Utility;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Models.ResponseObj;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class NewDungeonEvent : BaseEvent
    {
        private readonly byte[] offsets;

        public NewDungeonEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            offsets = packetOffsets?.NewDungeonExit;
            
            Id = Convert.ToInt32(parameters[offsets[0]]);

            if (parameters.ContainsKey(offsets[1]) && parameters[offsets[1]] is byte[] positionBytes)
            {
                PositionBytes = positionBytes;
            }

            Type = parameters.ContainsKey(offsets[2]) ? parameters[offsets[2]] as string : "NULL";

            Charges = Convert.ToInt32(parameters[offsets[3]]);
        }

        public int Id { get; }

        public Vector2 Position { get; }

        public string Type { get; }

        public int Charges { get; }
    }
}
