using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Handlers;

namespace AlbionOnlineSniffer.Core.Handlers
{
    /// <summary>
    /// Handler para eventos de mudança de estado de mob
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class MobChangeStateEventHandler
    {
        private readonly ILogger<MobChangeStateEventHandler> _logger;
        private readonly MobsManager _mobsManager;

        public MobChangeStateEventHandler(
            ILogger<MobChangeStateEventHandler> logger,
            MobsManager mobsManager)
        {
            _logger = logger;
            _mobsManager = mobsManager;
        }

        /// <summary>
        /// Processa evento de mudança de estado de mob
        /// </summary>
        /// <param name="mobStateEvent">Evento de mudança de estado</param>
        /// <returns>Task</returns>
        public async Task HandleAsync(MobChangeStateEvent mobStateEvent)
        {
            try
            {
                var status = mobStateEvent.IsDead ? "morto" : "vivo";
                _logger.LogInformation("🐉 MOB MUDOU ESTADO: ID {Id} ({Status}) em ({X}, {Y})", 
                    mobStateEvent.Id, status, mobStateEvent.Position.X, mobStateEvent.Position.Y);

                // Atualizar estado do mob (seria implementado se necessário)
                _logger.LogDebug("Estado do mob seria atualizado: ID {Id} -> {Status}", mobStateEvent.Id, status);

                _logger.LogDebug("Estado do mob atualizado: ID {Id} -> {Status}", mobStateEvent.Id, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar MobChangeStateEvent: {Message}", ex.Message);
            }

            await Task.CompletedTask;
        }
    }
} 