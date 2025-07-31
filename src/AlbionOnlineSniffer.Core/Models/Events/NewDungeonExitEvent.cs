using System.Numerics;
using AlbionOnlineSniffer.Core.Models.GameObjects;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento específico para quando uma nova saída de dungeon é detectada
    /// </summary>
    public class NewDungeonExitEvent : GameEvent
    {
        public int DungeonId { get; set; }
        public DungeonType Type { get; set; }
        public Vector2 Position { get; set; }
        public int Charges { get; set; }
        
        public NewDungeonExitEvent(Dungeon dungeon)
        {
            EventType = "NewDungeonExit";
            DungeonId = dungeon.Id;
            Type = dungeon.Type;
            Position = dungeon.Position;
            Charges = dungeon.Charges;
        }
    }
} 