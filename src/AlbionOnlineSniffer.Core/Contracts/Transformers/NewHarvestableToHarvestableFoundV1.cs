using System;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Contracts;
using AlbionOnlineSniffer.Core.Models.Events;
using System.Numerics;

namespace AlbionOnlineSniffer.Core.Contracts.Transformers
{
    public class NewHarvestableToHarvestableFoundV1 : IEventContractTransformer
    {
        private readonly PositionDecryptionService _positionService;

        public NewHarvestableToHarvestableFoundV1(PositionDecryptionService positionService)
        {
            _positionService = positionService;
        }

        public bool CanTransform(object gameEvent) => gameEvent is NewHarvestableEvent;
        
        public (bool Success, string Topic, object Contract) TryTransform(object gameEvent)
        {
            if (gameEvent is not NewHarvestableEvent e) return (false, string.Empty, null!);
            
            // Descriptografar posição se disponível
            Vector2 position = Vector2.Zero;
            if (e.PositionBytes != null)
            {
                position = _positionService.DecryptPosition(e.PositionBytes);
            }
            
            var contract = new HarvestableFoundV1
            {
                EventId = Guid.NewGuid().ToString("n"),
                ObservedAt = DateTimeOffset.UtcNow,
                Id = e.Id,
                TypeId = e.TypeId,
                X = position.X,
                Y = position.Y,
                Tier = e.Tier,
                Charges = e.Charges
            };
            
            return (true, "albion.event.harvestable.found.v1", contract);
        }
    }
}
