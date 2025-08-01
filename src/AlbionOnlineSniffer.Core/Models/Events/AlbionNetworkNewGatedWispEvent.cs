using System;
using System.Collections.Generic;
using System.Numerics;
using Albion.Network;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento NewGatedWisp compatível com Albion.Network.BaseEvent
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkNewGatedWispEvent : BaseEvent
    {
        private readonly byte[] _offsets;

        public AlbionNetworkNewGatedWispEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            // TODO: Carregar offsets do PacketOffsets
            _offsets = new byte[] { 0, 1, 2, 3 }; // Placeholder
            
            Id = Convert.ToInt32(parameters[_offsets[0]]);
            TypeId = Convert.ToInt32(parameters[_offsets[1]]);
            Position = (Vector2)parameters[_offsets[2]];
            Tier = (byte)(parameters.ContainsKey(_offsets[3]) ? Convert.ToInt32(parameters[_offsets[3]]) : 0);
        }

        public int Id { get; }
        public int TypeId { get; }
        public Vector2 Position { get; }
        public byte Tier { get; }
    }
} 