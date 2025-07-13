namespace AlbionOnlineSniffer.Core.Models {
    public class LootChest {
        public int Id { get; set; }
        public System.Numerics.Vector2 Position { get; set; }
        public string Name { get; set; }
        public int Charge { get; set; }
        public LootChest(int id, System.Numerics.Vector2 position, string name, int charge) {
            Id = id; Position = position; Name = name; Charge = charge;
        }
    }
} 