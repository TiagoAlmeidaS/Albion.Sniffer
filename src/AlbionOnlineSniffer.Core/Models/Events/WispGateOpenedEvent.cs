using System;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento de wisp gate aberto
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class WispGateOpenedEvent : GameEvent
    {
        public WispGateOpenedEvent(int id, bool isCollected)
        {
            EventType = "WispGateOpened";
            Id = id;
            IsCollected = isCollected;
        }

        public int Id { get; set; }
        public bool IsCollected { get; set; }
    }
} 