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
    /// Handler para eventos KeySync que implementa Albion.Network.EventPacketHandler<T>
    /// Conecta com o EventDispatcher para publicação na fila
    /// </summary>
    public class KeySyncEventHandler : EventPacketHandler<AlbionNetworkKeySyncEvent>
    {
        private readonly EventDispatcher _eventDispatcher;
        private readonly ILogger<KeySyncEventHandler> _logger;

        public KeySyncEventHandler(EventDispatcher eventDispatcher, ILogger<KeySyncEventHandler> logger, PacketIndexes packetIndexes) 
            : base(packetIndexes.KeySync)
        {
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        protected override async Task OnActionAsync(AlbionNetworkKeySyncEvent value)
        {
            try
            {
                _logger.LogInformation("🎯 PROCESSANDO KeySyncEvent: ID {Id}, Key {Key}", 
                    value.Id, value.Key);

                // Criar evento do jogo para o EventDispatcher
                var code = new byte[] { (byte)value.Key }; // TODO: Implementar parsing correto do código
                var gameEvent = new KeySyncEvent(code);
                gameEvent.Timestamp = DateTime.UtcNow;

                // Disparar para o EventDispatcher que publicará na fila
                await _eventDispatcher.DispatchEvent(gameEvent);

                _logger.LogInformation("✅ KeySyncEvent processado e disparado para fila: ID {Id}", value.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao processar KeySyncEvent: {Message}", ex.Message);
            }
        }
    }
} 
