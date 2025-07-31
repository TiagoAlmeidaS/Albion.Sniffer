using System.Numerics;
using AlbionOnlineSniffer.Core.Models.GameObjects;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento específico para quando um novo baú de loot é detectado
    /// </summary>
    public class NewLootChestEvent : GameEvent
    {
        public int ChestId { get; set; }
        public string Name { get; set; }
        public Vector2 Position { get; set; }
        public int Charge { get; set; }
        
        public NewLootChestEvent(LootChest chest)
        {
            EventType = "NewLootChest";
            ChestId = chest.Id;
            Name = chest.Name;
            Position = chest.Position;
            Charge = chest.Charge;
        }
    }
} 