using System;
using System.Collections.Generic;
using System.Numerics;

namespace AlbionOnlineSniffer.Core.Models
{
    /// <summary>
    /// Representa um cluster/mapa do Albion Online
    /// Baseado no albion-radar-deatheye-2pc
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
    /// Cores dos clusters
    /// </summary>
    public enum ClusterColor
    {
        Unknown = 0,
        Green = 1,
        Red = 2,
        City = 3
    }

    /// <summary>
    /// Subtipos de cluster
    /// </summary>
    public enum ClusterSubtype
    {
        Unknown = 0,
        Solo = 1,
        Group = 2,
        Corrupted = 3,
        Hellgate = 4
    }

    /// <summary>
    /// Objetivo de um cluster
    /// </summary>
    public class ClusterObjective
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    /// <summary>
    /// Dados dinâmicos de cluster
    /// </summary>
    public class DynamicClusterData
    {
        public byte Type { get; set; }
        public string ClusterId { get; set; } = string.Empty;
        public string ClusterName { get; set; } = string.Empty;
        public Vector2 UnknownVector1 { get; set; }
        public Vector2 UnknownVector2 { get; set; }
        public List<TemplateInstance> TemplateInstances { get; set; } = new();
    }

    /// <summary>
    /// Instância de template
    /// </summary>
    public class TemplateInstance
    {
        public string TemplateId { get; set; } = string.Empty;
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
        public float Scale { get; set; }
    }
} 