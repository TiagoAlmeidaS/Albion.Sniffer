using System;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Contracts;
using AlbionOnlineSniffer.Core.Models.Events;

namespace AlbionOnlineSniffer.Core.Contracts.Transformers;

public class NewGatedWispToGatedWispFoundV1 : IEventContractTransformer
{
    public bool CanTransform(object gameEvent) => gameEvent is NewGatedWispEvent;
    
    public (bool Success, string Topic, object Contract) TryTransform(object gameEvent)
    {
        if (gameEvent is not NewGatedWispEvent e) return (false, string.Empty, null!);
        
        var contract = new GatedWispFoundV1
        {
            EventId = Guid.NewGuid().ToString("n"),
            ObservedAt = DateTimeOffset.UtcNow,
            Id = e.Id,
            X = 0, // TODO: Extract from PositionBytes when available
            Y = 0
        };
        
        return (true, "albion.event.gated.wisp.found.v1", contract);
    }
}
