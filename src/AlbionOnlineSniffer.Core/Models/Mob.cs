namespace AlbionOnlineSniffer.Core.Models {
    public class Mob {
        public int Id { get; set; }
        public int TypeId { get; set; }
        public System.Numerics.Vector2 Position { get; set; }
        public byte EnchLvl { get; set; }
        public MobInfo Info { get; set; }
        public Health Health { get; set; }
        public float Speed { get; set; }
        public System.Numerics.Vector2 NewPosition { get; set; }
        public System.DateTime Time { get; set; }
        public int Charge { get; set; }
        public Mob(int id, int typeId, System.Numerics.Vector2 position, byte enchLvl, MobInfo info, Health health) {
            Id = id; TypeId = typeId; Position = position; EnchLvl = enchLvl; Info = info; Health = health;
        }
    }
} 