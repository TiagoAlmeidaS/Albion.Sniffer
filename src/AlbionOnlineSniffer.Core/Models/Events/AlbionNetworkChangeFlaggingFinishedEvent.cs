using System;
using System.Collections.Generic;
using Albion.Network;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento ChangeFlaggingFinished compatível com Albion.Network.BaseEvent
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkChangeFlaggingFinishedEvent : BaseEvent
    {
        private readonly byte[] _offsets;

        public AlbionNetworkChangeFlaggingFinishedEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            // TODO: Carregar offsets do PacketOffsets
            _offsets = new byte[] { 0, 1 }; // Placeholder
            
            PlayerId = Convert.ToInt32(parameters[_offsets[0]]);
            IsFlagged = Convert.ToBoolean(parameters[_offsets[1]]);
        }

        public int PlayerId { get; }
        public bool IsFlagged { get; }
    }
} 