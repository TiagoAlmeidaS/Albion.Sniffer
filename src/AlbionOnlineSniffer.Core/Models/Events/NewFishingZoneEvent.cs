using System.Numerics;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class NewFishingZoneEvent
    {
        public string Id { get; set; }
        public Vector2 Position { get; set; }
        public int Size { get; set; }
        public int RespawnCount { get; set; }
    }
} 