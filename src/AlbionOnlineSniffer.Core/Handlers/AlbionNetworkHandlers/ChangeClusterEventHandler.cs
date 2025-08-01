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
    /// Handler para respostas ChangeCluster que implementa Albion.Network.ResponsePacketHandler<T>
    /// Conecta com o EventDispatcher para publicação na fila
    /// </summary>
    public class ChangeClusterEventHandler : ResponsePacketHandler<AlbionNetworkChangeClusterEvent>
    {
        private readonly EventDispatcher _eventDispatcher;
        private readonly ILogger<ChangeClusterEventHandler> _logger;

        public ChangeClusterEventHandler(EventDispatcher eventDispatcher, ILogger<ChangeClusterEventHandler> logger, PacketIndexes packetIndexes) 
            : base(packetIndexes.ChangeCluster)
        {
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        protected override async Task OnActionAsync(AlbionNetworkChangeClusterEvent value)
        {
            try
            {
                _logger.LogInformation("🎯 PROCESSANDO ChangeClusterEvent: ClusterId {ClusterId}, Success {Success}", 
                    value.ClusterId, value.Success);

                // Criar evento do jogo para o EventDispatcher
                var gameEvent = new ChangeClusterEvent($"Cluster_{value.ClusterId}", $"Cluster_{value.ClusterId}"); // TODO: Implementar parsing correto
                gameEvent.Timestamp = DateTime.UtcNow;

                // Disparar para o EventDispatcher que publicará na fila
                await _eventDispatcher.DispatchEvent(gameEvent);

                _logger.LogInformation("✅ ChangeClusterEvent processado e disparado para fila: ClusterId {ClusterId}", value.ClusterId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao processar ChangeClusterEvent: {Message}", ex.Message);
            }
        }
    }
} 
