using System.Numerics;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class NewHarvestableEvent
    {
        public string Id { get; set; }
        public int Type { get; set; }
        public int Tier { get; set; }
        public Vector2 Position { get; set; }
        public int Count { get; set; }
        public int Charge { get; set; }
    }
} 