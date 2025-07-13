namespace AlbionOnlineSniffer.Core.Models {
    public class FishNode {
        public int Id { get; set; }
        public System.Numerics.Vector2 Position { get; set; }
        public int Size { get; set; }
        public int RespawnCount { get; set; }
        public FishNode(int id, System.Numerics.Vector2 position, int size, int respawnCount) {
            Id = id; Position = position; Size = size; RespawnCount = respawnCount;
        }
    }
} 