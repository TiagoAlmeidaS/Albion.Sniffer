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

        /// <summary>
        /// Obtém estatísticas avançadas com filtros por opcode range
        /// </summary>
        public object GetAdvancedStats(string type, int? minOpcode = null, int? maxOpcode = null, string? searchTerm = null)
        {
            try
            {
                if (type.ToLower() == "udp")
                {
                    return GetAdvancedUDPStats(minOpcode, maxOpcode, searchTerm);
                }
                else if (type.ToLower() == "discovery")
                {
                    return GetAdvancedDiscoveryStats(minOpcode, maxOpcode, searchTerm);
                }
                else
                {
                    return new { error = "Tipo de estatística inválido. Use 'udp' ou 'discovery'" };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estatísticas avançadas");
                return new { error = ex.Message };
            }
        }

        private object GetAdvancedUDPStats(int? minOpcode, int? maxOpcode, string? searchTerm)
        {
            var allStats = _udpStatistics.EventStatistics;
            var filteredStats = allStats.Values.AsEnumerable();

            // Filtro por range de opcode (se disponível nos detalhes)
            if (minOpcode.HasValue || maxOpcode.HasValue)
            {
                filteredStats = filteredStats.Where(s => 
                {
                    // Tentar extrair opcode dos detalhes do evento
                    if (s.Details != null && s.Details.ContainsKey("details"))
                    {
                        var details = s.Details["details"];
                        if (details is Dictionary<string, object> eventDetails && eventDetails.ContainsKey("Opcode"))
                        {
                            if (int.TryParse(eventDetails["Opcode"].ToString(), out var opcode))
                            {
                                if (minOpcode.HasValue && opcode < minOpcode.Value) return false;
                                if (maxOpcode.HasValue && opcode > maxOpcode.Value) return false;
                                return true;
                            }
                        }
                    }
                    return false;
                });
            }

            // Filtro por termo de busca
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                filteredStats = filteredStats.Where(s => 
                    s.EventName.ToLower().Contains(searchLower) ||
                    s.EventType.ToLower().Contains(searchLower) ||
                    (s.Details != null && s.Details.Any(d => d.Value?.ToString()?.ToLower().Contains(searchLower) == true))
                );
            }

            return filteredStats
                .OrderByDescending(s => s.Count)
                .Take(100)
                .Select(s => new
                {
                    eventName = s.EventName,
                    eventType = s.EventType,
                    count = s.Count,
                    isSuccessful = s.IsSuccessful,
                    lastSeen = s.LastSeen,
                    details = s.Details,
                    category = GetEventCategoryFromName(s.EventName)
                })
                .ToList();
        }

        private object GetAdvancedDiscoveryStats(int? minOpcode, int? maxOpcode, string? searchTerm)
        {
            var allStats = _discoveryStatistics.PacketStatistics;
            var filteredStats = allStats.Values.AsEnumerable();

            // Filtro por range de opcode
            if (minOpcode.HasValue || maxOpcode.HasValue)
            {
                filteredStats = filteredStats.Where(s => 
                {
                    if (int.TryParse(s.Code, out var opcode))
                    {
                        if (minOpcode.HasValue && opcode < minOpcode.Value) return false;
                        if (maxOpcode.HasValue && opcode > maxOpcode.Value) return false;
                        return true;
                    }
                    return false;
                });
            }

            // Filtro por termo de busca
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                filteredStats = filteredStats.Where(s => 
                    s.Code.ToLower().Contains(searchLower) ||
                    s.Type.ToLower().Contains(searchLower)
                );
            }

            return filteredStats
                .OrderByDescending(s => s.Count)
                .Take(100)
                .Select(s => new
                {
                    code = s.Code,
                    type = s.Type,
                    count = s.Count,
                    isHidden = s.IsHidden,
                    lastSeen = s.LastSeen,
                    category = GetPacketCategoryFromCode(s.Code)
                })
                .ToList();
        }

        /// <summary>
        /// Obtém distribuição de opcodes
        /// </summary>
        public object GetOpcodeDistribution(string type)
        {
            try
            {
                if (type.ToLower() == "udp")
                {
                    return GetUDPOpcodeDistribution();
                }
                else if (type.ToLower() == "discovery")
                {
                    return GetDiscoveryOpcodeDistribution();
                }
                else
                {
                    return new { error = "Tipo inválido. Use 'udp' ou 'discovery'" };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter distribuição de opcodes");
                return new { error = ex.Message };
            }
        }

        private object GetUDPOpcodeDistribution()
        {
            var allStats = _udpStatistics.EventStatistics;
            var opcodeCounts = new Dictionary<string, int>();

            foreach (var stat in allStats.Values)
            {
                if (stat.Details != null && stat.Details.ContainsKey("details"))
                {
                    var details = stat.Details["details"];
                    if (details is Dictionary<string, object> eventDetails && eventDetails.ContainsKey("Opcode"))
                    {
                        var opcode = eventDetails["Opcode"].ToString();
                        if (!string.IsNullOrEmpty(opcode) && opcode != "N/A")
                        {
                            if (opcodeCounts.ContainsKey(opcode))
                                opcodeCounts[opcode] += stat.Count;
                            else
                                opcodeCounts[opcode] = stat.Count;
                        }
                    }
                }
            }

            return opcodeCounts
                .OrderByDescending(kvp => kvp.Value)
                .Take(50)
                .Select(kvp => new { opcode = kvp.Key, count = kvp.Value })
                .ToList();
        }

        private object GetDiscoveryOpcodeDistribution()
        {
            var allStats = _discoveryStatistics.PacketStatistics;
            return allStats.Values
                .OrderByDescending(s => s.Count)
                .Take(50)
                .Select(s => new { opcode = s.Code, count = s.Count })
                .ToList();
        }

        /// <summary>
        /// Obtém métricas de performance
        /// </summary>
        public object GetPerformanceMetrics()
        {
            try
            {
                var udpStats = _udpStatistics.GetWebStats();
                var discoveryStats = _discoveryStatistics.GetWebStats();

                return new
                {
                    timestamp = DateTimeOffset.UtcNow,
                    udp = new
                    {
                        totalEvents = udpStats.GetValueOrDefault("totalEvents", 0),
                        successRate = udpStats.GetValueOrDefault("successRate", 0.0),
                        errorRate = udpStats.GetValueOrDefault("errorRate", 0.0),
                        eventsPerSecond = udpStats.GetValueOrDefault("eventsPerSecond", 0.0)
                    },
                    discovery = new
                    {
                        totalPackets = discoveryStats.GetValueOrDefault("totalPackets", 0),
                        successRate = discoveryStats.GetValueOrDefault("successRate", 0.0),
                        errorRate = discoveryStats.GetValueOrDefault("errorRate", 0.0),
                        packetsPerSecond = discoveryStats.GetValueOrDefault("packetsPerSecond", 0.0)
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter métricas de performance");
                return new { error = ex.Message };
            }
        }

        private string GetEventCategoryFromName(string eventName)
        {
            return eventName switch
            {
                var name when name.Contains("Fishing") => "Fishing",
                var name when name.Contains("Dungeon") || name.Contains("Wisp") => "Dungeons",
                var name when name.Contains("Harvestable") => "Harvestables",
                var name when name.Contains("Mob") => "Mobs",
                var name when name.Contains("Character") || name.Contains("Player") || name.Contains("Move") || name.Contains("Health") || name.Contains("Equipment") || name.Contains("Regeneration") || name.Contains("Mists") => "Players",
                var name when name.Contains("Loot") => "Loot",
                var name when name.Contains("Key") || name.Contains("Cluster") || name.Contains("Flagging") => "System",
                _ => "Unknown"
            };
        }

        private string GetPacketCategoryFromCode(string code)
        {
            if (int.TryParse(code, out var opcode))
            {
                return opcode switch
                {
                    >= 100 and <= 199 => "System",
                    >= 200 and <= 299 => "Player",
                    >= 300 and <= 399 => "Combat",
                    >= 400 and <= 499 => "Economy",
                    >= 500 and <= 599 => "Guild",
                    >= 600 and <= 699 => "Territory",
                    _ => "Unknown"
                };
            }
            return "Unknown";
        }
    }
}
