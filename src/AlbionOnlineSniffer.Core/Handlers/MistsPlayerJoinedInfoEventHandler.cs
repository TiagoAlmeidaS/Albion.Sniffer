using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Handlers;
using AlbionOnlineSniffer.Core.Models;

namespace AlbionOnlineSniffer.Core.Handlers
{
    /// <summary>
    /// Handler para eventos de informação de jogador entrando nos mists
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class MistsPlayerJoinedInfoEventHandler
    {
        private readonly ILogger<MistsPlayerJoinedInfoEventHandler> _logger;
        private readonly LocalPlayerHandler _localPlayerHandler;

        public MistsPlayerJoinedInfoEventHandler(
            ILogger<MistsPlayerJoinedInfoEventHandler> logger,
            LocalPlayerHandler localPlayerHandler)
        {
            _logger = logger;
            _localPlayerHandler = localPlayerHandler;
        }

        /// <summary>
        /// Processa evento de informação de jogador entrando nos mists
        /// </summary>
        /// <param name="mistsEvent">Evento de mists</param>
        /// <returns>Task</returns>
        public async Task HandleAsync(MistsPlayerJoinedInfoEvent mistsEvent)
        {
            try
            {
                _logger.LogInformation("🌫️ Mists Player Info: TimeCycle = {TimeCycle}", 
                    mistsEvent.TimeCycle.ToString("yyyy-MM-dd HH:mm:ss"));

                // Atualizar informações do local player sobre mists
                if (_localPlayerHandler.LocalPlayer != null)
                {
                    // TimeCycle seria uma propriedade do LocalPlayer se implementada
                    _logger.LogDebug("TimeCycle recebido: {TimeCycle}", mistsEvent.TimeCycle);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar MistsPlayerJoinedInfoEvent: {Message}", ex.Message);
            }

            await Task.CompletedTask;
        }
    }
} 