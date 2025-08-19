using System;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Contracts;
using AlbionOnlineSniffer.Core.Models.Events;

namespace AlbionOnlineSniffer.Core.Contracts.Transformers;

public class MistsPlayerJoinedInfoToMistsPlayerJoinedV1 : IEventContractTransformer
{
    public bool CanTransform(object gameEvent) => gameEvent is MistsPlayerJoinedInfoEvent;
    
    public (bool Success, string Topic, object Contract) TryTransform(object gameEvent)
    {
        if (gameEvent is not MistsPlayerJoinedInfoEvent e) return (false, string.Empty, null!);
        
        var contract = new MistsPlayerJoinedV1
        {
            EventId = Guid.NewGuid().ToString("n"),
            ObservedAt = DateTimeOffset.UtcNow,
            TimeCycle = e.TimeCycle
        };
        
        return (true, "albion.event.mists.player.joined.v1", contract);
    }
}
