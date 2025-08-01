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
    /// Handler para eventos NewFishingZone que implementa Albion.Network.EventPacketHandler<T>
    /// Conecta com o EventDispatcher para publicação na fila
    /// </summary>
    public class NewFishingZoneEventHandler : EventPacketHandler<AlbionNetworkNewFishingZoneEvent>
    {
        private readonly EventDispatcher _eventDispatcher;
        private readonly ILogger<NewFishingZoneEventHandler> _logger;

        public NewFishingZoneEventHandler(EventDispatcher eventDispatcher, ILogger<NewFishingZoneEventHandler> logger, PacketIndexes packetIndexes) 
            : base(packetIndexes.NewFishingZoneObject)
        {
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        protected override async Task OnActionAsync(AlbionNetworkNewFishingZoneEvent value)
        {
            try
            {
                _logger.LogInformation("🎯 PROCESSANDO NewFishingZoneEvent: ID {Id}, Posição {Position}", 
                    value.Id, value.Position);

                // Criar FishNode para o construtor do NewFishingZoneEvent
                var fishNode = new FishNode(
                    value.Id,
                    value.Position,
                    value.TypeId,
                    0 // respawnCount
                );

                // Criar evento do jogo para o EventDispatcher
                var gameEvent = new NewFishingZoneEvent(fishNode);
                gameEvent.Timestamp = DateTime.UtcNow;

                // Disparar para o EventDispatcher que publicará na fila
                await _eventDispatcher.DispatchEvent(gameEvent);

                _logger.LogInformation("✅ NewFishingZoneEvent processado e disparado para fila: ID {Id}", value.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao processar NewFishingZoneEvent: {Message}", ex.Message);
            }
        }
    }
} 
