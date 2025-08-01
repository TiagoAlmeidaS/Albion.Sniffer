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
    /// Handler para requisições JoinRequest que implementa Albion.Network.RequestPacketHandler<T>
    /// Conecta com o EventDispatcher para publicação na fila
    /// </summary>
    public class JoinRequestOperationHandler : RequestPacketHandler<AlbionNetworkJoinRequestEvent>
    {
        private readonly EventDispatcher _eventDispatcher;
        private readonly ILogger<JoinRequestOperationHandler> _logger;

        public JoinRequestOperationHandler(EventDispatcher eventDispatcher, ILogger<JoinRequestOperationHandler> logger, PacketIndexes packetIndexes) 
            : base(packetIndexes.JoinResponse) // Usando JoinResponse como placeholder
        {
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        protected override async Task OnActionAsync(AlbionNetworkJoinRequestEvent value)
        {
            try
            {
                _logger.LogInformation("🎯 PROCESSANDO JoinRequestEvent: PlayerId {PlayerId}, GuildId {GuildId}", 
                    value.PlayerId, value.GuildId);

                // Criar evento do jogo para o EventDispatcher
                var gameEvent = new JoinRequestEvent(value.PlayerId, value.GuildId);
                gameEvent.Timestamp = DateTime.UtcNow;

                // Disparar para o EventDispatcher que publicará na fila
                await _eventDispatcher.DispatchEvent(gameEvent);

                _logger.LogInformation("✅ JoinRequestEvent processado e disparado para fila: PlayerId {PlayerId}", value.PlayerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao processar JoinRequestEvent: {Message}", ex.Message);
            }
        }
    }
} 
