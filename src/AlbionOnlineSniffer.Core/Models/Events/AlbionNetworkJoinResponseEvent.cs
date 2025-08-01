using System;
using System.Collections.Generic;
using Albion.Network;
using AlbionOnlineSniffer.Core.Models;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento JoinResponse compatível com Albion.Network.BaseResponse
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkJoinResponseEvent : BaseOperation
    {
        private readonly byte[] _offsets;

        public AlbionNetworkJoinResponseEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            _offsets = PacketOffsetsLoader.GlobalPacketOffsets?.JoinResponse ?? new byte[] { 0, 1 };
            
            if (_offsets.Length >= 2)
            {
                Success = Convert.ToBoolean(parameters[_offsets[0]]);
                Message = parameters[_offsets[1]]?.ToString() ?? string.Empty;
            }
            else
            {
                Success = false;
                Message = string.Empty;
            }
        }

        public bool Success { get; }
        public string Message { get; }
    }
} 