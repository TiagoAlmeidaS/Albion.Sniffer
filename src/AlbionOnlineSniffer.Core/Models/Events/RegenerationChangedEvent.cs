using System;
using AlbionOnlineSniffer.Core.Models.GameObjects;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento de mudança na regeneração de vida
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class RegenerationChangedEvent : GameEvent
    {
        public RegenerationChangedEvent(int id, Health health)
        {
            EventType = "RegenerationChanged";
            Id = id;
            Health = health;
        }

        public int Id { get; set; }
        public Health Health { get; set; }
    }
} 