using System;
using System.Collections.Generic;
using Albion.Network;
using AlbionOnlineSniffer.Core.Models;
using AlbionOnlineSniffer.Core.Services;

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
            _offsets = PacketOffsetsLoader.GlobalPacketOffsets?.JoinResponse ?? new byte[] { 0, 1 };
            
            if (_offsets.Length >= 2)
            {
                PlayerId = Convert.ToInt32(parameters[_offsets[0]]);
                GuildId = Convert.ToInt32(parameters[_offsets[1]]);
            }
            else
            {
                PlayerId = 0;
                GuildId = 0;
            }
        }

        public int PlayerId { get; }
        public int GuildId { get; }
    }
} 