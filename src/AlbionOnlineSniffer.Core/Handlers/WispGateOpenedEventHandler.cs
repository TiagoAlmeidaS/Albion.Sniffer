using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Handlers;

namespace AlbionOnlineSniffer.Core.Handlers
{
    /// <summary>
    /// Handler para eventos de wisp gate aberto
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class WispGateOpenedEventHandler
    {
        private readonly ILogger<WispGateOpenedEventHandler> _logger;
        private readonly GatedWispsManager _gatedWispsManager;

        public WispGateOpenedEventHandler(
            ILogger<WispGateOpenedEventHandler> logger,
            GatedWispsManager gatedWispsManager)
        {
            _logger = logger;
            _gatedWispsManager = gatedWispsManager;
        }

        /// <summary>
        /// Processa evento de wisp gate aberto
        /// </summary>
        /// <param name="wispGateEvent">Evento de wisp gate</param>
        /// <returns>Task</returns>
        public async Task HandleAsync(WispGateOpenedEvent wispGateEvent)
        {
            try
            {
                var status = wispGateEvent.IsCollected ? "coletado" : "aberto";
                _logger.LogInformation("✨ WISP GATE {Status}: ID {Id}", status, wispGateEvent.Id);

                // Atualizar status do wisp gate (seria implementado se necessário)
                _logger.LogDebug("Status do wisp gate seria atualizado: ID {Id} -> {Status}", 
                    wispGateEvent.Id, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar WispGateOpenedEvent: {Message}", ex.Message);
            }

            await Task.CompletedTask;
        }
    }
} 