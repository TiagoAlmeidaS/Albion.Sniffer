using System;
using System.Collections.Generic;
using Albion.Network;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento RegenerationChanged compatível com Albion.Network.BaseEvent
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkRegenerationChangedEvent : BaseEvent
    {
        private readonly byte[] _offsets;

        public AlbionNetworkRegenerationChangedEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            // TODO: Carregar offsets do PacketOffsets
            _offsets = new byte[] { 0, 1 }; // Placeholder
            
            Id = Convert.ToInt32(parameters[_offsets[0]]);
            Regeneration = Convert.ToBoolean(parameters[_offsets[1]]);
        }

        public int Id { get; }
        public bool Regeneration { get; }
    }
} 