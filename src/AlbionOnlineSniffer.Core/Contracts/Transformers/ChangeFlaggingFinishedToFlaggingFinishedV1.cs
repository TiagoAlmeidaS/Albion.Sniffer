using System;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Contracts;
using AlbionOnlineSniffer.Core.Models.Events;

namespace AlbionOnlineSniffer.Core.Contracts.Transformers;

public class ChangeFlaggingFinishedToFlaggingFinishedV1 : IEventContractTransformer
{
    public bool CanTransform(object gameEvent) => gameEvent is ChangeFlaggingFinishedEvent;
    
    public (bool Success, string Topic, object Contract) TryTransform(object gameEvent)
    {
        if (gameEvent is not ChangeFlaggingFinishedEvent e) return (false, string.Empty, null!);
        
        var contract = new FlaggingFinishedV1
        {
            EventId = Guid.NewGuid().ToString("n"),
            ObservedAt = DateTimeOffset.UtcNow,
            Id = e.Id,
            Faction = e.Faction.ToString()
        };
        
        return (true, "albion.event.flagging.finished.v1", contract);
    }
}
