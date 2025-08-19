using System;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Contracts;
using AlbionOnlineSniffer.Core.Models.Events;

namespace AlbionOnlineSniffer.Core.Contracts.Transformers;

public class MobChangeStateToMobStateChangedV1 : IEventContractTransformer
{
    public bool CanTransform(object gameEvent) => gameEvent is MobChangeStateEvent;
    
    public (bool Success, string Topic, object Contract) TryTransform(object gameEvent)
    {
        if (gameEvent is not MobChangeStateEvent e) return (false, string.Empty, null!);
        
        var contract = new MobStateChangedV1
        {
            EventId = Guid.NewGuid().ToString("n"),
            ObservedAt = DateTimeOffset.UtcNow,
            Id = e.Id,
            Charge = e.Charge
        };
        
        return (true, "albion.event.mob.state.changed.v1", contract);
    }
}
