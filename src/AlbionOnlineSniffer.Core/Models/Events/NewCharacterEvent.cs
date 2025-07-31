using System.Numerics;
using AlbionOnlineSniffer.Core.Models.GameObjects;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento específico para quando um novo jogador é detectado
    /// </summary>
    public class NewCharacterEvent : GameEvent
    {
        public int PlayerId { get; set; }
        public string Name { get; set; }
        public string Guild { get; set; }
        public string Alliance { get; set; }
        public Vector2 Position { get; set; }
        public Health Health { get; set; }
        public Faction Faction { get; set; }
        public Equipment Equipment { get; set; }
        public int[] Spells { get; set; }
        
        public NewCharacterEvent(Player player)
        {
            EventType = "NewCharacter";
            PlayerId = player.Id;
            Name = player.Name;
            Guild = player.Guild;
            Alliance = player.Alliance;
            Position = player.Position;
            Health = player.Health;
            Faction = player.Faction;
            Equipment = player.Equipment;
            Spells = player.Spells;
        }
    }
} 