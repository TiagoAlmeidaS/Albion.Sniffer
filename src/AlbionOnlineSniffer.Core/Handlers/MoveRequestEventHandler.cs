using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Handlers;

namespace AlbionOnlineSniffer.Core.Handlers
{
    /// <summary>
    /// Handler para eventos de request de movimento
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class MoveRequestEventHandler
    {
        private readonly ILogger<MoveRequestEventHandler> _logger;
        private readonly PlayersManager _playersManager;
        private readonly MobsManager _mobsManager;

        public MoveRequestEventHandler(
            ILogger<MoveRequestEventHandler> logger,
            PlayersManager playersManager,
            MobsManager mobsManager)
        {
            _logger = logger;
            _playersManager = playersManager;
            _mobsManager = mobsManager;
        }

        /// <summary>
        /// Processa evento de request de movimento
        /// </summary>
        /// <param name="moveRequestEvent">Evento de request de movimento</param>
        /// <returns>Task</returns>
        public async Task HandleAsync(MoveRequestEvent moveRequestEvent)
        {
            try
            {
                var status = moveRequestEvent.IsMoving ? "movendo" : "parado";
                _logger.LogDebug("🚶 REQUEST DE MOVIMENTO: ID {Id} ({Status}) para ({X}, {Y})", 
                    moveRequestEvent.Id, status, moveRequestEvent.Position.X, moveRequestEvent.Position.Y);

                // Verificar se é um player ou mob
                var player = _playersManager.GetPlayer(moveRequestEvent.Id);
                if (player != null)
                {
                    _logger.LogDebug("Player {PlayerName} solicitou movimento para ({X}, {Y})", 
                        player.Name, moveRequestEvent.Position.X, moveRequestEvent.Position.Y);
                }
                else
                {
                    _logger.LogDebug("Mob/Entidade {Id} solicitou movimento para ({X}, {Y})", 
                        moveRequestEvent.Id, moveRequestEvent.Position.X, moveRequestEvent.Position.Y);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar MoveRequestEvent: {Message}", ex.Message);
            }

            await Task.CompletedTask;
        }
    }
} 