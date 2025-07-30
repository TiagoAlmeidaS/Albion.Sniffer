using System;
using System.Numerics;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento de request de movimento
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class MoveRequestEvent : GameEvent
    {
        public MoveRequestEvent(int id, Vector2 position, bool isMoving)
        {
            EventType = "MoveRequest";
            Id = id;
            Position = position;
            IsMoving = isMoving;
        }

        public int Id { get; set; }
        public Vector2 Position { get; set; }
        public bool IsMoving { get; set; }
    }
} 