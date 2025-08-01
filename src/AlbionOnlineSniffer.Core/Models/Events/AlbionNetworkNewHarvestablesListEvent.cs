using System;
using System.Collections.Generic;
using Albion.Network;
using AlbionOnlineSniffer.Core.Models.GameObjects;
using AlbionOnlineSniffer.Core.Models;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento NewHarvestablesList compatível com Albion.Network.BaseEvent
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkNewHarvestablesListEvent : BaseAlbionNetworkEvent
    {
        public AlbionNetworkNewHarvestablesListEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters, packetOffsets)
        {
            var offsets = GetOffsets("NewHarvestableList");
            
            if (offsets.Length >= 1)
            {
                // TODO: Implementar parsing da lista de harvestables
                Harvestables = new List<Harvestable>();
            }
            else
            {
                Harvestables = new List<Harvestable>();
            }
        }

        public List<Harvestable> Harvestables { get; }
    }
} 