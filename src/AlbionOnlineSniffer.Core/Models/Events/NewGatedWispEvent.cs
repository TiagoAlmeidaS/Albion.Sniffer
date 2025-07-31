using System.Numerics;
using AlbionOnlineSniffer.Core.Models.GameObjects;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento específico para quando um novo gated wisp é detectado
    /// </summary>
    public class NewGatedWispEvent : GameEvent
    {
        public int WispId { get; set; }
        public Vector2 Position { get; set; }
        
        public NewGatedWispEvent(GatedWisp wisp)
        {
            EventType = "NewWispGate";
            WispId = wisp.Id;
            Position = wisp.Position;
        }
    }
} 