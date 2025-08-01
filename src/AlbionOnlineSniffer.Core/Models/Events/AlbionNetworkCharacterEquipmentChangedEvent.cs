using System;
using System.Collections.Generic;
using Albion.Network;
using AlbionOnlineSniffer.Core.Models;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento CharacterEquipmentChanged compatível com Albion.Network.BaseEvent
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkCharacterEquipmentChangedEvent : BaseAlbionNetworkEvent
    {
        public AlbionNetworkCharacterEquipmentChangedEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters, packetOffsets)
        {
            var offsets = GetOffsets("CharacterEquipmentChanged");
            
            if (offsets.Length >= 2)
            {
                Id = Convert.ToInt32(parameters[offsets[0]]);
                EquipmentId = Convert.ToInt32(parameters[offsets[1]]);
            }
            else
            {
                Id = 0;
                EquipmentId = 0;
            }
        }

        public int Id { get; }
        public int EquipmentId { get; }
    }
} 