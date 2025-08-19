using System;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Contracts;
using AlbionOnlineSniffer.Core.Models.Events;

namespace AlbionOnlineSniffer.Core.Contracts.Transformers;

public class MountedToMountedStateChangedV1 : IEventContractTransformer
{
    public bool CanTransform(object gameEvent) => gameEvent is MountedEvent;
    
    public (bool Success, string Topic, object Contract) TryTransform(object gameEvent)
    {
        if (gameEvent is not MountedEvent e) return (false, string.Empty, null!);
        
        var contract = new MountedStateChangedV1
        {
            EventId = Guid.NewGuid().ToString("n"),
            ObservedAt = DateTimeOffset.UtcNow,
            Id = e.Id,
            IsMounted = e.IsMounted
        };
        
        return (true, "albion.event.mounted.state.changed.v1", contract);
    }
}
