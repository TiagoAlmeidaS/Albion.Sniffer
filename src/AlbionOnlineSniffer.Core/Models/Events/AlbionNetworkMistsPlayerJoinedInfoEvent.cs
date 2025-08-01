using System;
using System.Collections.Generic;
using Albion.Network;
using AlbionOnlineSniffer.Core.Models;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento MistsPlayerJoinedInfo compatível com Albion.Network.BaseEvent
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkMistsPlayerJoinedInfoEvent : BaseAlbionNetworkEvent
    {
        public AlbionNetworkMistsPlayerJoinedInfoEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters, packetOffsets)
        {
            var offsets = GetOffsets("MistsPlayerJoinedInfo");
            
            if (offsets.Length >= 3)
            {
                PlayerId = Convert.ToInt32(parameters[offsets[0]]);
                GuildId = Convert.ToInt32(parameters[offsets[1]]);
                AllianceId = Convert.ToInt32(parameters[offsets[2]]);
            }
            else
            {
                PlayerId = 0;
                GuildId = 0;
                AllianceId = 0;
            }
        }

        public int PlayerId { get; }
        public int GuildId { get; }
        public int AllianceId { get; }
    }
} 