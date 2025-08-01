using System;
using System.Collections.Generic;
using Albion.Network;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento LoadClusterObjects compatível com Albion.Network.BaseEvent
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkLoadClusterObjectsEvent : BaseEvent
    {
        private readonly byte[] _offsets;

        public AlbionNetworkLoadClusterObjectsEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            // TODO: Carregar offsets do PacketOffsets
            _offsets = new byte[] { 0, 1 }; // Placeholder
            
            ClusterId = Convert.ToInt32(parameters[_offsets[0]]);
            ObjectCount = Convert.ToInt32(parameters[_offsets[1]]);
        }

        public int ClusterId { get; }
        public int ObjectCount { get; }
    }
} 