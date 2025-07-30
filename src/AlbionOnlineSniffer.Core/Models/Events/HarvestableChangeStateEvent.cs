using System;
using System.Numerics;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento de mudança de estado de harvestable
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class HarvestableChangeStateEvent : GameEvent
    {
        public HarvestableChangeStateEvent(int id, Vector2 position, int count, int charge)
        {
            EventType = "HarvestableChangeState";
            Id = id;
            Position = position;
            Count = count;
            Charge = charge;
        }

        public int Id { get; set; }
        public Vector2 Position { get; set; }
        public int Count { get; set; }
        public int Charge { get; set; }
    }
} 