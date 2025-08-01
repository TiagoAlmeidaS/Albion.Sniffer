using System;
using System.Collections.Generic;
using System.Numerics;
using Albion.Network;
using AlbionOnlineSniffer.Core.Models;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento NewFishingZone compatível com Albion.Network.BaseEvent
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkNewFishingZoneEvent : BaseAlbionNetworkEvent
    {
        public AlbionNetworkNewFishingZoneEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters, packetOffsets)
        {
            var offsets = GetOffsets("NewFishingZoneObject");
            
            if (offsets.Length >= 3)
            {
                Id = Convert.ToInt32(parameters[offsets[0]]);
                TypeId = Convert.ToInt32(parameters[offsets[1]]);
                Position = (Vector2)parameters[offsets[2]];
            }
            else
            {
                Id = 0;
                TypeId = 0;
                Position = Vector2.Zero;
            }
        }

        public int Id { get; }
        public int TypeId { get; }
        public Vector2 Position { get; }
    }
} 