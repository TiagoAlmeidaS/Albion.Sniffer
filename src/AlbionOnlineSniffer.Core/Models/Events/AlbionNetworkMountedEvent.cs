using System;
using System.Collections.Generic;
using Albion.Network;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento Mounted compatível com Albion.Network.BaseEvent
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkMountedEvent : BaseEvent
    {
        private readonly byte[] _offsets;

        public AlbionNetworkMountedEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            _offsets = PacketOffsetsLoader.GlobalPacketOffsets?.Mounted ?? new byte[] { 0, 1 };
            
            Id = Convert.ToInt32(parameters[_offsets[0]]);
            IsMounted = Convert.ToBoolean(parameters[_offsets[1]]);
        }

        public int Id { get; }
        public bool IsMounted { get; }
    }
} 