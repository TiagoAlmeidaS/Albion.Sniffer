using System;
using System.Collections.Generic;
using Albion.Network;
using AlbionOnlineSniffer.Core.Models;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento ChangeCluster compatível com Albion.Network.BaseOperation
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkChangeClusterEvent : BaseOperation
    {
        private readonly byte[] _offsets;

        public AlbionNetworkChangeClusterEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            _offsets = PacketOffsetsLoader.GlobalPacketOffsets?.ChangeCluster ?? new byte[] { 0, 1 };
            
            if (_offsets.Length >= 2 && parameters.ContainsKey(_offsets[0]) && parameters.ContainsKey(_offsets[1]))
            {
                ClusterId = Convert.ToInt32(parameters[_offsets[0]]);
                Success = Convert.ToBoolean(parameters[_offsets[1]]);
            }
            else
            {
                ClusterId = 0;
                Success = false;
            }
        }

        public int ClusterId { get; }
        public bool Success { get; }
    }
} 