using System;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Contracts;
using AlbionOnlineSniffer.Core.Models.Events;
using System.Numerics;

namespace AlbionOnlineSniffer.Core.Contracts.Transformers
{
    public class MoveEventToPlayerSpottedV1 : IEventContractTransformer
    {
        private readonly PositionDecryptionService _positionService;

        public MoveEventToPlayerSpottedV1(PositionDecryptionService positionService)
        {
            _positionService = positionService;
        }

        public bool CanTransform(object gameEvent) => gameEvent is MoveEvent;
        
        public (bool Success, string Topic, object Contract) TryTransform(object gameEvent)
        {
            if (gameEvent is not MoveEvent e) return (false, string.Empty, null!);
            
            // Descriptografar posição se disponível
            Vector2 position = Vector2.Zero;
            if (e.NewPositionBytes != null)
            {
                position = _positionService.DecryptPosition(e.NewPositionBytes);
            }
            else if (e.PositionBytes != null)
            {
                position = _positionService.DecryptPosition(e.PositionBytes);
            }
            else if (e.NewPosition.HasValue)
            {
                position = e.NewPosition.Value;
            }
            else if (e.Position.HasValue)
            {
                position = e.Position.Value;
            }
            
            var contract = new PlayerSpottedV1
            {
                EventId = Guid.NewGuid().ToString("n"),
                ObservedAt = DateTimeOffset.UtcNow,
                Cluster = string.Empty, // TODO: Extrair do contexto atual
                Region = string.Empty,  // TODO: Extrair do contexto atual
                PlayerId = e.Id,
                PlayerName = string.Empty, // MoveEvent não carrega nome do jogador
                GuildName = null,
                AllianceName = null,
                X = position.X,
                Y = position.Y,
                Tier = 0 // TODO: Extrair do equipamento ou contexto
            };
            
            return (true, "albion.event.player.spotted.v1", contract);
        }
    }
}
