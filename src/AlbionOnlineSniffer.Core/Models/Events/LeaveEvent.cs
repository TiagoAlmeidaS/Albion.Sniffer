using System;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento de saída de entidade
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class LeaveEvent : GameEvent
    {
        public int Id { get; set; }
        public string EntityType { get; set; } = string.Empty;

        public LeaveEvent(int id, string entityType)
        {
            EventType = "Leave";
            Id = id;
            EntityType = entityType;
        }
    }
} 