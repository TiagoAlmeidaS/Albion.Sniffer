using System;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Contracts;
using AlbionOnlineSniffer.Core.Models.Events;

namespace AlbionOnlineSniffer.Core.Contracts.Transformers;

public class CharacterEquipmentChangedToEquipmentChangedV1 : IEventContractTransformer
{
    public bool CanTransform(object gameEvent) => gameEvent is CharacterEquipmentChangedEvent;
    
    public (bool Success, string Topic, object Contract) TryTransform(object gameEvent)
    {
        if (gameEvent is not CharacterEquipmentChangedEvent e) return (false, string.Empty, null!);
        
        var contract = new EquipmentChangedV1
        {
            EventId = Guid.NewGuid().ToString("n"),
            ObservedAt = DateTimeOffset.UtcNow,
            Id = e.Id,
            Equipments = e.Equipments ?? Array.Empty<int>(),
            Spells = e.Spells ?? Array.Empty<int>()
        };
        
        return (true, "albion.event.equipment.changed.v1", contract);
    }
}
