using System;
using System.Numerics;

namespace AlbionOnlineSniffer.Core.Models.GameObjects
{
    /// <summary>
    /// Representa um gated wisp no Albion Online
    /// </summary>
    public class GatedWisp
    {
        public GatedWisp(int id, Vector2 position)
        {
            Id = id;
            Position = position;
            IsCollected = false;
            Time = DateTime.UtcNow;
        }

        public int Id { get; set; }
        public Vector2 Position { get; set; }
        public bool IsCollected { get; set; }
        public DateTime Time { get; set; }
    }
} 