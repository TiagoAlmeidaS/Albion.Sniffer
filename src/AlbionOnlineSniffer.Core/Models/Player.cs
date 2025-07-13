namespace AlbionOnlineSniffer.Core.Models {
    public class Player {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Guild { get; set; }
        public string Alliance { get; set; }
        public System.Numerics.Vector2 Position { get; set; }
        public Health Health { get; set; }
        public Faction Faction { get; set; }
        public Equipment Equipment { get; set; }
        public int[] Spells { get; set; }
        public bool IsMounted { get; set; }
        public bool IsStanding { get; set; }
        public float Speed { get; set; }
        public System.Numerics.Vector2 NewPosition { get; set; }
        public System.DateTime Time { get; set; }
        public Player(int id, string name, string guild, string alliance, System.Numerics.Vector2 position, Health health, Faction faction, Equipment equipment, int[] spells) {
            Id = id; Name = name; Guild = guild; Alliance = alliance; Position = position; Health = health; Faction = faction; Equipment = equipment; Spells = spells;
        }
    }
} 