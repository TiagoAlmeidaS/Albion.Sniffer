using System.Numerics;
using AlbionOnlineSniffer.Core.Models.GameObjects;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento específico para quando uma nova zona de pesca é detectada
    /// </summary>
    public class NewFishingZoneEvent : GameEvent
    {
        public int FishNodeId { get; set; }
        public Vector2 Position { get; set; }
        public int Size { get; set; }
        public int RespawnCount { get; set; }
        
        public NewFishingZoneEvent(FishNode fishNode)
        {
            EventType = "NewFishingZoneObject";
            FishNodeId = fishNode.Id;
            Position = fishNode.Position;
            Size = fishNode.Size;
            RespawnCount = fishNode.RespawnCount;
        }
    }
} 