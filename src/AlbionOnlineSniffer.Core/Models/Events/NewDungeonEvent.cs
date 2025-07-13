using System.Numerics;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class NewDungeonEvent
    {
        public string Id { get; set; }
        public int Type { get; set; }
        public Vector2 Position { get; set; }
        public int Charges { get; set; }
    }
} 