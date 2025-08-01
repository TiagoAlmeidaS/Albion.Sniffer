using System;
using System.Collections.Generic;
using System.Numerics;
using Albion.Network;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento NewDungeonExit compatível com Albion.Network.BaseEvent
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkNewDungeonExitEvent : BaseEvent
    {
        private readonly byte[] _offsets;

        public AlbionNetworkNewDungeonExitEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            // TODO: Carregar offsets do PacketOffsets
            _offsets = new byte[] { 0, 1, 2 }; // Placeholder
            
            Id = Convert.ToInt32(parameters[_offsets[0]]);
            TypeId = Convert.ToInt32(parameters[_offsets[1]]);
            Position = (Vector2)parameters[_offsets[2]];
        }

        public int Id { get; }
        public int TypeId { get; }
        public Vector2 Position { get; }
    }
} 