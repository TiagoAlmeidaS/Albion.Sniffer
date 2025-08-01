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
    /// Handler para eventos ChangeFlaggingFinished que implementa Albion.Network.EventPacketHandler<T>
    /// Conecta com o EventDispatcher para publicação na fila
    /// </summary>
    public class ChangeFlaggingFinishedEventHandler : EventPacketHandler<AlbionNetworkChangeFlaggingFinishedEvent>
    {
        private readonly EventDispatcher _eventDispatcher;
        private readonly ILogger<ChangeFlaggingFinishedEventHandler> _logger;

        public ChangeFlaggingFinishedEventHandler(EventDispatcher eventDispatcher, ILogger<ChangeFlaggingFinishedEventHandler> logger, PacketIndexes packetIndexes) 
            : base(packetIndexes.ChangeFlaggingFinished)
        {
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        protected override async Task OnActionAsync(AlbionNetworkChangeFlaggingFinishedEvent value)
        {
            try
            {
                _logger.LogInformation("🎯 PROCESSANDO ChangeFlaggingFinishedEvent: PlayerId {PlayerId}, IsFlagged {IsFlagged}", 
                    value.PlayerId, value.IsFlagged);

                // Criar evento do jogo para o EventDispatcher
                var gameEvent = new ChangeFlaggingFinishedEvent(value.PlayerId, Faction.NoPVP); // TODO: Implementar parsing correto do faction
                gameEvent.Timestamp = DateTime.UtcNow;

                // Disparar para o EventDispatcher que publicará na fila
                await _eventDispatcher.DispatchEvent(gameEvent);

                _logger.LogInformation("✅ ChangeFlaggingFinishedEvent processado e disparado para fila: PlayerId {PlayerId}", value.PlayerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao processar ChangeFlaggingFinishedEvent: {Message}", ex.Message);
            }
        }
    }
} 
