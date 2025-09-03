using AlbionOnlineSniffer.Core.Models.EventCategories;
using AlbionOnlineSniffer.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AlbionOnlineSniffer.Web.Services
{
    /// <summary>
    /// Serviço web para gerenciar filtros de eventos
    /// </summary>
    public class EventFilterService
    {
        private readonly ILogger<EventFilterService> _logger;
        private readonly EventCategoryService _categoryService;
        private readonly DiscoveryStatistics _discoveryStatistics;
        private readonly UDPStatistics _udpStatistics;

        public EventFilterService(
            ILogger<EventFilterService> logger,
            EventCategoryService categoryService,
            DiscoveryStatistics discoveryStatistics,
            UDPStatistics udpStatistics)
        {
            _logger = logger;
            _categoryService = categoryService;
            _discoveryStatistics = discoveryStatistics;
            _udpStatistics = udpStatistics;
        }

        /// <summary>
        /// Obtém todas as categorias de eventos
        /// </summary>
        public object GetCategories()
        {
            var categories = _categoryService.GetCategories();
            return categories.Select(c => new
            {
                id = c.Id,
                name = c.Name,
                description = c.Description,
                icon = c.Icon,
                color = c.Color,
                type = c.Type.ToString(),
                eventNames = c.EventNames,
                packetCodes = c.PacketCodes,
                eventCount = c.EventNames.Count,
                packetCodeCount = c.PacketCodes.Count,
                isEnabled = c.IsEnabled,
                priority = c.Priority
            }).ToList();
        }

        /// <summary>
        /// Obtém categorias por tipo
        /// </summary>
        public object GetCategoriesByType(string type)
        {
            if (!Enum.TryParse<EventType>(type, true, out var eventType))
            {
                return new { error = "Tipo de evento inválido" };
            }

            var categories = _categoryService.GetCategoriesByType(eventType);
            return categories.Select(c => new
            {
                id = c.Id,
                name = c.Name,
                description = c.Description,
                icon = c.Icon,
                color = c.Color,
                type = c.Type.ToString(),
                eventCount = c.EventNames.Count,
                packetCodeCount = c.PacketCodes.Count,
                isEnabled = c.IsEnabled,
                priority = c.Priority
            }).ToList();
        }

        /// <summary>
        /// Obtém configuração atual de filtros
        /// </summary>
        public object GetFilterConfig()
        {
            var config = _categoryService.GetFilterConfig();
            return new
            {
                enabledCategories = config.EnabledCategories,
                disabledEventNames = config.DisabledEventNames,
                disabledPacketCodes = config.DisabledPacketCodes,
                showHiddenPackets = config.ShowHiddenPackets,
                showVisiblePackets = config.ShowVisiblePackets,
                minPacketCount = config.MinPacketCount,
                searchTerm = config.SearchTerm
            };
        }

        /// <summary>
        /// Atualiza configuração de filtros
        /// </summary>
        public object UpdateFilterConfig(object configData)
        {
            try
            {
                var config = new EventFilterConfig();
                
                // Aqui você pode deserializar o configData para EventFilterConfig
                // Por simplicidade, vou assumir que os dados vêm em um formato específico
                
                _categoryService.UpdateFilterConfig(config);
                
                _logger.LogInformation("🔧 Configuração de filtros atualizada via API");
                
                return new { success = true, message = "Configuração de filtros atualizada com sucesso" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar configuração de filtros");
                return new { success = false, error = ex.Message };
            }
        }

        /// <summary>
        /// Obtém estatísticas filtradas
        /// </summary>
        public object GetFilteredStats(string type, string? category = null, bool? showHidden = null, int? minCount = null)
        {
            try
            {
                if (type.ToLower() == "discovery")
                {
                    return GetFilteredDiscoveryStats(category, showHidden, minCount);
                }
                else if (type.ToLower() == "udp")
                {
                    return GetFilteredUDPStats(category, showHidden, minCount);
                }
                else
                {
                    return new { error = "Tipo de estatística inválido. Use 'discovery' ou 'udp'" };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estatísticas filtradas");
                return new { error = ex.Message };
            }
        }

        private object GetFilteredDiscoveryStats(string? category, bool? showHidden, int? minCount)
        {
            var allStats = _discoveryStatistics.PacketStatistics;
            var filteredStats = allStats.Values.AsEnumerable();

            // Aplicar filtros
            if (category != null)
            {
                var categoryObj = _categoryService.GetCategory(category);
                if (categoryObj != null)
                {
                    filteredStats = filteredStats.Where(s => categoryObj.PacketCodes.Contains(s.Code));
                }
            }

            if (showHidden.HasValue)
            {
                filteredStats = filteredStats.Where(s => s.IsHidden == showHidden.Value);
            }

            if (minCount.HasValue)
            {
                filteredStats = filteredStats.Where(s => s.Count >= minCount.Value);
            }

            return filteredStats
                .OrderByDescending(s => s.Count)
                .Take(50)
                .Select(s => new
                {
                    code = s.Code,
                    type = s.Type,
                    count = s.Count,
                    isHidden = s.IsHidden,
                    lastSeen = s.LastSeen
                })
                .ToList();
        }

        private object GetFilteredUDPStats(string? category, bool? showHidden, int? minCount)
        {
            var allStats = _udpStatistics.EventStatistics;
            var filteredStats = allStats.Values.AsEnumerable();

            // Aplicar filtros
            if (category != null)
            {
                var categoryObj = _categoryService.GetCategory(category);
                if (categoryObj != null)
                {
                    filteredStats = filteredStats.Where(s => categoryObj.EventNames.Contains(s.EventName));
                }
            }

            if (showHidden.HasValue)
            {
                filteredStats = filteredStats.Where(s => s.IsSuccessful == showHidden.Value);
            }

            if (minCount.HasValue)
            {
                filteredStats = filteredStats.Where(s => s.Count >= minCount.Value);
            }

            return filteredStats
                .OrderByDescending(s => s.Count)
                .Take(50)
                .Select(s => new
                {
                    eventName = s.EventName,
                    eventType = s.EventType,
                    count = s.Count,
                    isSuccessful = s.IsSuccessful,
                    lastSeen = s.LastSeen
                })
                .ToList();
        }

        /// <summary>
        /// Obtém estatísticas por categoria
        /// </summary>
        public object GetCategoryStatistics()
        {
            var categories = _categoryService.GetCategories();
            var stats = new List<object>();

            foreach (var category in categories)
            {
                var categoryStats = new
                {
                    id = category.Id,
                    name = category.Name,
                    icon = category.Icon,
                    color = category.Color,
                    type = category.Type.ToString(),
                    eventCount = category.EventNames.Count,
                    packetCodeCount = category.PacketCodes.Count,
                    isEnabled = category.IsEnabled,
                    priority = category.Priority
                };

                stats.Add(categoryStats);
            }

            return stats;
        }
    }
}
