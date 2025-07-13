using System.Numerics;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class NewGatedWispEvent
    {
        public string Id { get; set; }
        public Vector2 Position { get; set; }
        public bool IsCollected { get; set; }
    }
} 