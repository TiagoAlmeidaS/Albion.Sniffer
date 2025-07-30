using System.Numerics;

namespace AlbionOnlineSniffer.Core.Models.GameObjects
{
    /// <summary>
    /// Representa um harvestable (recurso coletável) no Albion Online
    /// Baseado no modelo do albion-radar-deatheye-2pc
    /// </summary>
    public class Harvestable
    {
        public Harvestable(int id, string type, int tier, Vector2 position, int count, int charge)
        {
            Id = id;
            Type = type;
            Tier = tier;
            Position = position;
            Count = count;
            Charge = charge;
        }

        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public int Tier { get; set; }
        public Vector2 Position { get; set; }
        public int Count { get; set; }
        public int Charge { get; set; }
    }
} 