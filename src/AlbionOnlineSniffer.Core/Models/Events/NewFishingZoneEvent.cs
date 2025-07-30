using System;
using System.Numerics;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento de nova zona de pesca
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class NewFishingZoneEvent : GameEvent
    {
        public NewFishingZoneEvent(int id, Vector2 position, int size, int respawnCount)
        {
            EventType = "NewFishingZone";
            Id = id;
            Position = position;
            Size = size;
            RespawnCount = respawnCount;
        }

        public int Id { get; set; }
        public Vector2 Position { get; set; }
        public int Size { get; set; }
        public int RespawnCount { get; set; }
    }
} 