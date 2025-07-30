using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Handlers;

namespace AlbionOnlineSniffer.Core.Handlers
{
    /// <summary>
    /// Handler para eventos de mudança de estado de harvestable
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class HarvestableChangeStateEventHandler
    {
        private readonly ILogger<HarvestableChangeStateEventHandler> _logger;
        private readonly HarvestablesManager _harvestablesManager;

        public HarvestableChangeStateEventHandler(
            ILogger<HarvestableChangeStateEventHandler> logger,
            HarvestablesManager harvestablesManager)
        {
            _logger = logger;
            _harvestablesManager = harvestablesManager;
        }

        /// <summary>
        /// Processa evento de mudança de estado de harvestable
        /// </summary>
        /// <param name="harvestableStateEvent">Evento de mudança de estado</param>
        /// <returns>Task</returns>
        public async Task HandleAsync(HarvestableChangeStateEvent harvestableStateEvent)
        {
            try
            {
                _logger.LogInformation("🌿 HARVESTABLE MUDOU ESTADO: ID {Id} (Count: {Count}, Charge: {Charge}) em ({X}, {Y})", 
                    harvestableStateEvent.Id, harvestableStateEvent.Count, harvestableStateEvent.Charge, 
                    harvestableStateEvent.Position.X, harvestableStateEvent.Position.Y);

                // Atualizar estado do harvestable (seria implementado se necessário)
                _logger.LogDebug("Estado do harvestable seria atualizado: ID {Id}", harvestableStateEvent.Id);

                _logger.LogDebug("Estado do harvestable atualizado: ID {Id}", harvestableStateEvent.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar HarvestableChangeStateEvent: {Message}", ex.Message);
            }

            await Task.CompletedTask;
        }
    }
} 