using System;
using System.Collections.Generic;
using Albion.Network;
using AlbionOnlineSniffer.Core.Models;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento WispGateOpened compatível com Albion.Network.BaseEvent
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkWispGateOpenedEvent : BaseAlbionNetworkEvent
    {
        public AlbionNetworkWispGateOpenedEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters, packetOffsets)
        {
            var offsets = GetOffsets("WispGateOpened");
            
            if (offsets.Length >= 2)
            {
                Id = Convert.ToInt32(parameters[offsets[0]]);
                IsOpened = Convert.ToBoolean(parameters[offsets[1]]);
            }
            else
            {
                Id = 0;
                IsOpened = false;
            }
        }

        public int Id { get; }
        public bool IsOpened { get; }
    }
} 