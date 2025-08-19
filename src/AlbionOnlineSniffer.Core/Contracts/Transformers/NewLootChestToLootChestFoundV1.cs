using System;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Contracts;
using AlbionOnlineSniffer.Core.Models.Events;

namespace AlbionOnlineSniffer.Core.Contracts.Transformers;

public class NewLootChestToLootChestFoundV1 : IEventContractTransformer
{
    public bool CanTransform(object gameEvent) => gameEvent is NewLootChestEvent;
    
    public (bool Success, string Topic, object Contract) TryTransform(object gameEvent)
    {
        if (gameEvent is not NewLootChestEvent e) return (false, string.Empty, null!);
        
        var contract = new LootChestFoundV1
        {
            EventId = Guid.NewGuid().ToString("n"),
            ObservedAt = DateTimeOffset.UtcNow,
            Id = e.Id,
            X = 0, // TODO: Extract from PositionBytes when available
            Y = 0
        };
        
        return (true, "albion.event.loot.chest.found.v1", contract);
    }
}
