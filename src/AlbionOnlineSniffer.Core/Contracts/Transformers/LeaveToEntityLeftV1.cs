using System;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Contracts;
using AlbionOnlineSniffer.Core.Models.Events;

namespace AlbionOnlineSniffer.Core.Contracts.Transformers;

public class LeaveToEntityLeftV1 : IEventContractTransformer
{
    public bool CanTransform(object gameEvent) => gameEvent is LeaveEvent;
    
    public (bool Success, string Topic, object Contract) TryTransform(object gameEvent)
    {
        if (gameEvent is not LeaveEvent e) return (false, string.Empty, null!);
        
        var contract = new EntityLeftV1
        {
            EventId = Guid.NewGuid().ToString("n"),
            ObservedAt = DateTimeOffset.UtcNow,
            Id = e.Id
        };
        
        return (true, "albion.event.entity.left.v1", contract);
    }
}
