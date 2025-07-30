using System;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento de montaria
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class MountedEvent : GameEvent
    {
        public MountedEvent(int id, bool isMounted)
        {
            EventType = "Mounted";
            Id = id;
            IsMounted = isMounted;
        }

        public int Id { get; set; }
        public bool IsMounted { get; set; }
    }
} 