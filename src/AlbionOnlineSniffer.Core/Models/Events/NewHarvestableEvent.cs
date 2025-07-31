using System.Numerics;
using AlbionOnlineSniffer.Core.Models.GameObjects;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento específico para quando um novo harvestable é detectado
    /// </summary>
    public class NewHarvestableEvent : GameEvent
    {
        public int HarvestableId { get; set; }
        public string Type { get; set; }
        public int Tier { get; set; }
        public Vector2 Position { get; set; }
        public int Count { get; set; }
        public int Charge { get; set; }
        
        public NewHarvestableEvent(Harvestable harvestable)
        {
            EventType = "NewHarvestableObject";
            HarvestableId = harvestable.Id;
            Type = harvestable.Type;
            Tier = harvestable.Tier;
            Position = harvestable.Position;
            Count = harvestable.Count;
            Charge = harvestable.Charge;
        }
    }
} 