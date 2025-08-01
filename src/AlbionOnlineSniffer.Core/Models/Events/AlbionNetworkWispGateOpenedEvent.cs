using System;
using System.Collections.Generic;
using Albion.Network;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento WispGateOpened compatível com Albion.Network.BaseEvent
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkWispGateOpenedEvent : BaseEvent
    {
        private readonly byte[] _offsets;

        public AlbionNetworkWispGateOpenedEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            // TODO: Carregar offsets do PacketOffsets
            _offsets = new byte[] { 0, 1 }; // Placeholder
            
            Id = Convert.ToInt32(parameters[_offsets[0]]);
            IsOpened = Convert.ToBoolean(parameters[_offsets[1]]);
        }

        public int Id { get; }
        public bool IsOpened { get; }
    }
} 