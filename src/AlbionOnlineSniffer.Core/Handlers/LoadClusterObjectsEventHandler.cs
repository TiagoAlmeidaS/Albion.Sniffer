using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Handlers;
using AlbionOnlineSniffer.Core.Models;

namespace AlbionOnlineSniffer.Core.Handlers
{
    /// <summary>
    /// Handler para eventos de carregamento de objetos de cluster
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class LoadClusterObjectsEventHandler
    {
        private readonly ILogger<LoadClusterObjectsEventHandler> _logger;
        private readonly LocalPlayerHandler _localPlayerHandler;

        public LoadClusterObjectsEventHandler(
            ILogger<LoadClusterObjectsEventHandler> logger,
            LocalPlayerHandler localPlayerHandler)
        {
            _logger = logger;
            _localPlayerHandler = localPlayerHandler;
        }

        /// <summary>
        /// Processa evento de carregamento de objetos de cluster
        /// </summary>
        /// <param name="clusterObjectsEvent">Evento de objetos de cluster</param>
        /// <returns>Task</returns>
        public async Task HandleAsync(LoadClusterObjectsEvent clusterObjectsEvent)
        {
            try
            {
                if (clusterObjectsEvent.ClusterObjectives != null && clusterObjectsEvent.ClusterObjectives.Count > 0)
                {
                    _logger.LogInformation("🗺️ Cluster Objects carregados: {Count} objetivos", 
                        clusterObjectsEvent.ClusterObjectives.Count);

                    // Atualizar informações do local player sobre objetivos do cluster
                    if (_localPlayerHandler.LocalPlayer != null)
                    {
                        // ClusterObjectives seria uma propriedade do LocalPlayer se implementada
                        _logger.LogDebug("Objetivos do cluster recebidos: {Count}", clusterObjectsEvent.ClusterObjectives?.Count ?? 0);
                    }

                    // Log detalhado dos objetivos
                    foreach (var objective in clusterObjectsEvent.ClusterObjectives.Values)
                    {
                        _logger.LogDebug("Objetivo: ID {Id}, Nome: {Name}, Completo: {IsCompleted}", 
                            objective.Id, objective.Name, objective.IsCompleted);
                    }
                }
                else
                {
                    _logger.LogInformation("🗺️ Cluster Objects: Nenhum objetivo encontrado");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar LoadClusterObjectsEvent: {Message}", ex.Message);
            }

            await Task.CompletedTask;
        }
    }
} 