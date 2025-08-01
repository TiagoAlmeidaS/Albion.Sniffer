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
    /// Handler para eventos HealthUpdate que implementa Albion.Network.EventPacketHandler<T>
    /// Conecta com o EventDispatcher para publicação na fila
    /// </summary>
    public class HealthUpdateEventHandler : EventPacketHandler<AlbionNetworkHealthUpdateEvent>
    {
        private readonly EventDispatcher _eventDispatcher;
        private readonly ILogger<HealthUpdateEventHandler> _logger;

        public HealthUpdateEventHandler(EventDispatcher eventDispatcher, ILogger<HealthUpdateEventHandler> logger, PacketIndexes packetIndexes) 
            : base(packetIndexes.HealthUpdateEvent)
        {
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        protected override async Task OnActionAsync(AlbionNetworkHealthUpdateEvent value)
        {
            try
            {
                _logger.LogInformation("🎯 PROCESSANDO HealthUpdateEvent: ID {Id}, Health {CurrentHealth}/{MaxHealth}", 
                    value.Id, value.Health.CurrentHealth, value.Health.MaxHealth);

                // Criar evento do jogo para o EventDispatcher
                var gameEvent = new HealthUpdateEvent(value.Id, value.Health);
                gameEvent.Timestamp = DateTime.UtcNow;

                // Disparar para o EventDispatcher que publicará na fila
                await _eventDispatcher.DispatchEvent(gameEvent);

                _logger.LogInformation("✅ HealthUpdateEvent processado e disparado para fila: ID {Id}", value.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao processar HealthUpdateEvent: {Message}", ex.Message);
            }
        }
    }
} 
