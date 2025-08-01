using System;
using System.Threading.Tasks;
using Albion.Network;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models;
using AlbionOnlineSniffer.Core.Models.GameObjects;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace AlbionOnlineSniffer.Core.Handlers.AlbionNetworkHandlers
{
    /// <summary>
    /// Handler para eventos NewHarvestablesList que implementa Albion.Network.EventPacketHandler<T>
    /// Conecta com o EventDispatcher para publicação na fila
    /// </summary>
    public class NewHarvestablesListEventHandler : EventPacketHandler<AlbionNetworkNewHarvestablesListEvent>
    {
        private readonly EventDispatcher _eventDispatcher;
        private readonly ILogger<NewHarvestablesListEventHandler> _logger;

        public NewHarvestablesListEventHandler(EventDispatcher eventDispatcher, ILogger<NewHarvestablesListEventHandler> logger, PacketIndexes packetIndexes) 
            : base(packetIndexes.NewHarvestableList)
        {
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        protected override async Task OnActionAsync(AlbionNetworkNewHarvestablesListEvent value)
        {
            try
            {
                _logger.LogInformation("🎯 PROCESSANDO NewHarvestablesListEvent: Count {Count}", 
                    value.Harvestables?.Count ?? 0);

                // Criar evento do jogo para o EventDispatcher
                var gameEvent = new NewHarvestablesListEvent(value.Harvestables ?? new List<Harvestable>());
                gameEvent.Timestamp = DateTime.UtcNow;

                // Disparar para o EventDispatcher que publicará na fila
                await _eventDispatcher.DispatchEvent(gameEvent);

                _logger.LogInformation("✅ NewHarvestablesListEvent processado e disparado para fila: Count {Count}", 
                    value.Harvestables?.Count ?? 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao processar NewHarvestablesListEvent: {Message}", ex.Message);
            }
        }
    }
} 
