using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Handlers;

namespace AlbionOnlineSniffer.Core.Handlers
{
    /// <summary>
    /// Handler para eventos de mudança na regeneração de vida
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class RegenerationChangedEventHandler
    {
        private readonly ILogger<RegenerationChangedEventHandler> _logger;
        private readonly PlayersManager _playersManager;
        private readonly MobsManager _mobsManager;

        public RegenerationChangedEventHandler(
            ILogger<RegenerationChangedEventHandler> logger,
            PlayersManager playersManager,
            MobsManager mobsManager)
        {
            _logger = logger;
            _playersManager = playersManager;
            _mobsManager = mobsManager;
        }

        /// <summary>
        /// Processa evento de mudança na regeneração de vida
        /// </summary>
        /// <param name="regenerationEvent">Evento de regeneração</param>
        /// <returns>Task</returns>
        public async Task HandleAsync(RegenerationChangedEvent regenerationEvent)
        {
            try
            {
                _logger.LogDebug("💚 Regeneração alterada: ID {Id}, Vida: {CurrentHealth}/{MaxHealth}", 
                    regenerationEvent.Id, regenerationEvent.Health.Value, regenerationEvent.Health.MaxValue);

                // Atualizar regeneração para players
                _playersManager.UpdatePlayerHealth(regenerationEvent.Id, regenerationEvent.Health.Value);

                // Atualizar regeneração para mobs
                _mobsManager.UpdateMobHealth(regenerationEvent.Id, regenerationEvent.Health.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar RegenerationChangedEvent: {Message}", ex.Message);
            }

            await Task.CompletedTask;
        }
    }
} 