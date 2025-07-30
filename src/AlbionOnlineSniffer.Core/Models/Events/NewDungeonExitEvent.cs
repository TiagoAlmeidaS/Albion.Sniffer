using System;
using System.Numerics;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento de nova saída de dungeon
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class NewDungeonExitEvent : GameEvent
    {
        public NewDungeonExitEvent(int id, Vector2 position, string type, int charges)
        {
            EventType = "NewDungeonExit";
            Id = id;
            Position = position;
            Type = type;
            Charges = charges;
        }

        public int Id { get; set; }
        public Vector2 Position { get; set; }
        public string Type { get; set; }
        public int Charges { get; set; }
    }
} 