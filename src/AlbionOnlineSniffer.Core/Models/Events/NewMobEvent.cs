using System.Numerics;
using AlbionOnlineSniffer.Core.Models.GameObjects;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento específico para quando um novo mob é detectado
    /// </summary>
    public class NewMobEvent : GameEvent
    {
        public int MobId { get; set; }
        public int TypeId { get; set; }
        public Vector2 Position { get; set; }
        public Health Health { get; set; }
        public int Charge { get; set; }
        
        public NewMobEvent(Mob mob)
        {
            EventType = "NewMobEvent";
            MobId = mob.Id;
            TypeId = mob.TypeId;
            Position = mob.Position;
            Health = mob.Health;
            Charge = mob.Charge;
        }
    }
} 