using System;
using System.Collections.Generic;
using Albion.Network;
using AlbionOnlineSniffer.Core.Models;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Classe base para eventos do Albion.Network que precisam de offsets
    /// </summary>
    public abstract class BaseAlbionNetworkEvent : BaseEvent
    {
        protected readonly PacketOffsets _packetOffsets;

        protected BaseAlbionNetworkEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            _packetOffsets = packetOffsets ?? throw new ArgumentNullException(nameof(packetOffsets));
        }

        /// <summary>
        /// Obtém os offsets para um tipo específico de evento
        /// </summary>
        /// <param name="eventType">Tipo do evento</param>
        /// <returns>Array de offsets</returns>
        protected byte[] GetOffsets(string eventType)
        {
            var property = typeof(PacketOffsets).GetProperty(eventType);
            return property?.GetValue(_packetOffsets) as byte[] ?? new byte[0];
        }
    }
} 