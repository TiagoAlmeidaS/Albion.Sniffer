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
    /// Handler para eventos NewGatedWisp que implementa Albion.Network.EventPacketHandler<T>
    /// Conecta com o EventDispatcher para publicação na fila
    /// </summary>
    public class NewGatedWispEventHandler : EventPacketHandler<AlbionNetworkNewGatedWispEvent>
    {
        private readonly EventDispatcher _eventDispatcher;
        private readonly ILogger<NewGatedWispEventHandler> _logger;

        public NewGatedWispEventHandler(EventDispatcher eventDispatcher, ILogger<NewGatedWispEventHandler> logger, PacketIndexes packetIndexes) 
            : base(packetIndexes.NewWispGate)
        {
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        protected override async Task OnActionAsync(AlbionNetworkNewGatedWispEvent value)
        {
            try
            {
                _logger.LogInformation("🎯 PROCESSANDO NewGatedWispEvent: ID {Id}, TypeId {TypeId}, Posição {Position}", 
                    value.Id, value.TypeId, value.Position);

                // Criar GatedWisp para o construtor do NewGatedWispEvent
                var gatedWisp = new GatedWisp(
                    value.Id,
                    value.Position
                );

                // Criar evento do jogo para o EventDispatcher
                var gameEvent = new NewGatedWispEvent(gatedWisp);
                gameEvent.Timestamp = DateTime.UtcNow;

                // Disparar para o EventDispatcher que publicará na fila
                await _eventDispatcher.DispatchEvent(gameEvent);

                _logger.LogInformation("✅ NewGatedWispEvent processado e disparado para fila: ID {Id}", value.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao processar NewGatedWispEvent: {Message}", ex.Message);
            }
        }
    }
} 
