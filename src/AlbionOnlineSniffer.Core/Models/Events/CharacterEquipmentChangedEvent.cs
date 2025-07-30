using System;
using System.Collections.Generic;
using AlbionOnlineSniffer.Core.Models.GameObjects;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento de mudança de equipamento do personagem
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class CharacterEquipmentChangedEvent : GameEvent
    {
        public CharacterEquipmentChangedEvent(int id, List<Equipment> equipments, List<string> spells)
        {
            EventType = "CharacterEquipmentChanged";
            Id = id;
            Equipments = equipments;
            Spells = spells;
        }

        public int Id { get; set; }
        public List<Equipment> Equipments { get; set; }
        public List<string> Spells { get; set; }
    }
} 