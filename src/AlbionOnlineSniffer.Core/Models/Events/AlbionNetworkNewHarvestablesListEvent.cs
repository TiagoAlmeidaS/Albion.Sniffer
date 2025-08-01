using System;
using System.Collections.Generic;
using Albion.Network;
using AlbionOnlineSniffer.Core.Models.GameObjects;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento NewHarvestablesList compatível com Albion.Network.BaseEvent
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkNewHarvestablesListEvent : BaseEvent
    {
        private readonly byte[] _offsets;

        public AlbionNetworkNewHarvestablesListEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            // TODO: Carregar offsets do PacketOffsets
            _offsets = new byte[] { 0 }; // Placeholder
            
            // TODO: Implementar parsing da lista de harvestables
            Harvestables = new List<Harvestable>();
        }

        public List<Harvestable> Harvestables { get; }
    }
} 