namespace AlbionOnlineSniffer.Core.Models {
    public class Harvestable {
        public int Id { get; set; }
        public string Type { get; set; }
        public int Tier { get; set; }
        public System.Numerics.Vector2 Position { get; set; }
        public int Count { get; set; }
        public int Charge { get; set; }
        public Harvestable(int id, string type, int tier, System.Numerics.Vector2 position, int count, int charge) {
            Id = id; Type = type; Tier = tier; Position = position; Count = count; Charge = charge;
        }
    }
} 