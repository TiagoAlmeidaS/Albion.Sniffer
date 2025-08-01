using System;
using System.Collections.Generic;
using Albion.Network;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento LeaveResponse compatível com Albion.Network.BaseOperation
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkLeaveResponseEvent : BaseOperation
    {
        private readonly byte[] _offsets;

        public AlbionNetworkLeaveResponseEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            // TODO: Carregar offsets do PacketOffsets
            _offsets = new byte[] { 0, 1 }; // Placeholder
            
            Success = Convert.ToBoolean(parameters[_offsets[0]]);
            Message = parameters[_offsets[1]]?.ToString() ?? string.Empty;
        }

        public bool Success { get; }
        public string Message { get; }
    }
} 