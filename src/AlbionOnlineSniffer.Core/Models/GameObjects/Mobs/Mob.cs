using System.Numerics;
using System.Reflection;
using AlbionOnlineSniffer.Core.Models.ResponseObj;

namespace AlbionOnlineSniffer.Core.Models.GameObjects.Mobs
{
    [Obfuscation(Feature = "mutation", Exclude = false)]
    public class Mob
    {
        public Mob(int id, int typeId, Vector2 position, byte charge, MobInfo mobInfo, Players.Health health)
        {
            Id = id;
            TypeId = typeId;
            Position = position;
            Charge = charge;
            MobInfo = mobInfo;
            Health = health;
        }

        public int Id { get; set; }
        public int TypeId { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 NewPosition { get; set; }
        public float Speed { get; set; }

        public DateTime Time { get; set; }

        public int Charge { get; set; }
        public MobInfo MobInfo { get; set; }

        public Players.Health Health { get; set; }
    }
}
