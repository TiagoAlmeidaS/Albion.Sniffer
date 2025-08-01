using System;
using System.Collections.Generic;
using System.Numerics;
using Albion.Network;
using AlbionOnlineSniffer.Core.Models;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento HarvestableChangeState compatível com Albion.Network.BaseEvent
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkHarvestableChangeStateEvent : BaseAlbionNetworkEvent
    {
        public AlbionNetworkHarvestableChangeStateEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters, packetOffsets)
        {
            var offsets = GetOffsets("HarvestableChangeState");
            
            if (offsets.Length >= 4)
            {
                Id = Convert.ToInt32(parameters[offsets[0]]);
                Position = (Vector2)parameters[offsets[1]];
                Count = Convert.ToInt32(parameters[offsets[2]]);
                Charge = Convert.ToInt32(parameters[offsets[3]]);
            }
            else
            {
                Id = 0;
                Position = Vector2.Zero;
                Count = 0;
                Charge = 0;
            }
        }

        public int Id { get; }
        public Vector2 Position { get; }
        public int Count { get; }
        public int Charge { get; }
    }
} 