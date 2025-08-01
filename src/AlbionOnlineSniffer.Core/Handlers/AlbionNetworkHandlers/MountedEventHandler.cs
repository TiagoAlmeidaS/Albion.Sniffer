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
    /// Handler para eventos Mounted que implementa Albion.Network.EventPacketHandler<T>
    /// Conecta com o EventDispatcher para publicação na fila
    /// </summary>
    public class MountedEventHandler : EventPacketHandler<AlbionNetworkMountedEvent>
    {
        private readonly EventDispatcher _eventDispatcher;
        private readonly ILogger<MountedEventHandler> _logger;

        public MountedEventHandler(EventDispatcher eventDispatcher, ILogger<MountedEventHandler> logger, PacketIndexes packetIndexes) 
            : base(packetIndexes.Mounted)
        {
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        protected override async Task OnActionAsync(AlbionNetworkMountedEvent value)
        {
            try
            {
                _logger.LogInformation("🎯 PROCESSANDO MountedEvent: ID {Id}, Mounted {IsMounted}", 
                    value.Id, value.IsMounted);

                // Criar evento do jogo para o EventDispatcher
                var gameEvent = new MountedEvent(value.Id, value.IsMounted);
                gameEvent.Timestamp = DateTime.UtcNow;

                // Disparar para o EventDispatcher que publicará na fila
                await _eventDispatcher.DispatchEvent(gameEvent);

                _logger.LogInformation("✅ MountedEvent processado e disparado para fila: ID {Id}", value.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao processar MountedEvent: {Message}", ex.Message);
            }
        }
    }
} 
