namespace AlbionOnlineSniffer.Core.Models {
    public class GatedWisp {
        public int Id { get; set; }
        public System.Numerics.Vector2 Position { get; set; }
        public GatedWisp(int id, System.Numerics.Vector2 position) {
            Id = id; Position = position;
        }
    }
} 