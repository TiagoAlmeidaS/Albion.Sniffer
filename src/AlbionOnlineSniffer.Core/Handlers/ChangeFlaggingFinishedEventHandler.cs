using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Handlers;

namespace AlbionOnlineSniffer.Core.Handlers
{
    /// <summary>
    /// Handler para eventos de mudança de flagging finalizada
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class ChangeFlaggingFinishedEventHandler
    {
        private readonly ILogger<ChangeFlaggingFinishedEventHandler> _logger;
        private readonly PlayersManager _playersManager;

        public ChangeFlaggingFinishedEventHandler(
            ILogger<ChangeFlaggingFinishedEventHandler> logger,
            PlayersManager playersManager)
        {
            _logger = logger;
            _playersManager = playersManager;
        }

        /// <summary>
        /// Processa evento de mudança de flagging finalizada
        /// </summary>
        /// <param name="flaggingEvent">Evento de flagging</param>
        /// <returns>Task</returns>
        public async Task HandleAsync(ChangeFlaggingFinishedEvent flaggingEvent)
        {
            try
            {
                _logger.LogInformation("🏁 FLAGGING FINALIZADA: Player {Id}, Faction: {Faction}", 
                    flaggingEvent.Id, flaggingEvent.Faction);

                // Atualizar faction do player
                var player = _playersManager.GetPlayer(flaggingEvent.Id);
                if (player != null)
                {
                    player.Faction = flaggingEvent.Faction;
                    _logger.LogDebug("Faction atualizada para player {PlayerName}: {Faction}", 
                        player.Name, flaggingEvent.Faction);
                }
                else
                {
                    _logger.LogWarning("Player {Id} não encontrado para atualizar faction", flaggingEvent.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar ChangeFlaggingFinishedEvent: {Message}", ex.Message);
            }

            await Task.CompletedTask;
        }
    }
} 