using System;
using System.Collections.Generic;
using System.Numerics;
using Albion.Network;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento HarvestableChangeState compatível com Albion.Network.BaseEvent
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkHarvestableChangeStateEvent : BaseEvent
    {
        private readonly byte[] _offsets;

        public AlbionNetworkHarvestableChangeStateEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            // TODO: Carregar offsets do PacketOffsets
            _offsets = new byte[] { 0, 1, 2, 3 }; // Placeholder
            
            Id = Convert.ToInt32(parameters[_offsets[0]]);
            Position = (Vector2)parameters[_offsets[1]];
            Count = Convert.ToInt32(parameters[_offsets[2]]);
            Charge = Convert.ToInt32(parameters[_offsets[3]]);
        }

        public int Id { get; }
        public Vector2 Position { get; }
        public int Count { get; }
        public int Charge { get; }
    }
} 