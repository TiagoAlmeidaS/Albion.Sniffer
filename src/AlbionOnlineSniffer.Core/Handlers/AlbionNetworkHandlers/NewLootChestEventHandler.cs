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
    /// Handler para eventos NewLootChest que implementa Albion.Network.EventPacketHandler<T>
    /// Conecta com o EventDispatcher para publicação na fila
    /// </summary>
    public class NewLootChestEventHandler : EventPacketHandler<AlbionNetworkNewLootChestEvent>
    {
        private readonly EventDispatcher _eventDispatcher;
        private readonly ILogger<NewLootChestEventHandler> _logger;

        public NewLootChestEventHandler(EventDispatcher eventDispatcher, ILogger<NewLootChestEventHandler> logger, PacketIndexes packetIndexes) 
            : base(packetIndexes.NewLootChest)
        {
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        protected override async Task OnActionAsync(AlbionNetworkNewLootChestEvent value)
        {
            try
            {
                _logger.LogInformation("🎯 PROCESSANDO NewLootChestEvent: ID {Id}, TypeId {TypeId}, Posição {Position}", 
                    value.Id, value.TypeId, value.Position);

                // Criar LootChest para o construtor do NewLootChestEvent
                var lootChest = new LootChest(
                    value.Id,
                    value.Position,
                    $"Chest_{value.TypeId}",
                    0 // charge
                );

                // Criar evento do jogo para o EventDispatcher
                var gameEvent = new NewLootChestEvent(lootChest);
                gameEvent.Timestamp = DateTime.UtcNow;

                // Disparar para o EventDispatcher que publicará na fila
                await _eventDispatcher.DispatchEvent(gameEvent);

                _logger.LogInformation("✅ NewLootChestEvent processado e disparado para fila: ID {Id}", value.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao processar NewLootChestEvent: {Message}", ex.Message);
            }
        }
    }
} 
