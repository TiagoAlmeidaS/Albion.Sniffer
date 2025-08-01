using System;
using System.Collections.Generic;
using Albion.Network;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento KeySync compatível com Albion.Network.BaseEvent
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkKeySyncEvent : BaseEvent
    {
        private readonly byte[] _offsets;

        public AlbionNetworkKeySyncEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            _offsets = PacketOffsetsLoader.GlobalPacketOffsets?.KeySync ?? new byte[] { 0, 1 };
            
            Id = Convert.ToInt32(parameters[_offsets[0]]);
            Key = Convert.ToInt32(parameters[_offsets[1]]);
        }

        public int Id { get; }
        public int Key { get; }
    }
} 