using System;
using System.Threading.Tasks;
using Albion.Network;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models;
using AlbionOnlineSniffer.Core.Models.GameObjects;
using Microsoft.Extensions.Logging;

namespace AlbionOnlineSniffer.Core.Handlers.AlbionNetworkHandlers
{
    /// <summary>
    /// Handler para eventos RegenerationChanged que implementa Albion.Network.EventPacketHandler<T>
    /// Conecta com o EventDispatcher para publicação na fila
    /// </summary>
    public class RegenerationChangedEventHandler : EventPacketHandler<AlbionNetworkRegenerationChangedEvent>
    {
        private readonly EventDispatcher _eventDispatcher;
        private readonly ILogger<RegenerationChangedEventHandler> _logger;

        public RegenerationChangedEventHandler(EventDispatcher eventDispatcher, ILogger<RegenerationChangedEventHandler> logger, PacketIndexes packetIndexes) 
            : base(packetIndexes.RegenerationHealthChangedEvent)
        {
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        protected override async Task OnActionAsync(AlbionNetworkRegenerationChangedEvent value)
        {
            try
            {
                _logger.LogInformation("🎯 PROCESSANDO RegenerationChangedEvent: ID {Id}, Regeneration {Regeneration}", 
                    value.Id, value.Regeneration);

                // Criar evento do jogo para o EventDispatcher
                var health = new Health(100, 100); // TODO: Extrair health do evento
                var gameEvent = new RegenerationChangedEvent(value.Id, health);
                gameEvent.Timestamp = DateTime.UtcNow;

                // Disparar para o EventDispatcher que publicará na fila
                await _eventDispatcher.DispatchEvent(gameEvent);

                _logger.LogInformation("✅ RegenerationChangedEvent processado e disparado para fila: ID {Id}", value.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao processar RegenerationChangedEvent: {Message}", ex.Message);
            }
        }
    }
} 
