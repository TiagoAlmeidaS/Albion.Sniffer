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
    /// Handler para eventos HarvestableChangeState que implementa Albion.Network.EventPacketHandler<T>
    /// Conecta com o EventDispatcher para publicação na fila
    /// </summary>
    public class HarvestableChangeStateEventHandler : EventPacketHandler<AlbionNetworkHarvestableChangeStateEvent>
    {
        private readonly EventDispatcher _eventDispatcher;
        private readonly ILogger<HarvestableChangeStateEventHandler> _logger;

        public HarvestableChangeStateEventHandler(EventDispatcher eventDispatcher, ILogger<HarvestableChangeStateEventHandler> logger, PacketIndexes packetIndexes) 
            : base(packetIndexes.HarvestableChangeState)
        {
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        protected override async Task OnActionAsync(AlbionNetworkHarvestableChangeStateEvent value)
        {
            try
            {
                _logger.LogInformation("🎯 PROCESSANDO HarvestableChangeStateEvent: ID {Id}, Position {Position}, Count {Count}, Charge {Charge}", 
                    value.Id, value.Position, value.Count, value.Charge);

                // Criar evento do jogo para o EventDispatcher
                var gameEvent = new HarvestableChangeStateEvent(value.Id, value.Position, value.Count, value.Charge);
                gameEvent.Timestamp = DateTime.UtcNow;

                // Disparar para o EventDispatcher que publicará na fila
                await _eventDispatcher.DispatchEvent(gameEvent);

                _logger.LogInformation("✅ HarvestableChangeStateEvent processado e disparado para fila: ID {Id}", value.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao processar HarvestableChangeStateEvent: {Message}", ex.Message);
            }
        }
    }
} 
