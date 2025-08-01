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
    /// Handler para eventos Leave que implementa Albion.Network.EventPacketHandler<T>
    /// Conecta com o EventDispatcher para publicação na fila
    /// </summary>
    public class LeaveEventHandler : EventPacketHandler<AlbionNetworkLeaveEvent>
    {
        private readonly EventDispatcher _eventDispatcher;
        private readonly ILogger<LeaveEventHandler> _logger;

        public LeaveEventHandler(EventDispatcher eventDispatcher, ILogger<LeaveEventHandler> logger, PacketIndexes packetIndexes) 
            : base(packetIndexes.Leave)
        {
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        protected override async Task OnActionAsync(AlbionNetworkLeaveEvent value)
        {
            try
            {
                _logger.LogInformation("🎯 PROCESSANDO LeaveEvent: ID {Id}, EntityType {EntityType}", 
                    value.Id, value.EntityType);

                // Criar evento do jogo para o EventDispatcher
                var gameEvent = new LeaveEvent(value.Id, value.EntityType);
                gameEvent.Timestamp = DateTime.UtcNow;

                // Disparar para o EventDispatcher que publicará na fila
                await _eventDispatcher.DispatchEvent(gameEvent);

                _logger.LogInformation("✅ LeaveEvent processado e disparado para fila: ID {Id}, Type {EntityType}", value.Id, value.EntityType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao processar LeaveEvent: {Message}", ex.Message);
            }
        }
    }
} 