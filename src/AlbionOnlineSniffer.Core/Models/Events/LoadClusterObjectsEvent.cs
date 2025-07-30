using System;
using System.Collections.Generic;
using AlbionOnlineSniffer.Core.Models;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento de carregamento de objetos de cluster
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class LoadClusterObjectsEvent : GameEvent
    {
        public LoadClusterObjectsEvent(Dictionary<int, ClusterObjective> clusterObjectives)
        {
            EventType = "LoadClusterObjects";
            ClusterObjectives = clusterObjectives;
        }

        public Dictionary<int, ClusterObjective> ClusterObjectives { get; set; }
    }
} 