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
    /// Handler para eventos MistsPlayerJoinedInfo que implementa Albion.Network.EventPacketHandler<T>
    /// Conecta com o EventDispatcher para publicação na fila
    /// </summary>
    public class MistsPlayerJoinedInfoEventHandler : EventPacketHandler<AlbionNetworkMistsPlayerJoinedInfoEvent>
    {
        private readonly EventDispatcher _eventDispatcher;
        private readonly ILogger<MistsPlayerJoinedInfoEventHandler> _logger;

        public MistsPlayerJoinedInfoEventHandler(EventDispatcher eventDispatcher, ILogger<MistsPlayerJoinedInfoEventHandler> logger, PacketIndexes packetIndexes) 
            : base(packetIndexes.MistsPlayerJoinedInfo)
        {
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        protected override async Task OnActionAsync(AlbionNetworkMistsPlayerJoinedInfoEvent value)
        {
            try
            {
                _logger.LogInformation("🎯 PROCESSANDO MistsPlayerJoinedInfoEvent: PlayerId {PlayerId}, GuildId {GuildId}, AllianceId {AllianceId}", 
                    value.PlayerId, value.GuildId, value.AllianceId);

                // Criar evento do jogo para o EventDispatcher
                var gameEvent = new MistsPlayerJoinedInfoEvent(DateTime.UtcNow); // TODO: Implementar parsing correto do timeCycle
                gameEvent.Timestamp = DateTime.UtcNow;

                // Disparar para o EventDispatcher que publicará na fila
                await _eventDispatcher.DispatchEvent(gameEvent);

                _logger.LogInformation("✅ MistsPlayerJoinedInfoEvent processado e disparado para fila: PlayerId {PlayerId}", value.PlayerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao processar MistsPlayerJoinedInfoEvent: {Message}", ex.Message);
            }
        }
    }
} 
