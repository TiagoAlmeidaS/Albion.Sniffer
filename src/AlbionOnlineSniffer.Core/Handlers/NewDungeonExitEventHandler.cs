using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Handlers;

namespace AlbionOnlineSniffer.Core.Handlers
{
    /// <summary>
    /// Handler para eventos de nova saída de dungeon
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class NewDungeonExitEventHandler
    {
        private readonly ILogger<NewDungeonExitEventHandler> _logger;
        private readonly DungeonsManager _dungeonsManager;

        public NewDungeonExitEventHandler(
            ILogger<NewDungeonExitEventHandler> logger,
            DungeonsManager dungeonsManager)
        {
            _logger = logger;
            _dungeonsManager = dungeonsManager;
        }

        /// <summary>
        /// Processa evento de nova saída de dungeon
        /// </summary>
        /// <param name="dungeonExitEvent">Evento de saída de dungeon</param>
        /// <returns>Task</returns>
        public async Task HandleAsync(NewDungeonExitEvent dungeonExitEvent)
        {
            try
            {
                _logger.LogInformation("🏰 NOVA SAÍDA DE DUNGEON: ID {Id} ({Type}, Charges: {Charges}) em ({X}, {Y})", 
                    dungeonExitEvent.Id, dungeonExitEvent.Type, dungeonExitEvent.Charges, 
                    dungeonExitEvent.Position.X, dungeonExitEvent.Position.Y);

                // Adicionar nova saída de dungeon (seria implementado se necessário)
                _logger.LogDebug("Nova saída de dungeon seria adicionada: ID {Id}", dungeonExitEvent.Id);

                _logger.LogDebug("Saída de dungeon adicionada: ID {Id}", dungeonExitEvent.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar NewDungeonExitEvent: {Message}", ex.Message);
            }

            await Task.CompletedTask;
        }
    }
} 