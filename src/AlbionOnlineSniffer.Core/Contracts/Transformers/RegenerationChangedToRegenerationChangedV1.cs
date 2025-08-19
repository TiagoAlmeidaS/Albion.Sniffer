using System;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Contracts;
using AlbionOnlineSniffer.Core.Models.Events;

namespace AlbionOnlineSniffer.Core.Contracts.Transformers;

public class RegenerationChangedToRegenerationChangedV1 : IEventContractTransformer
{
    public bool CanTransform(object gameEvent) => gameEvent is RegenerationChangedEvent;
    
    public (bool Success, string Topic, object Contract) TryTransform(object gameEvent)
    {
        if (gameEvent is not RegenerationChangedEvent e) return (false, string.Empty, null!);
        
        var contract = new RegenerationChangedV1
        {
            EventId = Guid.NewGuid().ToString("n"),
            ObservedAt = DateTimeOffset.UtcNow,
            Id = e.Id,
            CurrentHealth = e.Health?.Value ?? 0,
            MaxHealth = e.Health?.MaxValue ?? 0,
            RegenerationRate = e.Health?.Regeneration ?? 0
        };
        
        return (true, "albion.event.regeneration.changed.v1", contract);
    }
}
