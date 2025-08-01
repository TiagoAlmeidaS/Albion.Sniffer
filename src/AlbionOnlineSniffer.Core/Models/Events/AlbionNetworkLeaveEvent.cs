using System;
using System.Collections.Generic;
using Albion.Network;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento Leave compatível com Albion.Network.BaseEvent
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkLeaveEvent : BaseEvent
    {
        private readonly byte[] _offsets;

        public AlbionNetworkLeaveEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            _offsets = PacketOffsetsLoader.GlobalPacketOffsets?.Leave ?? new byte[] { 0, 1 };
            
            Id = Convert.ToInt32(parameters[_offsets[0]]);
            EntityType = parameters[_offsets[1]] as string ?? "unknown";
        }

        public int Id { get; }
        public string EntityType { get; }
    }
} 