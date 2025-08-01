using System;
using System.Threading.Tasks;
using Albion.Network;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models;
using Microsoft.Extensions.Logging;

namespace AlbionOnlineSniffer.Core.Handlers.AlbionNetworkHandlers
{
    /// <summary>
    /// Handler para requisições MoveRequest que implementa Albion.Network.RequestPacketHandler<T>
    /// Conecta com o EventDispatcher para publicação na fila
    /// </summary>
    public class MoveRequestOperationHandler : RequestPacketHandler<AlbionNetworkMoveRequestEvent>
    {
        private readonly EventDispatcher _eventDispatcher;
        private readonly ILogger<MoveRequestOperationHandler> _logger;

        public MoveRequestOperationHandler(EventDispatcher eventDispatcher, ILogger<MoveRequestOperationHandler> logger, PacketIndexes packetIndexes) 
            : base(packetIndexes.MoveRequest)
        {
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        protected override async Task OnActionAsync(AlbionNetworkMoveRequestEvent value)
        {
            try
            {
                _logger.LogInformation("🎯 PROCESSANDO MoveRequestEvent: PlayerId {PlayerId}, Position {Position}", 
                    value.PlayerId, value.Position);

                // Criar evento do jogo para o EventDispatcher
                var gameEvent = new MoveRequestEvent(value.PlayerId, value.Position, true); // TODO: Implementar parsing correto do isMoving
                gameEvent.Timestamp = DateTime.UtcNow;

                // Disparar para o EventDispatcher que publicará na fila
                await _eventDispatcher.DispatchEvent(gameEvent);

                _logger.LogInformation("✅ MoveRequestEvent processado e disparado para fila: PlayerId {PlayerId}", value.PlayerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao processar MoveRequestEvent: {Message}", ex.Message);
            }
        }
    }
} 
