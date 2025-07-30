using System;
using System.Collections.Generic;
using System.Numerics;
using AlbionOnlineSniffer.Core.Models.GameObjects;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento de nova lista de harvestables
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class NewHarvestablesListEvent : GameEvent
    {
        public NewHarvestablesListEvent(List<Harvestable> harvestables)
        {
            EventType = "NewHarvestablesList";
            Harvestables = harvestables;
        }

        public List<Harvestable> Harvestables { get; set; }
    }
} 