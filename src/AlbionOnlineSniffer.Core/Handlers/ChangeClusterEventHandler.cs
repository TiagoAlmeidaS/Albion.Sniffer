using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Handlers;
using AlbionOnlineSniffer.Core.Models;

namespace AlbionOnlineSniffer.Core.Handlers
{
    /// <summary>
    /// Handler para eventos de mudança de cluster/mapa
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class ChangeClusterEventHandler
    {
        private readonly ILogger<ChangeClusterEventHandler> _logger;
        private readonly LocalPlayerHandler _localPlayerHandler;
        private readonly PlayersManager _playersManager;
        private readonly HarvestablesManager _harvestablesManager;
        private readonly MobsManager _mobsManager;
        private readonly DungeonsManager _dungeonsManager;
        private readonly FishNodesManager _fishNodesManager;
        private readonly GatedWispsManager _gatedWispsManager;
        private readonly LootChestsManager _lootChestsManager;

        public ChangeClusterEventHandler(
            ILogger<ChangeClusterEventHandler> logger,
            LocalPlayerHandler localPlayerHandler,
            PlayersManager playersManager,
            HarvestablesManager harvestablesManager,
            MobsManager mobsManager,
            DungeonsManager dungeonsManager,
            FishNodesManager fishNodesManager,
            GatedWispsManager gatedWispsManager,
            LootChestsManager lootChestsManager)
        {
            _logger = logger;
            _localPlayerHandler = localPlayerHandler;
            _playersManager = playersManager;
            _harvestablesManager = harvestablesManager;
            _mobsManager = mobsManager;
            _dungeonsManager = dungeonsManager;
            _fishNodesManager = fishNodesManager;
            _gatedWispsManager = gatedWispsManager;
            _lootChestsManager = lootChestsManager;
        }

        /// <summary>
        /// Processa evento de mudança de cluster
        /// </summary>
        /// <param name="clusterEvent">Evento de mudança de cluster</param>
        /// <returns>Task</returns>
        public async Task HandleAsync(ChangeClusterEvent clusterEvent)
        {
            try
            {
                _logger.LogInformation("🗺️ MUDANÇA DE CLUSTER: {OldCluster} -> {NewCluster}", 
                    clusterEvent.OldCluster, clusterEvent.NewCluster);

                // Limpar todos os dados do cluster anterior
                _logger.LogInformation("🧹 LIMPANDO DADOS DO CLUSTER ANTERIOR...");
                
                _playersManager.Clear();
                _mobsManager.Clear();
                _harvestablesManager.Clear();
                _dungeonsManager.Clear();
                _fishNodesManager.Clear();
                _gatedWispsManager.Clear();
                _lootChestsManager.Clear();

                _logger.LogInformation("✅ Dados do cluster anterior limpos com sucesso");

                // Atualizar informações do local player sobre o novo cluster
                if (_localPlayerHandler.LocalPlayer != null)
                {
                    // Cluster seria uma propriedade do LocalPlayer se implementada
                    _logger.LogDebug("Local player mudou para cluster: {NewCluster}", clusterEvent.NewCluster);
                }

                _logger.LogInformation("🔄 Mudança de cluster processada: {OldCluster} -> {NewCluster}", 
                    clusterEvent.OldCluster, clusterEvent.NewCluster);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar ChangeClusterEvent: {Message}", ex.Message);
            }

            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Evento de mudança de cluster
    /// </summary>
    public class ChangeClusterEvent : GameEvent
    {
        public string OldCluster { get; set; } = string.Empty;
        public string NewCluster { get; set; } = string.Empty;

        public ChangeClusterEvent(string oldCluster, string newCluster)
        {
            EventType = "ChangeCluster";
            OldCluster = oldCluster;
            NewCluster = newCluster;
        }
    }
} 