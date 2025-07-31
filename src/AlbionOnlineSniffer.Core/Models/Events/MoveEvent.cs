using System.Numerics;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento específico para quando um jogador se move
    /// </summary>
    public class MoveEvent : GameEvent
    {
        public int PlayerId { get; set; }
        public Vector2 Position { get; set; }
        public float Speed { get; set; }
        
        public MoveEvent(int playerId, Vector2 position, float speed)
        {
            EventType = "Move";
            PlayerId = playerId;
            Position = position;
            Speed = speed;
        }
    }
} 