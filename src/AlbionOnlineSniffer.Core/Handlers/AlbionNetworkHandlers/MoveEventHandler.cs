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
    /// Handler para eventos Move que implementa Albion.Network.EventPacketHandler<T>
    /// Conecta com o EventDispatcher para publicação na fila
    /// </summary>
    public class MoveEventHandler : EventPacketHandler<AlbionNetworkMoveEvent>
    {
        private readonly EventDispatcher _eventDispatcher;
        private readonly ILogger<MoveEventHandler> _logger;

        public MoveEventHandler(EventDispatcher eventDispatcher, ILogger<MoveEventHandler> logger, PacketIndexes packetIndexes) 
            : base(packetIndexes.Move)
        {
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        protected override async Task OnActionAsync(AlbionNetworkMoveEvent value)
        {
            try
            {
                _logger.LogInformation("🎯 PROCESSANDO MoveEvent: ID {Id} -> Posição {Position}", 
                    value.Id, value.Position);

                // Criar evento do jogo para o EventDispatcher
                var gameEvent = new MoveEvent(value.Id, value.Position, value.Speed);
                gameEvent.Timestamp = DateTime.UtcNow;

                // Disparar para o EventDispatcher que publicará na fila
                await _eventDispatcher.DispatchEvent(gameEvent);

                _logger.LogInformation("✅ MoveEvent processado e disparado para fila: ID {Id}", value.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao processar MoveEvent: {Message}", ex.Message);
            }
        }
    }
} 
