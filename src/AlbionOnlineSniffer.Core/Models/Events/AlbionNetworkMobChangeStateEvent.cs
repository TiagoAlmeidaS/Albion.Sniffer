using System;
using System.Collections.Generic;
using System.Numerics;
using Albion.Network;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento MobChangeState compatível com Albion.Network.BaseEvent
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkMobChangeStateEvent : BaseEvent
    {
        private readonly byte[] _offsets;

        public AlbionNetworkMobChangeStateEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            _offsets = PacketOffsetsLoader.GlobalPacketOffsets?.MobChangeState ?? new byte[] { 0, 1, 2 };
            
            Id = Convert.ToInt32(parameters[_offsets[0]]);
            Position = (Vector2)parameters[_offsets[1]];
            IsDead = Convert.ToBoolean(parameters[_offsets[2]]);
        }

        public int Id { get; }
        public Vector2 Position { get; }
        public bool IsDead { get; }
    }
} 