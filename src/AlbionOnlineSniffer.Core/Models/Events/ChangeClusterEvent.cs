using System;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento de mudança de cluster
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class ChangeClusterEvent : GameEvent
    {
        public string OldCluster { get; set; } = string.Empty;
        public string NewCluster { get; set; } = string.Empty;

        public ChangeClusterEvent(string oldCluster, string newCluster)
        {
            EventType = "ChangeCluster";
            OldCluster = oldCluster;
            NewCluster = newCluster;
        }
    }
} 