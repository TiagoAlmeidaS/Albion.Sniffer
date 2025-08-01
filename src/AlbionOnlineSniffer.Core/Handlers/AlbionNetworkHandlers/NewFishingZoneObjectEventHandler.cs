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
    /// Handler para eventos NewFishingZoneObject que implementa Albion.Network.EventPacketHandler<T>
    /// Conecta com o EventDispatcher para publicação na fila
    /// </summary>
    public class NewFishingZoneObjectEventHandler : EventPacketHandler<AlbionNetworkNewFishingZoneObjectEvent>
    {
        private readonly EventDispatcher _eventDispatcher;
        private readonly ILogger<NewFishingZoneObjectEventHandler> _logger;

        public NewFishingZoneObjectEventHandler(EventDispatcher eventDispatcher, ILogger<NewFishingZoneObjectEventHandler> logger, PacketIndexes packetIndexes) 
            : base(packetIndexes.NewFishingZoneObject)
        {
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        protected override async Task OnActionAsync(AlbionNetworkNewFishingZoneObjectEvent value)
        {
            try
            {
                _logger.LogInformation("🎯 PROCESSANDO NewFishingZoneObjectEvent: ID {Id}, TypeId {TypeId}, Posição {Position}", 
                    value.Id, value.TypeId, value.Position);

                // Criar FishNode para o construtor do NewFishingZoneObjectEvent
                var fishNode = new FishNode(
                    value.Id,
                    value.Position,
                    value.TypeId, // size
                    0 // respawnCount
                );

                // Criar evento do jogo para o EventDispatcher
                var gameEvent = new NewFishingZoneEvent(fishNode);
                gameEvent.Timestamp = DateTime.UtcNow;

                // Disparar para o EventDispatcher que publicará na fila
                await _eventDispatcher.DispatchEvent(gameEvent);

                _logger.LogInformation("✅ NewFishingZoneObjectEvent processado e disparado para fila: ID {Id}", value.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao processar NewFishingZoneObjectEvent: {Message}", ex.Message);
            }
        }
    }
} 
