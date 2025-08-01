using System;
using System.Collections.Generic;
using Albion.Network;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento CharacterEquipmentChanged compatível com Albion.Network.BaseEvent
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkCharacterEquipmentChangedEvent : BaseEvent
    {
        private readonly byte[] _offsets;

        public AlbionNetworkCharacterEquipmentChangedEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            // TODO: Carregar offsets do PacketOffsets
            _offsets = new byte[] { 0, 1 }; // Placeholder
            
            Id = Convert.ToInt32(parameters[_offsets[0]]);
            EquipmentId = Convert.ToInt32(parameters[_offsets[1]]);
        }

        public int Id { get; }
        public int EquipmentId { get; }
    }
} 