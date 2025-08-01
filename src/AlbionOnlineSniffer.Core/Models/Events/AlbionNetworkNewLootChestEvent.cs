using System;
using System.Collections.Generic;
using System.Numerics;
using Albion.Network;
using AlbionOnlineSniffer.Core.Models.GameObjects;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento NewLootChest compatível com Albion.Network.BaseEvent
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkNewLootChestEvent : BaseEvent
    {
        private readonly byte[] _offsets;

        public AlbionNetworkNewLootChestEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            _offsets = PacketOffsetsLoader.GlobalPacketOffsets?.NewLootChest ?? new byte[] { 0, 1, 2, 3, 4 };
            
            Id = Convert.ToInt32(parameters[_offsets[0]]);
            TypeId = Convert.ToInt32(parameters[_offsets[1]]);
            Position = (Vector2)parameters[_offsets[2]];

            Health = parameters.ContainsKey(_offsets[3]) ? 
                new Health(Convert.ToInt32(parameters[_offsets[3]]), Convert.ToInt32(parameters[_offsets[4]])) 
                : new Health(Convert.ToInt32(parameters[_offsets[4]]));
        }

        public int Id { get; }
        public int TypeId { get; }
        public Vector2 Position { get; }
        public Health Health { get; }
    }
} 