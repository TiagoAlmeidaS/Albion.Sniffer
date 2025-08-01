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
    /// Handler para eventos WispGateOpened que implementa Albion.Network.EventPacketHandler<T>
    /// Conecta com o EventDispatcher para publicação na fila
    /// </summary>
    public class WispGateOpenedEventHandler : EventPacketHandler<AlbionNetworkWispGateOpenedEvent>
    {
        private readonly EventDispatcher _eventDispatcher;
        private readonly ILogger<WispGateOpenedEventHandler> _logger;

        public WispGateOpenedEventHandler(EventDispatcher eventDispatcher, ILogger<WispGateOpenedEventHandler> logger, PacketIndexes packetIndexes) 
            : base(packetIndexes.WispGateOpened)
        {
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        protected override async Task OnActionAsync(AlbionNetworkWispGateOpenedEvent value)
        {
            try
            {
                _logger.LogInformation("🎯 PROCESSANDO WispGateOpenedEvent: ID {Id}, IsOpened {IsOpened}", 
                    value.Id, value.IsOpened);

                // Criar GatedWisp para o construtor do WispGateOpenedEvent
                var gatedWisp = new GatedWisp(value.Id, new System.Numerics.Vector2(0, 0)); // TODO: Extrair posição do evento
                var gameEvent = new WispGateOpenedEvent(gatedWisp);
                gameEvent.Timestamp = DateTime.UtcNow;

                // Disparar para o EventDispatcher que publicará na fila
                await _eventDispatcher.DispatchEvent(gameEvent);

                _logger.LogInformation("✅ WispGateOpenedEvent processado e disparado para fila: ID {Id}", value.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao processar WispGateOpenedEvent: {Message}", ex.Message);
            }
        }
    }
} 
