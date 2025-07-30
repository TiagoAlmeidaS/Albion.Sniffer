using System.Numerics;

namespace AlbionOnlineSniffer.Core.Models.GameObjects
{
    /// <summary>
    /// Representa um baú de loot no Albion Online
    /// Baseado no modelo do albion-radar-deatheye-2pc
    /// </summary>
    public class LootChest
    {
        public LootChest(int id, Vector2 position, string name, int charge)
        {
            Id = id;
            Position = position;
            Name = name;
            Charge = charge;
        }

        public int Id { get; set; }
        public Vector2 Position { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Charge { get; set; }
    }
} 