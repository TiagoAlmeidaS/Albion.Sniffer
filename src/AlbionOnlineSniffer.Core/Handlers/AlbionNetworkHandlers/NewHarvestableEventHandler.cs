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
    /// Handler para eventos NewHarvestable que implementa Albion.Network.EventPacketHandler<T>
    /// Conecta com o EventDispatcher para publicação na fila
    /// </summary>
    public class NewHarvestableEventHandler : EventPacketHandler<AlbionNetworkNewHarvestableEvent>
    {
        private readonly EventDispatcher _eventDispatcher;
        private readonly ILogger<NewHarvestableEventHandler> _logger;

        public NewHarvestableEventHandler(EventDispatcher eventDispatcher, ILogger<NewHarvestableEventHandler> logger, PacketIndexes packetIndexes) 
            : base(packetIndexes.NewHarvestableObject)
        {
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        protected override async Task OnActionAsync(AlbionNetworkNewHarvestableEvent value)
        {
            try
            {
                _logger.LogInformation("🎯 PROCESSANDO NewHarvestableEvent: ID {Id}, TypeId {TypeId}, Posição {Position}", 
                    value.Id, value.TypeId, value.Position);

                // Criar Harvestable para o construtor do NewHarvestableEvent
                var harvestable = new Harvestable(
                    value.Id,
                    $"Type_{value.TypeId}",
                    value.Tier,
                    value.Position,
                    1, // count
                    0  // charge
                );

                // Criar evento do jogo para o EventDispatcher
                var gameEvent = new NewHarvestableEvent(harvestable);
                gameEvent.Timestamp = DateTime.UtcNow;

                // Disparar para o EventDispatcher que publicará na fila
                await _eventDispatcher.DispatchEvent(gameEvent);

                _logger.LogInformation("✅ NewHarvestableEvent processado e disparado para fila: ID {Id}", value.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao processar NewHarvestableEvent: {Message}", ex.Message);
            }
        }
    }
} 
