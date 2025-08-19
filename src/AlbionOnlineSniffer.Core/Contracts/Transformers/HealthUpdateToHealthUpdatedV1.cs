using System;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Contracts;
using AlbionOnlineSniffer.Core.Models.Events;

namespace AlbionOnlineSniffer.Core.Contracts.Transformers;

public class HealthUpdateToHealthUpdatedV1 : IEventContractTransformer
{
    public bool CanTransform(object gameEvent) => gameEvent is HealthUpdateEvent;
    
    public (bool Success, string Topic, object Contract) TryTransform(object gameEvent)
    {
        if (gameEvent is not HealthUpdateEvent e) return (false, string.Empty, null!);
        
        var contract = new HealthUpdatedV1
        {
            EventId = Guid.NewGuid().ToString("n"),
            ObservedAt = DateTimeOffset.UtcNow,
            Id = e.Id,
            Health = e.Health,
            MaxHealth = e.MaxHealth,
            Energy = e.Energy
        };
        
        return (true, "albion.event.health.updated.v1", contract);
    }
}
