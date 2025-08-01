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
    /// Handler para eventos MobChangeState que implementa Albion.Network.EventPacketHandler<T>
    /// Conecta com o EventDispatcher para publicação na fila
    /// </summary>
    public class MobChangeStateEventHandler : EventPacketHandler<AlbionNetworkMobChangeStateEvent>
    {
        private readonly EventDispatcher _eventDispatcher;
        private readonly ILogger<MobChangeStateEventHandler> _logger;

        public MobChangeStateEventHandler(EventDispatcher eventDispatcher, ILogger<MobChangeStateEventHandler> logger, PacketIndexes packetIndexes) 
            : base(packetIndexes.MobChangeState)
        {
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        protected override async Task OnActionAsync(AlbionNetworkMobChangeStateEvent value)
        {
            try
            {
                _logger.LogInformation("🎯 PROCESSANDO MobChangeStateEvent: ID {Id}, Position {Position}, IsDead {IsDead}", 
                    value.Id, value.Position, value.IsDead);

                // Criar evento do jogo para o EventDispatcher
                var gameEvent = new MobChangeStateEvent(value.Id, value.Position, value.IsDead);
                gameEvent.Timestamp = DateTime.UtcNow;

                // Disparar para o EventDispatcher que publicará na fila
                await _eventDispatcher.DispatchEvent(gameEvent);

                _logger.LogInformation("✅ MobChangeStateEvent processado e disparado para fila: ID {Id}", value.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao processar MobChangeStateEvent: {Message}", ex.Message);
            }
        }
    }
} 
