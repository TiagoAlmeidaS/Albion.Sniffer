using System;
using System.Collections.Generic;
using System.Numerics;
using Albion.Network;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento NewDungeon compatível com Albion.Network.BaseEvent
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkNewDungeonEvent : BaseEvent
    {
        private readonly byte[] _offsets;

        public AlbionNetworkNewDungeonEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            _offsets = PacketOffsetsLoader.GlobalPacketOffsets?.NewDungeon ?? new byte[] { 0, 1, 2, 3 };
            
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