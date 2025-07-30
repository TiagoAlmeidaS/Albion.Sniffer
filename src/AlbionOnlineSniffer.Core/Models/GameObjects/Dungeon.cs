using System.Numerics;

namespace AlbionOnlineSniffer.Core.Models.GameObjects
{
    /// <summary>
    /// Representa uma dungeon no Albion Online
    /// Baseado no modelo do albion-radar-deatheye-2pc
    /// </summary>
    public class Dungeon
    {
        public Dungeon(int id, string type, Vector2 position, int charges)
        {
            Id = id;
            Type = GetDungeonType(type);
            Position = position;
            Charges = charges;
        }

        private DungeonType GetDungeonType(string type)
        {
            if (type.Contains("CORRUPTED")) return DungeonType.Corrupted;
            if (type.Contains("MISTS")) return DungeonType.Mists;
            if (type.Contains("HELLGATE")) return DungeonType.Hellgate;
            if (type.Contains("_SOLO")) return DungeonType.Solo;
            return DungeonType.Group;
        }

        public int Id { get; set; }
        public DungeonType Type { get; set; }
        public Vector2 Position { get; set; }
        public int Charges { get; set; }
    }

    /// <summary>
    /// Tipos de dungeon
    /// </summary>
    public enum DungeonType : byte
    {
        Solo,
        Group,
        Corrupted,
        Hellgate,
        Mists
    }
} 