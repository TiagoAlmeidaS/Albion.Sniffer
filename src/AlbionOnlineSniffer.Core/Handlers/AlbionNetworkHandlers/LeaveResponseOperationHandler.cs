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
    /// Handler para respostas LeaveResponse que implementa Albion.Network.ResponsePacketHandler<T>
    /// Conecta com o EventDispatcher para publicação na fila
    /// </summary>
    public class LeaveResponseOperationHandler : ResponsePacketHandler<AlbionNetworkLeaveResponseEvent>
    {
        private readonly EventDispatcher _eventDispatcher;
        private readonly ILogger<LeaveResponseOperationHandler> _logger;

        public LeaveResponseOperationHandler(EventDispatcher eventDispatcher, ILogger<LeaveResponseOperationHandler> logger, PacketIndexes packetIndexes) 
            : base(packetIndexes.Leave)
        {
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        protected override async Task OnActionAsync(AlbionNetworkLeaveResponseEvent value)
        {
            try
            {
                _logger.LogInformation("🎯 PROCESSANDO LeaveResponseEvent: Success {Success}, Message {Message}", 
                    value.Success, value.Message);

                // Criar evento do jogo para o EventDispatcher
                var gameEvent = new LeaveResponseEvent(value.Success, value.Message);
                gameEvent.Timestamp = DateTime.UtcNow;

                // Disparar para o EventDispatcher que publicará na fila
                await _eventDispatcher.DispatchEvent(gameEvent);

                _logger.LogInformation("✅ LeaveResponseEvent processado e disparado para fila: Success {Success}", value.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao processar LeaveResponseEvent: {Message}", ex.Message);
            }
        }
    }
} 
