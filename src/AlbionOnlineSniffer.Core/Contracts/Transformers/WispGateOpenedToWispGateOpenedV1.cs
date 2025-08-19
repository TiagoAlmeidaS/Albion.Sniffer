using System;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Contracts;
using AlbionOnlineSniffer.Core.Models.Events;

namespace AlbionOnlineSniffer.Core.Contracts.Transformers;

public class WispGateOpenedToWispGateOpenedV1 : IEventContractTransformer
{
    public bool CanTransform(object gameEvent) => gameEvent is WispGateOpenedEvent;
    
    public (bool Success, string Topic, object Contract) TryTransform(object gameEvent)
    {
        if (gameEvent is not WispGateOpenedEvent e) return (false, string.Empty, null!);
        
        var contract = new WispGateOpenedV1
        {
            EventId = Guid.NewGuid().ToString("n"),
            ObservedAt = DateTimeOffset.UtcNow,
            Id = e.Id,
            IsCollected = e.isCollected
        };
        
        return (true, "albion.event.wisp.gate.opened.v1", contract);
    }
}
