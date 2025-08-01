using System;
using System.Collections.Generic;
using System.Numerics;
using Albion.Network;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento MoveRequest compatível com Albion.Network.BaseRequest
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkMoveRequestEvent : BaseOperation
    {
        private readonly byte[] _offsets;

        public AlbionNetworkMoveRequestEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            _offsets = PacketOffsetsLoader.GlobalPacketOffsets?.MoveRequest ?? new byte[] { 0, 1 };
            
            PlayerId = Convert.ToInt32(parameters[_offsets[0]]);
            Position = (Vector2)parameters[_offsets[1]];
        }

        public int PlayerId { get; }
        public Vector2 Position { get; }
    }
} 