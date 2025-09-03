using AlbionOnlineSniffer.Core.Models.EventCategories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AlbionOnlineSniffer.Core.Services
{
    /// <summary>
    /// Serviço para gerenciar categorias e filtros de eventos
    /// </summary>
    public class EventCategoryService
    {
        private readonly ILogger<EventCategoryService> _logger;
        private readonly List<EventCategory> _categories;
        private readonly EventFilterConfig _filterConfig;

        public EventCategoryService(ILogger<EventCategoryService> logger)
        {
            _logger = logger;
            _categories = InitializeCategories();
            _filterConfig = new EventFilterConfig();
            
            _logger.LogInformation("📂 EventCategoryService inicializado com {Count} categorias", _categories.Count);
        }

        /// <summary>
        /// Inicializa as categorias de eventos baseadas nos handlers existentes
        /// </summary>
        private List<EventCategory> InitializeCategories()
        {
            return new List<EventCategory>
            {
                // 🎣 CATEGORIA: FISHING
                new EventCategory
                {
                    Id = "fishing",
                    Name = "Fishing",
                    Description = "Eventos relacionados à pesca",
                    Icon = "🎣",
                    Color = "#06b6d4",
                    Type = EventType.UDP,
                    EventNames = new List<string>
                    {
                        "StartFishingEvent", "FishingBiteEvent", "FishingFinishEvent", 
                        "FishingMiniGameUpdateEvent", "NewFishingZoneEvent"
                    },
                    Priority = 1
                },

                // 🏰 CATEGORIA: DUNGEONS
                new EventCategory
                {
                    Id = "dungeons",
                    Name = "Dungeons",
                    Description = "Eventos relacionados a dungeons e portais",
                    Icon = "🏰",
                    Color = "#8b5cf6",
                    Type = EventType.UDP,
                    EventNames = new List<string>
                    {
                        "NewDungeonEvent", "WispGateOpenedEvent", "NewGatedWispEvent"
                    },
                    Priority = 2
                },

                // 🌿 CATEGORIA: HARVESTABLES
                new EventCategory
                {
                    Id = "harvestables",
                    Name = "Harvestables",
                    Description = "Eventos relacionados a recursos coletáveis",
                    Icon = "🌿",
                    Color = "#10b981",
                    Type = EventType.UDP,
                    EventNames = new List<string>
                    {
                        "NewHarvestableEvent", "NewHarvestablesListEvent", "HarvestableChangeStateEvent"
                    },
                    Priority = 3
                },

                // 👹 CATEGORIA: MOBS
                new EventCategory
                {
                    Id = "mobs",
                    Name = "Mobs",
                    Description = "Eventos relacionados a criaturas e mobs",
                    Icon = "👹",
                    Color = "#ef4444",
                    Type = EventType.UDP,
                    EventNames = new List<string>
                    {
                        "NewMobEvent", "MobChangeStateEvent"
                    },
                    Priority = 4
                },

                // 👤 CATEGORIA: PLAYERS
                new EventCategory
                {
                    Id = "players",
                    Name = "Players",
                    Description = "Eventos relacionados a jogadores",
                    Icon = "👤",
                    Color = "#3b82f6",
                    Type = EventType.UDP,
                    EventNames = new List<string>
                    {
                        "NewCharacterEvent", "MoveEvent", "LeaveEvent", "MountedEvent",
                        "HealthUpdateEvent", "CharacterEquipmentChangedEvent", "RegenerationChangedEvent",
                        "MistsPlayerJoinedInfoEvent"
                    },
                    Priority = 5
                },

                // 💰 CATEGORIA: LOOT
                new EventCategory
                {
                    Id = "loot",
                    Name = "Loot",
                    Description = "Eventos relacionados a loot e tesouros",
                    Icon = "💰",
                    Color = "#f59e0b",
                    Type = EventType.UDP,
                    EventNames = new List<string>
                    {
                        "NewLootChestEvent"
                    },
                    Priority = 6
                },

                // 🔑 CATEGORIA: SYSTEM
                new EventCategory
                {
                    Id = "system",
                    Name = "System",
                    Description = "Eventos do sistema e sincronização",
                    Icon = "🔑",
                    Color = "#6b7280",
                    Type = EventType.UDP,
                    EventNames = new List<string>
                    {
                        "KeySyncEvent", "LoadClusterObjectsEvent", "ChangeFlaggingFinishedEvent"
                    },
                    Priority = 7
                },

                // 🔍 CATEGORIA: DISCOVERY
                new EventCategory
                {
                    Id = "discovery",
                    Name = "Discovery",
                    Description = "Pacotes descobertos pelo sistema de interceptação",
                    Icon = "🔍",
                    Color = "#ec4899",
                    Type = EventType.Discovery,
                    EventNames = new List<string>
                    {
                        "ResponsePacket", "RequestPacket", "EventPacket"
                    },
                    Priority = 8
                }
            };
        }

        /// <summary>
        /// Obtém todas as categorias
        /// </summary>
        public List<EventCategory> GetCategories()
        {
            return _categories.OrderBy(c => c.Priority).ToList();
        }

        /// <summary>
        /// Obtém categorias por tipo
        /// </summary>
        public List<EventCategory> GetCategoriesByType(EventType type)
        {
            return _categories.Where(c => c.Type == type).OrderBy(c => c.Priority).ToList();
        }

        /// <summary>
        /// Obtém categoria por ID
        /// </summary>
        public EventCategory? GetCategory(string id)
        {
            return _categories.FirstOrDefault(c => c.Id == id);
        }

        /// <summary>
        /// Obtém categoria por nome do evento
        /// </summary>
        public EventCategory? GetCategoryByEventName(string eventName)
        {
            return _categories.FirstOrDefault(c => c.EventNames.Contains(eventName));
        }

        /// <summary>
        /// Obtém categoria por código do pacote
        /// </summary>
        public EventCategory? GetCategoryByPacketCode(int packetCode)
        {
            return _categories.FirstOrDefault(c => c.PacketCodes.Contains(packetCode));
        }

        /// <summary>
        /// Atualiza configuração de filtros
        /// </summary>
        public void UpdateFilterConfig(EventFilterConfig config)
        {
            _filterConfig.EnabledCategories = config.EnabledCategories;
            _filterConfig.DisabledEventNames = config.DisabledEventNames;
            _filterConfig.DisabledPacketCodes = config.DisabledPacketCodes;
            _filterConfig.ShowHiddenPackets = config.ShowHiddenPackets;
            _filterConfig.ShowVisiblePackets = config.ShowVisiblePackets;
            _filterConfig.MinPacketCount = config.MinPacketCount;
            _filterConfig.SearchTerm = config.SearchTerm;

            _logger.LogInformation("🔧 Configuração de filtros atualizada: {EnabledCategories} categorias habilitadas", 
                config.EnabledCategories.Count);
        }

        /// <summary>
        /// Obtém configuração atual de filtros
        /// </summary>
        public EventFilterConfig GetFilterConfig()
        {
            return _filterConfig;
        }

        /// <summary>
        /// Verifica se um evento deve ser exibido baseado nos filtros
        /// </summary>
        public bool ShouldShowEvent(string eventName, int? packetCode = null, bool isHidden = false, int count = 0)
        {
            // Verificar se o evento está desabilitado
            if (_filterConfig.DisabledEventNames.Contains(eventName))
                return false;

            // Verificar se o código do pacote está desabilitado
            if (packetCode.HasValue && _filterConfig.DisabledPacketCodes.Contains(packetCode.Value))
                return false;

            // Verificar contagem mínima
            if (count < _filterConfig.MinPacketCount)
                return false;

            // Verificar visibilidade
            if (isHidden && !_filterConfig.ShowHiddenPackets)
                return false;
            if (!isHidden && !_filterConfig.ShowVisiblePackets)
                return false;

            // Verificar categoria
            var category = GetCategoryByEventName(eventName);
            if (category != null && _filterConfig.EnabledCategories.Any() && !_filterConfig.EnabledCategories.Contains(category.Id))
                return false;

            // Verificar termo de busca
            if (!string.IsNullOrEmpty(_filterConfig.SearchTerm))
            {
                var searchTerm = _filterConfig.SearchTerm.ToLower();
                if (!eventName.ToLower().Contains(searchTerm) && 
                    !category?.Name.ToLower().Contains(searchTerm) == true)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Obtém estatísticas por categoria
        /// </summary>
        public Dictionary<string, object> GetCategoryStatistics()
        {
            var stats = new Dictionary<string, object>();
            
            foreach (var category in _categories)
            {
                stats[category.Id] = new
                {
                    name = category.Name,
                    icon = category.Icon,
                    color = category.Color,
                    type = category.Type.ToString(),
                    eventCount = category.EventNames.Count,
                    packetCodeCount = category.PacketCodes.Count,
                    isEnabled = category.IsEnabled,
                    priority = category.Priority
                };
            }

            return stats;
        }
    }
}
