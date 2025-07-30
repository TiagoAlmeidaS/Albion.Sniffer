using System;
using AlbionOnlineSniffer.Core.Models.GameObjects;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento de atualização de vida
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class HealthUpdateEvent : GameEvent
    {
        public HealthUpdateEvent(int id, Health health)
        {
            EventType = "HealthUpdate";
            Id = id;
            Health = health;
        }

        public int Id { get; set; }
        public Health Health { get; set; }
    }
} 