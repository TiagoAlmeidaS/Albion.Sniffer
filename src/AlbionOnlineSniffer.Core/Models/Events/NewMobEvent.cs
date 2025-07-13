using System.Numerics;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class NewMobEvent
    {
        public string Id { get; set; }
        public int TypeId { get; set; }
        public Vector2 Position { get; set; }
        public int Health { get; set; }
        public int Charge { get; set; }
    }
} 