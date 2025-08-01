using System;
using System.Collections.Generic;
using System.Numerics;
using Albion.Network;
using AlbionOnlineSniffer.Core.Models;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento Move compatível com Albion.Network.BaseEvent
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkMoveEvent : BaseAlbionNetworkEvent
    {
        public AlbionNetworkMoveEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters, packetOffsets)
        {
            var offsets = GetOffsets("Move");
            
            if (offsets.Length >= 2)
            {
                Id = Convert.ToInt32(parameters[offsets[0]]);
                Position = (Vector2)parameters[offsets[1]];
                Speed = parameters.ContainsKey(offsets[2]) ? (float)parameters[offsets[2]] : 0f;
            }
            else
            {
                Id = 0;
                Position = Vector2.Zero;
                Speed = 0f;
            }
        }

        public int Id { get; }
        public Vector2 Position { get; }
        public float Speed { get; }
    }
} 