using System;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Contracts;
using AlbionOnlineSniffer.Core.Models.Events;
using System.Numerics;

namespace AlbionOnlineSniffer.Core.Contracts.Transformers
{
    public class NewMobToMobSpawnedV1 : IEventContractTransformer
    {
        private readonly PositionDecryptionService _positionService;

        public NewMobToMobSpawnedV1(PositionDecryptionService positionService)
        {
            _positionService = positionService;
        }

        public bool CanTransform(object gameEvent) => gameEvent is NewMobEvent;
        
        public (bool Success, string Topic, object Contract) TryTransform(object gameEvent)
        {
            if (gameEvent is not NewMobEvent e) return (false, string.Empty, null!);
            
            // Descriptografar posição se disponível
            Vector2 position = Vector2.Zero;
            if (e.PositionBytes != null)
            {
                position = _positionService.DecryptPosition(e.PositionBytes);
            }
            
            var contract = new MobSpawnedV1
            {
                EventId = Guid.NewGuid().ToString("n"),
                ObservedAt = DateTimeOffset.UtcNow,
                MobId = e.Id,
                TypeId = e.TypeId,
                Tier = e.EnchantmentLevel,
                X = position.X,
                Y = position.Y,
                Health = e.Health,
                MaxHealth = e.MaxHealth
            };
            
            return (true, "albion.event.mob.spawned.v1", contract);
        }
    }
}
