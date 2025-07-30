using System;
using System.Numerics;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento de mudança de estado de mob
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class MobChangeStateEvent : GameEvent
    {
        public MobChangeStateEvent(int id, Vector2 position, bool isDead)
        {
            EventType = "MobChangeState";
            Id = id;
            Position = position;
            IsDead = isDead;
        }

        public int Id { get; set; }
        public Vector2 Position { get; set; }
        public bool IsDead { get; set; }
    }
} 