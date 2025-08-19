using System;
using System.Linq;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Contracts;
using AlbionOnlineSniffer.Core.Models.Events;
using System.Numerics;

namespace AlbionOnlineSniffer.Core.Contracts.Transformers
{
    public class NewHarvestablesListToHarvestablesListFoundV1 : IEventContractTransformer
    {
        private readonly PositionDecryptionService _positionService;

        public NewHarvestablesListToHarvestablesListFoundV1(PositionDecryptionService positionService)
        {
            _positionService = positionService;
        }

        public bool CanTransform(object gameEvent) => gameEvent is NewHarvestablesListEvent;
        
        public (bool Success, string Topic, object Contract) TryTransform(object gameEvent)
        {
            if (gameEvent is not NewHarvestablesListEvent e) return (false, string.Empty, null!);
            
            var harvestables = e.HarvestableObjects.Select(h => new HarvestableFoundV1
            {
                EventId = Guid.NewGuid().ToString("n"),
                ObservedAt = DateTimeOffset.UtcNow,
                Id = h.Id,
                TypeId = h.TypeId,
                X = 0, // TODO: Extrair de h.PositionBytes se disponível
                Y = 0,
                Tier = h.Tier,
                Charges = h.Charges
            }).ToArray();
            
            var contract = new HarvestablesListFoundV1
            {
                EventId = Guid.NewGuid().ToString("n"),
                ObservedAt = DateTimeOffset.UtcNow,
                Harvestables = harvestables
            };
            
            return (true, "albion.event.harvestables.list.found.v1", contract);
        }
    }
}
