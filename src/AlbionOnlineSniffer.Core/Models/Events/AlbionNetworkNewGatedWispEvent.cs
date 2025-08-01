using System;
using System.Collections.Generic;
using System.Numerics;
using Albion.Network;
using AlbionOnlineSniffer.Core.Models;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento NewGatedWisp compatível com Albion.Network.BaseEvent
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkNewGatedWispEvent : BaseAlbionNetworkEvent
    {
        public AlbionNetworkNewGatedWispEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters, packetOffsets)
        {
            var offsets = GetOffsets("NewWispGate");
            
            if (offsets.Length >= 3)
            {
                Id = Convert.ToInt32(parameters[offsets[0]]);
                TypeId = Convert.ToInt32(parameters[offsets[1]]);
                Position = (Vector2)parameters[offsets[2]];
                Tier = (byte)(parameters.ContainsKey(offsets[3]) ? Convert.ToInt32(parameters[offsets[3]]) : 0);
            }
            else
            {
                Id = 0;
                TypeId = 0;
                Position = Vector2.Zero;
                Tier = 0;
            }
        }

        public int Id { get; }
        public int TypeId { get; }
        public Vector2 Position { get; }
        public byte Tier { get; }
    }
} 