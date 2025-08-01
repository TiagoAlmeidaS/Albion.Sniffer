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
    /// Handler para respostas JoinResponse que implementa Albion.Network.ResponsePacketHandler<T>
    /// Conecta com o EventDispatcher para publicação na fila
    /// </summary>
    public class JoinResponseOperationHandler : ResponsePacketHandler<AlbionNetworkJoinResponseEvent>
    {
        private readonly EventDispatcher _eventDispatcher;
        private readonly ILogger<JoinResponseOperationHandler> _logger;

        public JoinResponseOperationHandler(EventDispatcher eventDispatcher, ILogger<JoinResponseOperationHandler> logger, PacketIndexes packetIndexes) 
            : base(packetIndexes.JoinResponse)
        {
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        protected override async Task OnActionAsync(AlbionNetworkJoinResponseEvent value)
        {
            try
            {
                _logger.LogInformation("🎯 PROCESSANDO JoinResponseEvent: Success {Success}, Message {Message}", 
                    value.Success, value.Message);

                // Criar evento do jogo para o EventDispatcher
                var gameEvent = new JoinResponseEvent(value.Success, value.Message);
                gameEvent.Timestamp = DateTime.UtcNow;

                // Disparar para o EventDispatcher que publicará na fila
                await _eventDispatcher.DispatchEvent(gameEvent);

                _logger.LogInformation("✅ JoinResponseEvent processado e disparado para fila: Success {Success}", value.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao processar JoinResponseEvent: {Message}", ex.Message);
            }
        }
    }
} 
