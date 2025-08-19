using System;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Contracts;
using AlbionOnlineSniffer.Core.Models.Events;

namespace AlbionOnlineSniffer.Core.Contracts.Transformers;

public class KeySyncToKeySyncV1 : IEventContractTransformer
{
    public bool CanTransform(object gameEvent) => gameEvent is KeySyncEvent;
    
    public (bool Success, string Topic, object Contract) TryTransform(object gameEvent)
    {
        if (gameEvent is not KeySyncEvent e) return (false, string.Empty, null!);
        
        var contract = new KeySyncV1
        {
            EventId = Guid.NewGuid().ToString("n"),
            ObservedAt = DateTimeOffset.UtcNow,
            Code = e.Code,
            Key = e.Key
        };
        
        return (true, "albion.event.key.sync.v1", contract);
    }
}
