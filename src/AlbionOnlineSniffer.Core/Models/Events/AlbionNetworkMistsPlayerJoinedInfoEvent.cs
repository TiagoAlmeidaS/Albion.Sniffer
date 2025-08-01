using System;
using System.Collections.Generic;
using Albion.Network;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento MistsPlayerJoinedInfo compatível com Albion.Network.BaseEvent
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkMistsPlayerJoinedInfoEvent : BaseEvent
    {
        private readonly byte[] _offsets;

        public AlbionNetworkMistsPlayerJoinedInfoEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            // TODO: Carregar offsets do PacketOffsets
            _offsets = new byte[] { 0, 1, 2 }; // Placeholder
            
            PlayerId = Convert.ToInt32(parameters[_offsets[0]]);
            GuildId = Convert.ToInt32(parameters[_offsets[1]]);
            AllianceId = Convert.ToInt32(parameters[_offsets[2]]);
        }

        public int PlayerId { get; }
        public int GuildId { get; }
        public int AllianceId { get; }
    }
} 