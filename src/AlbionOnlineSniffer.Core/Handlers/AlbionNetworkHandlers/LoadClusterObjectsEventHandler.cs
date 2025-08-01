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
    /// Handler para eventos LoadClusterObjects que implementa Albion.Network.EventPacketHandler<T>
    /// Conecta com o EventDispatcher para publicação na fila
    /// </summary>
    public class LoadClusterObjectsEventHandler : EventPacketHandler<AlbionNetworkLoadClusterObjectsEvent>
    {
        private readonly EventDispatcher _eventDispatcher;
        private readonly ILogger<LoadClusterObjectsEventHandler> _logger;

        public LoadClusterObjectsEventHandler(EventDispatcher eventDispatcher, ILogger<LoadClusterObjectsEventHandler> logger, PacketIndexes packetIndexes) 
            : base(packetIndexes.LoadClusterObjects)
        {
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        protected override async Task OnActionAsync(AlbionNetworkLoadClusterObjectsEvent value)
        {
            try
            {
                _logger.LogInformation("🎯 PROCESSANDO LoadClusterObjectsEvent: ClusterId {ClusterId}, ObjectCount {ObjectCount}", 
                    value.ClusterId, value.ObjectCount);

                // Criar evento do jogo para o EventDispatcher
                var gameEvent = new LoadClusterObjectsEvent(new Dictionary<int, ClusterObjective>()); // TODO: Implementar parsing correto
                gameEvent.Timestamp = DateTime.UtcNow;

                // Disparar para o EventDispatcher que publicará na fila
                await _eventDispatcher.DispatchEvent(gameEvent);

                _logger.LogInformation("✅ LoadClusterObjectsEvent processado e disparado para fila: ClusterId {ClusterId}", value.ClusterId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao processar LoadClusterObjectsEvent: {Message}", ex.Message);
            }
        }
    }
} 
