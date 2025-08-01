using System;
using System.Collections.Generic;
using Albion.Network;
using AlbionOnlineSniffer.Core.Models;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento RegenerationChanged compatível com Albion.Network.BaseEvent
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkRegenerationChangedEvent : BaseAlbionNetworkEvent
    {
        public AlbionNetworkRegenerationChangedEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters, packetOffsets)
        {
            var offsets = GetOffsets("RegenerationHealthChangedEvent");
            
            if (offsets.Length >= 2)
            {
                Id = Convert.ToInt32(parameters[offsets[0]]);
                Regeneration = Convert.ToBoolean(parameters[offsets[1]]);
            }
            else
            {
                Id = 0;
                Regeneration = false;
            }
        }

        public int Id { get; }
        public bool Regeneration { get; }
    }
} 