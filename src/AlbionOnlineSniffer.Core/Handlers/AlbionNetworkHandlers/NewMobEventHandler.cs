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
    /// Handler para eventos NewMob que implementa Albion.Network.EventPacketHandler<T>
    /// Conecta com o EventDispatcher para publicação na fila
    /// </summary>
    public class NewMobEventHandler : EventPacketHandler<AlbionNetworkNewMobEvent>
    {
        private readonly EventDispatcher _eventDispatcher;
        private readonly ILogger<NewMobEventHandler> _logger;

        public NewMobEventHandler(EventDispatcher eventDispatcher, ILogger<NewMobEventHandler> logger, PacketIndexes packetIndexes) 
            : base(packetIndexes.NewMobEvent)
        {
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        protected override async Task OnActionAsync(AlbionNetworkNewMobEvent value)
        {
            try
            {
                _logger.LogInformation("🎯 PROCESSANDO NewMobEvent: ID {Id}, TypeId {TypeId}, Posição {Position}", 
                    value.Id, value.TypeId, value.Position);

                // Criar Mob para o construtor do NewMobEvent
                var mobInfo = new MobInfo { Id = value.TypeId, Tier = 0, Type = "Unknown" };
                var mob = new Mob(
                    value.Id,
                    value.TypeId,
                    value.Position,
                    value.Charge,
                    mobInfo,
                    value.Health
                );

                // Criar evento do jogo para o EventDispatcher
                var gameEvent = new NewMobEvent(mob);
                gameEvent.Timestamp = DateTime.UtcNow;

                // Disparar para o EventDispatcher que publicará na fila
                await _eventDispatcher.DispatchEvent(gameEvent);

                _logger.LogInformation("✅ NewMobEvent processado e disparado para fila: ID {Id}", value.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao processar NewMobEvent: {Message}", ex.Message);
            }
        }
    }
} 
