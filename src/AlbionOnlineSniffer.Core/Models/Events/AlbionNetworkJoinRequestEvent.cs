using System;
using System.Collections.Generic;
using Albion.Network;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento JoinRequest compatível com Albion.Network.BaseOperation
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkJoinRequestEvent : BaseOperation
    {
        private readonly byte[] _offsets;

        public AlbionNetworkJoinRequestEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            // TODO: Carregar offsets do PacketOffsets
            _offsets = new byte[] { 0, 1 }; // Placeholder
            
            PlayerId = Convert.ToInt32(parameters[_offsets[0]]);
            GuildId = Convert.ToInt32(parameters[_offsets[1]]);
        }

        public int PlayerId { get; }
        public int GuildId { get; }
    }
} 