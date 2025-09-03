using Microsoft.AspNetCore.Mvc;
using AlbionOnlineSniffer.Web.Services;
using AlbionOnlineSniffer.Core.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AlbionOnlineSniffer.Web.Controllers
{
    /// <summary>
    /// Controller para análise geral de eventos UDP e discovery
    /// </summary>
    [ApiController]
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly EventFilterService _filterService;
        private readonly UDPStatistics _udpStats;
        private readonly DiscoveryStatistics _discoveryStats;
        private readonly ILogger<EventsController> _logger;

        public EventsController(
            EventFilterService filterService, 
            UDPStatistics udpStats,
            DiscoveryStatistics discoveryStats,
            ILogger<EventsController> logger)
        {
            _filterService = filterService;
            _udpStats = udpStats;
            _discoveryStats = discoveryStats;
            _logger = logger;
        }

        /// <summary>
        /// Obtém configuração de filtros
        /// </summary>
        [HttpGet("filters")]
        public IActionResult GetFilters()
        {
            try
            {
                var config = _filterService.GetFilterConfig();
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter configuração de filtros");
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Atualiza configuração de filtros
        /// </summary>
        [HttpPost("filters")]
        public IActionResult UpdateFilters([FromBody] object config)
        {
            try
            {
                var result = _filterService.UpdateFilterConfig(config);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar configuração de filtros");
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Obtém estatísticas por tipo de evento
        /// </summary>
        [HttpGet("stats/{type}")]
        public IActionResult GetStats(string type, [FromQuery] string? category = null, [FromQuery] bool? showHidden = null, [FromQuery] int? minCount = null)
        {
            try
            {
                var stats = _filterService.GetFilteredStats(type, category, showHidden, minCount);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estatísticas para tipo {Type}", type);
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Obtém estatísticas gerais de UDP
        /// </summary>
        [HttpGet("udp/stats")]
        public IActionResult GetUDPStats()
        {
            try
            {
                var stats = _udpStats.GetWebStats();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estatísticas UDP");
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Obtém top eventos UDP
        /// </summary>
        [HttpGet("udp/top-events")]
        public IActionResult GetTopUDPEvents([FromQuery] int limit = 10)
        {
            try
            {
                var events = _udpStats.GetTopEventsForWeb(limit);
                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter top eventos UDP");
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Obtém estatísticas gerais de Discovery
        /// </summary>
        [HttpGet("discovery/stats")]
        public IActionResult GetDiscoveryStats()
        {
            try
            {
                var stats = _discoveryStats.GetWebStats();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estatísticas Discovery");
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Obtém top packets Discovery
        /// </summary>
        [HttpGet("discovery/top-packets")]
        public IActionResult GetTopDiscoveryPackets([FromQuery] int limit = 10)
        {
            try
            {
                var packets = _discoveryStats.GetTopPacketsForWeb(limit);
                return Ok(packets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter top packets Discovery");
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Obtém categorias de eventos
        /// </summary>
        [HttpGet("categories")]
        public IActionResult GetCategories()
        {
            try
            {
                var categories = _filterService.GetCategories();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter categorias");
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Obtém categorias por tipo
        /// </summary>
        [HttpGet("categories/{type}")]
        public IActionResult GetCategoriesByType(string type)
        {
            try
            {
                var categories = _filterService.GetCategoriesByType(type);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter categorias para tipo {Type}", type);
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Obtém estatísticas por categoria
        /// </summary>
        [HttpGet("category-stats")]
        public IActionResult GetCategoryStatistics()
        {
            try
            {
                var stats = _filterService.GetCategoryStatistics();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estatísticas por categoria");
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Endpoint de teste para simular eventos
        /// </summary>
        [HttpPost("test")]
        public IActionResult TestEvent([FromBody] TestEventRequest request)
        {
            try
            {
                _logger.LogInformation("🧪 Teste de evento recebido: {EventType} - {Opcode} - {ParamCount} parâmetros", 
                    request.EventType, request.Opcode, request.ParamCount);

                // Simular registro de evento para teste
                _udpStats.RecordEvent(
                    request.EventType,
                    request.Category ?? "Test",
                    true,
                    new Dictionary<string, object>
                    {
                        ["timestamp"] = DateTimeOffset.UtcNow,
                        ["eventType"] = request.EventType,
                        ["category"] = request.Category ?? "Test",
                        ["opcode"] = request.Opcode,
                        ["paramCount"] = request.ParamCount,
                        ["isTest"] = true
                    }
                );

                return Ok(new { 
                    message = "Evento de teste registrado com sucesso",
                    timestamp = DateTimeOffset.UtcNow,
                    eventType = request.EventType,
                    opcode = request.Opcode,
                    paramCount = request.ParamCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar evento de teste");
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Obtém estatísticas avançadas com filtros
        /// </summary>
        [HttpGet("advanced-stats/{type}")]
        public IActionResult GetAdvancedStats(string type, [FromQuery] int? minOpcode = null, [FromQuery] int? maxOpcode = null, [FromQuery] string? searchTerm = null)
        {
            try
            {
                var stats = _filterService.GetAdvancedStats(type, minOpcode, maxOpcode, searchTerm);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estatísticas avançadas para tipo {Type}", type);
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Obtém distribuição de opcodes
        /// </summary>
        [HttpGet("opcode-distribution/{type}")]
        public IActionResult GetOpcodeDistribution(string type)
        {
            try
            {
                var distribution = _filterService.GetOpcodeDistribution(type);
                return Ok(distribution);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter distribuição de opcodes para tipo {Type}", type);
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Obtém métricas de performance
        /// </summary>
        [HttpGet("performance")]
        public IActionResult GetPerformanceMetrics()
        {
            try
            {
                var metrics = _filterService.GetPerformanceMetrics();
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter métricas de performance");
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Obtém informações de debug do sistema
        /// </summary>
        [HttpGet("debug")]
        public IActionResult GetDebugInfo()
        {
            try
            {
                var debugInfo = new
                {
                    timestamp = DateTimeOffset.UtcNow,
                    udpStats = new
                    {
                        totalEvents = _udpStats.GetTotalEventCount(),
                        successCount = _udpStats.GetSuccessCount(),
                        errorCount = _udpStats.GetErrorCount(),
                        categories = _udpStats.GetCategories()
                    },
                    discoveryStats = new
                    {
                        totalPackets = _discoveryStats.GetTotalPacketCount(),
                        successCount = _discoveryStats.GetSuccessCount(),
                        errorCount = _discoveryStats.GetErrorCount()
                    },
                    systemInfo = new
                    {
                        uptime = Environment.TickCount64,
                        memoryUsage = GC.GetTotalMemory(false),
                        processorCount = Environment.ProcessorCount
                    }
                };

                return Ok(debugInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter informações de debug");
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Endpoint para testar eventos e verificar preenchimento de dados
        /// </summary>
        [HttpPost("test-event")]
        public IActionResult TestEventWithDebug([FromBody] TestEventRequest request)
        {
            try
            {
                _logger.LogInformation("🧪 Teste de evento com debug: {EventType} - {Opcode} - {ParamCount} parâmetros", 
                    request.EventType, request.Opcode, request.ParamCount);

                // Simular evento com dados
                var testEventData = new
                {
                    EventType = request.EventType,
                    PacketCode = request.Opcode,
                    Parameters = new object[request.ParamCount],
                    Name = request.EventType,
                    Timestamp = DateTimeOffset.UtcNow,
                    Category = request.Category ?? "Test"
                };

                // Registrar no sistema de estatísticas
                _udpStats.RecordEvent(
                    request.EventType,
                    request.Category ?? "Test",
                    true,
                    new Dictionary<string, object>
                    {
                        ["timestamp"] = DateTimeOffset.UtcNow,
                        ["eventType"] = request.EventType,
                        ["category"] = request.Category ?? "Test",
                        ["opcode"] = request.Opcode,
                        ["paramCount"] = request.ParamCount,
                        ["isTest"] = true,
                        ["details"] = new
                        {
                            Opcode = request.Opcode,
                            ParamCount = request.ParamCount,
                            EventName = request.EventType,
                            Timestamp = DateTimeOffset.UtcNow.ToString(),
                            HasParameters = request.ParamCount > 0
                        }
                    }
                );

                return Ok(new { 
                    message = "Evento de teste registrado com sucesso",
                    timestamp = DateTimeOffset.UtcNow,
                    eventType = request.EventType,
                    opcode = request.Opcode,
                    paramCount = request.ParamCount,
                    testData = testEventData
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar evento de teste com debug");
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Endpoint para verificar status de preenchimento de dados
        /// </summary>
        [HttpGet("data-status")]
        public IActionResult GetDataStatus()
        {
            try
            {
                var udpStats = _udpStats.GetWebStats();
                var discoveryStats = _discoveryStats.GetWebStats();

                var dataStatus = new
                {
                    timestamp = DateTimeOffset.UtcNow,
                    udp = new
                    {
                        totalEvents = udpStats.GetValueOrDefault("totalEvents", 0),
                        hasData = (int)udpStats.GetValueOrDefault("totalEvents", 0) > 0,
                        lastEventTime = udpStats.GetValueOrDefault("lastEventTime", "N/A")
                    },
                    discovery = new
                    {
                        totalPackets = discoveryStats.GetValueOrDefault("totalPackets", 0),
                        hasData = (int)discoveryStats.GetValueOrDefault("totalPackets", 0) > 0,
                        lastPacketTime = discoveryStats.GetValueOrDefault("lastPacketTime", "N/A")
                    },
                    recommendations = GenerateRecommendations(udpStats, discoveryStats)
                };

                return Ok(dataStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter status de dados");
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        private List<string> GenerateRecommendations(Dictionary<string, object> udpStats, Dictionary<string, object> discoveryStats)
        {
            var recommendations = new List<string>();

            var udpTotal = (int)udpStats.GetValueOrDefault("totalEvents", 0);
            var discoveryTotal = (int)discoveryStats.GetValueOrDefault("totalPackets", 0);

            if (udpTotal == 0 && discoveryTotal == 0)
            {
                recommendations.Add("🔍 Nenhum dado detectado. Verifique se o sniffer está capturando packets.");
                recommendations.Add("🔧 Verifique se o EventDispatcher está registrando eventos corretamente.");
                recommendations.Add("📡 Confirme se há tráfego de rede do jogo Albion Online.");
            }
            else if (udpTotal == 0)
            {
                recommendations.Add("⚠️ Eventos UDP não detectados. Verifique integração UDPEventIntegrationService.");
                recommendations.Add("🔧 Confirme se EventDispatcher está enviando eventos UDP.");
            }
            else if (discoveryTotal == 0)
            {
                recommendations.Add("⚠️ Packets Discovery não detectados. Verifique captura de packets.");
                recommendations.Add("🔧 Confirme se DiscoveryStatistics está registrando packets.");
            }

            if (udpTotal > 0 || discoveryTotal > 0)
            {
                recommendations.Add("✅ Sistema funcionando! Dados estão sendo capturados.");
                recommendations.Add("📊 Use os filtros avançados para analisar dados específicos.");
            }

            return recommendations;
        }
    }

    /// <summary>
    /// Modelo para requisição de teste de evento
    /// </summary>
    public class TestEventRequest
    {
        public string EventType { get; set; } = "TestEvent";
        public string Opcode { get; set; } = "999";
        public int ParamCount { get; set; } = 0;
        public string? Category { get; set; }
    }
}