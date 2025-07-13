namespace AlbionOnlineSniffer.Core.Models {
    public class Dungeon {
        public int Id { get; set; }
        public string Type { get; set; }
        public System.Numerics.Vector2 Position { get; set; }
        public int Charges { get; set; }
        public Dungeon(int id, string type, System.Numerics.Vector2 position, int charges) {
            Id = id; Type = type; Position = position; Charges = charges;
        }
    }
} 