using System;
using System.Numerics;

namespace Albion.Sniffer.Core.Models
{
    /// <summary>
    /// Dados de movimento de um jogador
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class MoveData
    {
        public int PlayerId { get; set; }
        public Vector2 Position { get; set; }
        public DateTime Timestamp { get; set; }
    }
} 