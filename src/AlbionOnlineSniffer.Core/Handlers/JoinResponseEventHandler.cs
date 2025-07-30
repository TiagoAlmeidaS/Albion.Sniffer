using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Handlers;

namespace AlbionOnlineSniffer.Core.Handlers
{
    /// <summary>
    /// Handler para eventos de resposta de join
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class JoinResponseEventHandler
    {
        private readonly ILogger<JoinResponseEventHandler> _logger;

        public JoinResponseEventHandler(ILogger<JoinResponseEventHandler> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Processa evento de resposta de join
        /// </summary>
        /// <param name="joinResponseEvent">Evento de resposta de join</param>
        /// <returns>Task</returns>
        public async Task HandleAsync(JoinResponseEvent joinResponseEvent)
        {
            try
            {
                var status = joinResponseEvent.Success ? "sucesso" : "falha";
                _logger.LogInformation("🎯 RESPOSTA DE JOIN: {Status} - {Message}", 
                    status, joinResponseEvent.Message);

                if (joinResponseEvent.Success)
                {
                    _logger.LogDebug("Join realizado com sucesso: {Message}", joinResponseEvent.Message);
                }
                else
                {
                    _logger.LogWarning("Join falhou: {Message}", joinResponseEvent.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar JoinResponseEvent: {Message}", ex.Message);
            }

            await Task.CompletedTask;
        }
    }
} 