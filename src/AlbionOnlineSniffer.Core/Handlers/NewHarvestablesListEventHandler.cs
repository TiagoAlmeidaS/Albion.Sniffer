using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Handlers;

namespace AlbionOnlineSniffer.Core.Handlers
{
    /// <summary>
    /// Handler para eventos de nova lista de harvestables
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class NewHarvestablesListEventHandler
    {
        private readonly ILogger<NewHarvestablesListEventHandler> _logger;
        private readonly HarvestablesManager _harvestablesManager;

        public NewHarvestablesListEventHandler(
            ILogger<NewHarvestablesListEventHandler> logger,
            HarvestablesManager harvestablesManager)
        {
            _logger = logger;
            _harvestablesManager = harvestablesManager;
        }

        /// <summary>
        /// Processa evento de nova lista de harvestables
        /// </summary>
        /// <param name="harvestablesListEvent">Evento de lista de harvestables</param>
        /// <returns>Task</returns>
        public async Task HandleAsync(NewHarvestablesListEvent harvestablesListEvent)
        {
            try
            {
                _logger.LogInformation("🌿 NOVA LISTA DE HARVESTABLES: {Count} recursos", 
                    harvestablesListEvent.Harvestables?.Count ?? 0);

                // Processar cada harvestable da lista
                if (harvestablesListEvent.Harvestables != null)
                {
                    foreach (var harvestable in harvestablesListEvent.Harvestables)
                    {
                        _logger.LogDebug("Harvestable: ID {Id}, Tipo: {Type}, Tier: {Tier}, Count: {Count} em ({X}, {Y})", 
                            harvestable.Id, harvestable.Type, harvestable.Tier, harvestable.Count, 
                            harvestable.Position.X, harvestable.Position.Y);
                    }
                }

                _logger.LogDebug("Lista de harvestables processada: {Count} recursos", 
                    harvestablesListEvent.Harvestables?.Count ?? 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar NewHarvestablesListEvent: {Message}", ex.Message);
            }

            await Task.CompletedTask;
        }
    }
} 