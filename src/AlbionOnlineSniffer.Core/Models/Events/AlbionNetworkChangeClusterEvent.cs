using System;
using System.Collections.Generic;
using Albion.Network;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento ChangeCluster compatível com Albion.Network.BaseResponse
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkChangeClusterEvent : BaseOperation
    {
        private readonly byte[] _offsets;

        public AlbionNetworkChangeClusterEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            // TODO: Carregar offsets do PacketOffsets
            _offsets = new byte[] { 0, 1 }; // Placeholder
            
            ClusterId = Convert.ToInt32(parameters[_offsets[0]]);
            Success = Convert.ToBoolean(parameters[_offsets[1]]);
        }

        public int ClusterId { get; }
        public bool Success { get; }
    }
} 