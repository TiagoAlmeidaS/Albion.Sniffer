using System;
using System.Collections.Generic;
using System.Numerics;

namespace AlbionOnlineSniffer.Core.Models.GameObjects
{
    /// <summary>
    /// Representa um jogador no Albion Online
    /// Baseado no modelo do albion-radar-deatheye-2pc
    /// </summary>
    public class Player
    {
        public Player(int id, string name, string guild, string alliance, Vector2 position, Health health, Faction faction, Equipment equipment, int[] spells)
        {
            Id = id;
            Name = name;
            Guild = guild;
            Alliance = alliance;
            Faction = faction;
            Position = position;
            Health = health;
            Equipment = equipment;
            Spells = spells;
            Time = DateTime.UtcNow;
        }

        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Guild { get; set; } = string.Empty;
        public string Alliance { get; set; } = string.Empty;

        public bool IsStanding { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 NewPosition { get; set; }
        public float Speed { get; set; }
        public DateTime Time { get; set; }

        public Health Health { get; set; } = new Health(0, 0);
        public Faction Faction { get; set; }
        public bool IsMounted { get; set; }

        public Equipment Equipment { get; set; } = new Equipment();
        public int[] Spells { get; set; } = Array.Empty<int>();
    }



    /// <summary>
    /// Representa o equipamento de um jogador
    /// </summary>
    public class Equipment
    {
        public Equipment()
        {
            AllItemPower = 0;
            Items = new List<PlayerItem>();
        }

        public int AllItemPower { get; set; }
        public List<PlayerItem> Items { get; set; }
    }

    /// <summary>
    /// Representa um item de um jogador
    /// </summary>
    public class PlayerItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ItemPower { get; set; }
    }

    /// <summary>
    /// Fações do Albion Online
    /// </summary>
    public enum Faction
    {
        NoPVP = 0,
        Martlock = 1,
        Lymhurst = 2,
        Bridjewatch = 3,
        ForthSterling = 4,
        Thetford = 5,
        Caerleon = 6,
        PVP = 7
    }
} 