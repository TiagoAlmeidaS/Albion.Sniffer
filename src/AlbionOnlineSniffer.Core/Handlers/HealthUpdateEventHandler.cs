using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Handlers;

namespace AlbionOnlineSniffer.Core.Handlers
{
    /// <summary>
    /// Handler para eventos de atualização de vida
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class HealthUpdateEventHandler
    {
        private readonly ILogger<HealthUpdateEventHandler> _logger;
        private readonly PlayersManager _playersManager;
        private readonly MobsManager _mobsManager;

        public HealthUpdateEventHandler(
            ILogger<HealthUpdateEventHandler> logger,
            PlayersManager playersManager,
            MobsManager mobsManager)
        {
            _logger = logger;
            _playersManager = playersManager;
            _mobsManager = mobsManager;
        }

        /// <summary>
        /// Processa evento de atualização de vida
        /// </summary>
        /// <param name="healthUpdateEvent">Evento de atualização de vida</param>
        /// <returns>Task</returns>
        public async Task HandleAsync(HealthUpdateEvent healthUpdateEvent)
        {
            try
            {
                _logger.LogDebug("💚 ATUALIZAÇÃO DE VIDA: ID {Id}, Vida: {Current}/{Max} ({Percent}%)", 
                    healthUpdateEvent.Id, healthUpdateEvent.Health.Value, healthUpdateEvent.Health.MaxValue, 
                    healthUpdateEvent.Health.Percent);

                // Atualizar vida para players
                _playersManager.UpdatePlayerHealth(healthUpdateEvent.Id, healthUpdateEvent.Health.Value);

                // Atualizar vida para mobs
                _mobsManager.UpdateMobHealth(healthUpdateEvent.Id, healthUpdateEvent.Health.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar HealthUpdateEvent: {Message}", ex.Message);
            }

            await Task.CompletedTask;
        }
    }
} 