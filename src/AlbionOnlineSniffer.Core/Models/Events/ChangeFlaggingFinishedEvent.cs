using System;
using AlbionOnlineSniffer.Core.Models.GameObjects;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento de mudança de flagging finalizada
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class ChangeFlaggingFinishedEvent : GameEvent
    {
        public ChangeFlaggingFinishedEvent(int id, Faction faction)
        {
            EventType = "ChangeFlaggingFinished";
            Id = id;
            Faction = faction;
        }

        public int Id { get; set; }
        public Faction Faction { get; set; }
    }
} 