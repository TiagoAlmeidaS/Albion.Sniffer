using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Handlers;

namespace AlbionOnlineSniffer.Core.Handlers
{
    /// <summary>
    /// Handler para eventos de saída de jogadores/mobs
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class LeaveEventHandler
    {
        private readonly ILogger<LeaveEventHandler> _logger;
        private readonly PlayersManager _playersManager;
        private readonly MobsManager _mobsManager;
        private readonly DungeonsManager _dungeonsManager;
        private readonly FishNodesManager _fishNodesManager;
        private readonly GatedWispsManager _gatedWispsManager;
        private readonly LootChestsManager _lootChestsManager;

        public LeaveEventHandler(
            ILogger<LeaveEventHandler> logger,
            PlayersManager playersManager,
            MobsManager mobsManager,
            DungeonsManager dungeonsManager,
            FishNodesManager fishNodesManager,
            GatedWispsManager gatedWispsManager,
            LootChestsManager lootChestsManager)
        {
            _logger = logger;
            _playersManager = playersManager;
            _mobsManager = mobsManager;
            _dungeonsManager = dungeonsManager;
            _fishNodesManager = fishNodesManager;
            _gatedWispsManager = gatedWispsManager;
            _lootChestsManager = lootChestsManager;
        }

        /// <summary>
        /// Processa evento de saída de entidade
        /// </summary>
        /// <param name="leaveEvent">Evento de saída</param>
        /// <returns>Task</returns>
        public async Task HandleAsync(LeaveEvent leaveEvent)
        {
            try
            {
                _logger.LogInformation("👋 ENTIDADE SAIU: ID {Id}, Tipo: {Type}", 
                    leaveEvent.Id, leaveEvent.EntityType);

                // Remover entidade baseado no tipo
                switch (leaveEvent.EntityType.ToLowerInvariant())
                {
                    case "player":
                        _playersManager.RemovePlayer(leaveEvent.Id);
                        _logger.LogDebug("Player removido: ID {Id}", leaveEvent.Id);
                        break;
                        
                    case "mob":
                        _mobsManager.RemoveMob(leaveEvent.Id);
                        _logger.LogDebug("Mob removido: ID {Id}", leaveEvent.Id);
                        break;
                        
                    case "dungeon":
                        _dungeonsManager.Remove(leaveEvent.Id);
                        _logger.LogDebug("Dungeon removido: ID {Id}", leaveEvent.Id);
                        break;
                        
                    case "fishnode":
                        _fishNodesManager.Remove(leaveEvent.Id);
                        _logger.LogDebug("Fish Node removido: ID {Id}", leaveEvent.Id);
                        break;
                        
                    case "gatedwisp":
                        _gatedWispsManager.Remove(leaveEvent.Id);
                        _logger.LogDebug("Gated Wisp removido: ID {Id}", leaveEvent.Id);
                        break;
                        
                    case "lootchest":
                        _lootChestsManager.RemoveLootChest(leaveEvent.Id);
                        _logger.LogDebug("Loot Chest removido: ID {Id}", leaveEvent.Id);
                        break;
                        
                    default:
                        _logger.LogWarning("Tipo de entidade desconhecido para remoção: {Type} (ID: {Id})", 
                            leaveEvent.EntityType, leaveEvent.Id);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar LeaveEvent: {Message}", ex.Message);
            }

            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Evento de saída de entidade
    /// </summary>
    public class LeaveEvent : GameEvent
    {
        public int Id { get; set; }
        public string EntityType { get; set; } = string.Empty;

        public LeaveEvent(int id, string entityType)
        {
            EventType = "Leave";
            Id = id;
            EntityType = entityType;
        }
    }
} 