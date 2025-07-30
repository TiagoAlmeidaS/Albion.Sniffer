using System;
using System.Numerics;

namespace AlbionOnlineSniffer.Core.Models.GameObjects
{
    /// <summary>
    /// Representa uma zona de pesca no Albion Online
    /// </summary>
    public class FishNode
    {
        public FishNode(int id, Vector2 position, int size, int respawnCount)
        {
            Id = id;
            Position = position;
            Size = size;
            RespawnCount = respawnCount;
            Time = DateTime.UtcNow;
        }

        public int Id { get; set; }
        public Vector2 Position { get; set; }
        public int Size { get; set; }
        public int RespawnCount { get; set; }
        public DateTime Time { get; set; }
    }
} 