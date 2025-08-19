using System;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Contracts;
using AlbionOnlineSniffer.Core.Models.Events;

namespace AlbionOnlineSniffer.Core.Contracts.Transformers;

public class ChangeClusterToClusterChangedV1 : IEventContractTransformer
{
    public bool CanTransform(object gameEvent) => gameEvent is ChangeClusterEvent;
    
    public (bool Success, string Topic, object Contract) TryTransform(object gameEvent)
    {
        if (gameEvent is not ChangeClusterEvent e) return (false, string.Empty, null!);
        
        var contract = new ClusterChangedV1
        {
            EventId = Guid.NewGuid().ToString("n"),
            ObservedAt = DateTimeOffset.UtcNow,
            LocationId = e.LocationId ?? string.Empty,
            Type = e.Type ?? string.Empty,
            ClusterId = e.DynamicClusterData?.ClusterId,
            ClusterName = e.DynamicClusterData?.ClusterName,
            ClusterType = e.DynamicClusterData?.ClusterType,
            Level = e.DynamicClusterData?.Level ?? 0
        };
        
        return (true, "albion.event.cluster.changed.v1", contract);
    }
}
