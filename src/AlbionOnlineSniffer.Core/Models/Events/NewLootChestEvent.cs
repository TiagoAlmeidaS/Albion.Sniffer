using System.Numerics;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class NewLootChestEvent
    {
        public string Id { get; set; }
        public Vector2 Position { get; set; }
        public string Name { get; set; }
        public int EnchLvl { get; set; }
    }
} 