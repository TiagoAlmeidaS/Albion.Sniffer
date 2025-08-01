using System;
using System.Collections.Generic;
using Albion.Network;
using AlbionOnlineSniffer.Core.Models;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento LoadClusterObjects compatível com Albion.Network.BaseEvent
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkLoadClusterObjectsEvent : BaseAlbionNetworkEvent
    {
        public AlbionNetworkLoadClusterObjectsEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters, packetOffsets)
        {
            var offsets = GetOffsets("LoadClusterObjects");
            
            if (offsets.Length >= 2)
            {
                ClusterId = Convert.ToInt32(parameters[offsets[0]]);
                ObjectCount = Convert.ToInt32(parameters[offsets[1]]);
            }
            else
            {
                ClusterId = 0;
                ObjectCount = 0;
            }
        }

        public int ClusterId { get; }
        public int ObjectCount { get; }
    }
} 