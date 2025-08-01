using System;
using System.Collections.Generic;
using Albion.Network;
using AlbionOnlineSniffer.Core.Models.GameObjects;
using AlbionOnlineSniffer.Core.Models;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento HealthUpdate compatível com Albion.Network.BaseEvent
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkHealthUpdateEvent : BaseAlbionNetworkEvent
    {
        public AlbionNetworkHealthUpdateEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters, packetOffsets)
        {
            var offsets = GetOffsets("HealthUpdateEvent");
            
            if (offsets.Length >= 2)
            {
                Id = Convert.ToInt32(parameters[offsets[0]]);
                
                Health = parameters.ContainsKey(offsets[1]) ? 
                    new Health(Convert.ToInt32(parameters[offsets[1]]), Convert.ToInt32(parameters[offsets[2]])) 
                    : new Health(Convert.ToInt32(parameters[offsets[2]]));
            }
            else
            {
                Id = 0;
                Health = new Health(100);
            }
        }

        public int Id { get; }
        public Health Health { get; }
    }
} 