using System;
using System.Collections.Generic;
using System.Numerics;

namespace AlbionOnlineSniffer.Core.Models.GameObjects
{
    /// <summary>
    /// Representa o jogador local no Albion Online
    /// Baseado no modelo do albion-radar-deatheye-2pc
    /// </summary>
    public class LocalPlayer
    {
        public LocalPlayer()
        {
            Guild = "!!!!";
            Alliance = "!!!!";
            Faction = Faction.NoPVP;
            IsStanding = true;
            Position = Vector2.Zero;
            NewPosition = Vector2.Zero;
            Speed = 5.5f;
            CurrentCluster = new Cluster
            {
                ClusterColor = ClusterColor.Unknown,
                DisplayName = "Unknown"
            };
        }

        public int Id { get; set; }
        public string Nickname { get; set; } = string.Empty;
        public string Guild { get; set; } = string.Empty;
        public string Alliance { get; set; } = string.Empty;
        public Faction Faction { get; set; }

        public bool IsStanding { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 NewPosition { get; set; }
        public float Speed { get; set; }
        public DateTime Time { get; set; }

        public Cluster CurrentCluster { get; set; } = new();
        public DynamicClusterData? DynamicClusterData { get; set; }
        public List<string> Chests { get; set; } = new();
    }

    /// <summary>
    /// Representa um cluster (mapa) no Albion Online
    /// </summary>
    public class Cluster
    {
        public string Id { get; set; } = string.Empty;
        public ClusterColor ClusterColor { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public ClusterSubtype Subtype { get; set; }
        public string LobbyID { get; set; } = string.Empty;
        public Dictionary<int, ClusterObjective> ClusterObjectives { get; set; } = new();
        public DateTime TimeCycle { get; set; }
        public int PlayersCount { get; set; }
    }

    /// <summary>
    /// Representa um objetivo de cluster
    /// </summary>
    public class ClusterObjective
    {
        public int Id { get; set; }
        public DateTime Timer { get; set; }
        public Vector2 Position { get; set; }
        public int Charge { get; set; }
        public string Type { get; set; } = string.Empty;
    }

    /// <summary>
    /// Representa dados dinâmicos de cluster
    /// </summary>
    public class DynamicClusterData
    {
        public string ClusterId { get; set; } = string.Empty;
        public DateTime LastUpdate { get; set; }
    }

    /// <summary>
    /// Cores de cluster
    /// </summary>
    public enum ClusterColor : byte
    {
        City,
        Default,
        Black,
        Unknown
    }

    /// <summary>
    /// Subtipos de cluster
    /// </summary>
    public enum ClusterSubtype : byte
    {
        Unknown,
        Mist,
        Abbey
    }
} 