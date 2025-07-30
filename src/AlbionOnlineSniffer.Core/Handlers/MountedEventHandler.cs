using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Handlers;

namespace AlbionOnlineSniffer.Core.Handlers
{
    /// <summary>
    /// Handler para eventos de montaria
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class MountedEventHandler
    {
        private readonly ILogger<MountedEventHandler> _logger;
        private readonly PlayersManager _playersManager;

        public MountedEventHandler(
            ILogger<MountedEventHandler> logger,
            PlayersManager playersManager)
        {
            _logger = logger;
            _playersManager = playersManager;
        }

        /// <summary>
        /// Processa evento de montaria
        /// </summary>
        /// <param name="mountedEvent">Evento de montaria</param>
        /// <returns>Task</returns>
        public async Task HandleAsync(MountedEvent mountedEvent)
        {
            try
            {
                var status = mountedEvent.IsMounted ? "montado" : "desmontado";
                _logger.LogInformation("🐎 Player {Id} {Status}", mountedEvent.Id, status);

                // Atualizar status de montaria do player
                var player = _playersManager.GetPlayer(mountedEvent.Id);
                if (player != null)
                {
                    player.IsMounted = mountedEvent.IsMounted;
                    _logger.LogDebug("Status de montaria atualizado para player {PlayerName}", player.Name);
                }
                else
                {
                    _logger.LogWarning("Player {Id} não encontrado para atualizar status de montaria", mountedEvent.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar MountedEvent: {Message}", ex.Message);
            }

            await Task.CompletedTask;
        }
    }
} 