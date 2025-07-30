using System;
using System.Numerics;

namespace AlbionOnlineSniffer.Core.Models.GameObjects
{
    /// <summary>
    /// Representa um mob (criatura/monstro) no Albion Online
    /// Baseado no modelo do albion-radar-deatheye-2pc
    /// </summary>
    public class Mob
    {
        public Mob(int id, int typeId, Vector2 position, byte charge, MobInfo mobInfo, Health health)
        {
            Id = id;
            TypeId = typeId;
            Position = position;
            Charge = charge;
            MobInfo = mobInfo;
            Health = health;
            Time = DateTime.UtcNow;
        }

        public int Id { get; set; }
        public int TypeId { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 NewPosition { get; set; }
        public float Speed { get; set; }
        public DateTime Time { get; set; }

        public int Charge { get; set; }
        public MobInfo MobInfo { get; set; }
        public Health Health { get; set; }
    }

    /// <summary>
    /// Informações de um mob
    /// </summary>
    public class MobInfo
    {
        public int Id { get; set; }
        public int Tier { get; set; }
        public string Type { get; set; } = string.Empty;
        public string HarvestableType { get; set; } = string.Empty;
        public int Rarity { get; set; }
        public string Queue { get; set; } = string.Empty;
        public string MobName { get; set; } = string.Empty;
    }
} 