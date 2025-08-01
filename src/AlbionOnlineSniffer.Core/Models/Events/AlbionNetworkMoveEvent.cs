using System;
using System.Collections.Generic;
using System.Numerics;
using Albion.Network;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento Move compatível com Albion.Network.BaseEvent
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkMoveEvent : BaseEvent
    {
        private readonly byte[] _offsets;

        public AlbionNetworkMoveEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            _offsets = PacketOffsetsLoader.GlobalPacketOffsets?.Move ?? new byte[] { 0, 1, 2 };
            
            Id = Convert.ToInt32(parameters[_offsets[0]]);
            Position = (Vector2)parameters[_offsets[1]];
            Speed = parameters.ContainsKey(_offsets[2]) ? (float)parameters[_offsets[2]] : 0f;
        }

        public int Id { get; }
        public Vector2 Position { get; }
        public float Speed { get; }
    }
} 