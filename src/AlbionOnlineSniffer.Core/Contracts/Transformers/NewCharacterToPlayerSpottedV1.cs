using System;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Contracts;
using AlbionOnlineSniffer.Core.Models.Events;
using System.Numerics;

namespace AlbionOnlineSniffer.Core.Contracts.Transformers
{
    public class NewCharacterToPlayerSpottedV1 : IEventContractTransformer
    {
        private readonly PositionDecryptionService _positionService;

        public NewCharacterToPlayerSpottedV1(PositionDecryptionService positionService)
        {
            _positionService = positionService;
        }

        public bool CanTransform(object gameEvent) => gameEvent is NewCharacterEvent;
        
        public (bool Success, string Topic, object Contract) TryTransform(object gameEvent)
        {
            if (gameEvent is not NewCharacterEvent e) return (false, string.Empty, null!);
            
            // Descriptografar posição se disponível
            Vector2 position = Vector2.Zero;
            if (e.PositionBytes != null)
            {
                position = _positionService.DecryptPosition(e.PositionBytes);
            }
            else if (e.Position != Vector2.Zero)
            {
                position = e.Position;
            }
            
            var contract = new PlayerSpottedV1
            {
                EventId = Guid.NewGuid().ToString("n"),
                ObservedAt = DateTimeOffset.UtcNow,
                Cluster = string.Empty, // TODO: Extrair do contexto atual
                Region = string.Empty,  // TODO: Extrair do contexto atual
                PlayerId = e.Id,
                PlayerName = e.Name,
                GuildName = string.IsNullOrWhiteSpace(e.GuildName) ? null : e.GuildName,
                AllianceName = string.IsNullOrWhiteSpace(e.AllianceName) ? null : e.AllianceName,
                X = position.X,
                Y = position.Y,
                Tier = 0 // TODO: Extrair do equipamento ou contexto
            };
            
            return (true, "albion.event.player.spotted.v1", contract);
        }
    }
}
