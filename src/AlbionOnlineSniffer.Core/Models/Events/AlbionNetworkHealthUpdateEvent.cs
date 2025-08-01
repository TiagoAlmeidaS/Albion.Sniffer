using System;
using System.Collections.Generic;
using Albion.Network;
using AlbionOnlineSniffer.Core.Models.GameObjects;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento HealthUpdate compatível com Albion.Network.BaseEvent
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkHealthUpdateEvent : BaseEvent
    {
        private readonly byte[] _offsets;

        public AlbionNetworkHealthUpdateEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            _offsets = PacketOffsetsLoader.GlobalPacketOffsets?.HealthUpdateEvent ?? new byte[] { 0, 1, 2 };
            
            Id = Convert.ToInt32(parameters[_offsets[0]]);
            
            Health = parameters.ContainsKey(_offsets[1]) ? 
                new Health(Convert.ToInt32(parameters[_offsets[1]]), Convert.ToInt32(parameters[_offsets[2]])) 
                : new Health(Convert.ToInt32(parameters[_offsets[2]]));
        }

        public int Id { get; }
        public Health Health { get; }
    }
} 