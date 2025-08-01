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
    /// Handler para eventos NewDungeonExit que implementa Albion.Network.EventPacketHandler<T>
    /// Conecta com o EventDispatcher para publicação na fila
    /// </summary>
    public class NewDungeonExitEventHandler : EventPacketHandler<AlbionNetworkNewDungeonExitEvent>
    {
        private readonly EventDispatcher _eventDispatcher;
        private readonly ILogger<NewDungeonExitEventHandler> _logger;

        public NewDungeonExitEventHandler(EventDispatcher eventDispatcher, ILogger<NewDungeonExitEventHandler> logger, PacketIndexes packetIndexes) 
            : base(packetIndexes.NewDungeonExit)
        {
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        protected override async Task OnActionAsync(AlbionNetworkNewDungeonExitEvent value)
        {
            try
            {
                _logger.LogInformation("🎯 PROCESSANDO NewDungeonExitEvent: ID {Id}, TypeId {TypeId}, Posição {Position}", 
                    value.Id, value.TypeId, value.Position);

                // Criar Dungeon para o construtor do NewDungeonExitEvent
                var dungeon = new Dungeon(
                    value.Id,
                    $"DungeonExit_{value.TypeId}", // TODO: Mapear TypeId para string correta
                    value.Position,
                    0 // charges
                );

                // Criar evento do jogo para o EventDispatcher
                var gameEvent = new NewDungeonExitEvent(dungeon);
                gameEvent.Timestamp = DateTime.UtcNow;

                // Disparar para o EventDispatcher que publicará na fila
                await _eventDispatcher.DispatchEvent(gameEvent);

                _logger.LogInformation("✅ NewDungeonExitEvent processado e disparado para fila: ID {Id}", value.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao processar NewDungeonExitEvent: {Message}", ex.Message);
            }
        }
    }
} 
