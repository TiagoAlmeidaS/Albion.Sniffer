using System;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Contracts;
using AlbionOnlineSniffer.Core.Models.Events;

namespace AlbionOnlineSniffer.Core.Contracts.Transformers;

public class HarvestableChangeStateToHarvestableStateChangedV1 : IEventContractTransformer
{
    public bool CanTransform(object gameEvent) => gameEvent is HarvestableChangeStateEvent;
    
    public (bool Success, string Topic, object Contract) TryTransform(object gameEvent)
    {
        if (gameEvent is not HarvestableChangeStateEvent e) return (false, string.Empty, null!);
        
        var contract = new HarvestableStateChangedV1
        {
            EventId = Guid.NewGuid().ToString("n"),
            ObservedAt = DateTimeOffset.UtcNow,
            Id = e.Id,
            Count = e.Count,
            Charge = e.Charge
        };
        
        return (true, "albion.event.harvestable.state.changed.v1", contract);
    }
}
