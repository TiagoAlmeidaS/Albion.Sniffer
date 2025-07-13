using System.Numerics;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class NewCharacterEvent
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Guild { get; set; }
        public string Alliance { get; set; }
        public Vector2 Position { get; set; }
        public int Health { get; set; }
        public int Faction { get; set; }
        public object Equipments { get; set; }
        public object Spells { get; set; }
        public object EncryptedPosition { get; set; } // Para compatibilidade com decrypt
    }
} 