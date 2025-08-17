using AlbionOnlineSniffer.Web.Interfaces;

namespace AlbionOnlineSniffer.Web.Services
{
    /// <summary>
    /// Serviço para verificar a saúde da aplicação
    /// </summary>
    public class HealthCheckService
    {
        private readonly IInMemoryRepository<Packet> _packets;
        private readonly IInMemoryRepository<Event> _events;
        private readonly IInMemoryRepository<LogEntry> _logs;
        private readonly IInMemoryRepository<Session> _sessions;
        private readonly MetricsService _metricsService;
        private readonly ILogger<HealthCheckService> _logger;

        public HealthCheckService(
            IInMemoryRepository<Packet> packets,
            IInMemoryRepository<Event> events,
            IInMemoryRepository<LogEntry> logs,
            IInMemoryRepository<Session> sessions,
            MetricsService metricsService,
            ILogger<HealthCheckService> logger)
        {
            _packets = packets;
            _events = events;
            _logs = logs;
            _sessions = sessions;
            _metricsService = metricsService;
            _logger = logger;
        }

        /// <summary>
        /// Verifica a saúde geral da aplicação
        /// </summary>
        public HealthStatus CheckOverallHealth()
        {
            var checks = new List<HealthCheck>
            {
                CheckRepositoriesHealth(),
                CheckMemoryHealth(),
                CheckPerformanceHealth(),
                CheckErrorRateHealth()
            };

            var overallStatus = checks.All(c => c.Status == HealthStatus.Healthy) 
                ? HealthStatus.Healthy 
                : checks.Any(c => c.Status == HealthStatus.Unhealthy) 
                    ? HealthStatus.Unhealthy 
                    : HealthStatus.Degraded;

            return new HealthStatus
            {
                Status = overallStatus,
                Timestamp = DateTime.UtcNow,
                Checks = checks,
                Summary = GenerateHealthSummary(checks)
            };
        }

        /// <summary>
        /// Verifica a saúde dos repositórios
        /// </summary>
        private HealthCheck CheckRepositoriesHealth()
        {
            var packetStats = _packets.GetStats();
            var eventStats = _events.GetStats();
            var logStats = _logs.GetStats();
            var sessionStats = _sessions.GetStats();

            var issues = new List<string>();

            // Verifica se os repositórios estão funcionando
            if (packetStats.CurrentItems == 0 && packetStats.TotalItems > 0)
                issues.Add("Repositório de pacotes vazio mas com histórico");

            if (eventStats.CurrentItems == 0 && eventStats.TotalItems > 0)
                issues.Add("Repositório de eventos vazio mas com histórico");

            if (logStats.CurrentItems == 0 && logStats.TotalItems > 0)
                issues.Add("Repositório de logs vazio mas com histórico");

            if (sessionStats.CurrentItems == 0 && sessionStats.TotalItems > 0)
                issues.Add("Repositório de sessões vazio mas com histórico");

            // Verifica se há muitos itens sendo descartados
            if (packetStats.ItemsDiscarded > packetStats.TotalItems * 0.1)
                issues.Add("Muitos pacotes sendo descartados");

            if (eventStats.ItemsDiscarded > eventStats.TotalItems * 0.1)
                issues.Add("Muitos eventos sendo descartados");

            var status = issues.Count == 0 ? HealthStatus.Healthy : HealthStatus.Degraded;

            return new HealthCheck
            {
                Name = "Repositories",
                Status = status,
                Message = issues.Count == 0 ? "Todos os repositórios funcionando normalmente" : string.Join("; ", issues),
                Details = new Dictionary<string, object>
                {
                    ["packets_current"] = packetStats.CurrentItems,
                    ["packets_total"] = packetStats.TotalItems,
                    ["packets_discarded"] = packetStats.ItemsDiscarded,
                    ["events_current"] = eventStats.CurrentItems,
                    ["events_total"] = eventStats.TotalItems,
                    ["events_discarded"] = eventStats.ItemsDiscarded,
                    ["logs_current"] = logStats.CurrentItems,
                    ["logs_total"] = logStats.TotalItems,
                    ["logs_discarded"] = logStats.ItemsDiscarded,
                    ["sessions_current"] = sessionStats.CurrentItems,
                    ["sessions_total"] = sessionStats.TotalItems,
                    ["sessions_discarded"] = sessionStats.ItemsDiscarded
                }
            };
        }

        /// <summary>
        /// Verifica a saúde da memória
        /// </summary>
        private HealthCheck CheckMemoryHealth()
        {
            var packetStats = _packets.GetStats();
            var eventStats = _events.GetStats();
            var logStats = _logs.GetStats();
            var sessionStats = _sessions.GetStats();

            var totalCurrent = packetStats.CurrentItems + eventStats.CurrentItems + logStats.CurrentItems + sessionStats.CurrentItems;
            var totalMax = packetStats.MaxItems + eventStats.MaxItems + logStats.MaxItems + sessionStats.MaxItems;
            var memoryUsagePercent = totalMax > 0 ? (double)totalCurrent / totalMax * 100 : 0;

            var issues = new List<string>();

            if (memoryUsagePercent > 90)
                issues.Add("Uso de memória muito alto (>90%)");

            if (memoryUsagePercent > 80)
                issues.Add("Uso de memória alto (>80%)");

            var status = issues.Count == 0 ? HealthStatus.Healthy : 
                        issues.Any(i => i.Contains(">90%")) ? HealthStatus.Unhealthy : HealthStatus.Degraded;

            return new HealthCheck
            {
                Name = "Memory",
                Status = status,
                Message = issues.Count == 0 ? $"Uso de memória normal ({memoryUsagePercent:F1}%)" : string.Join("; ", issues),
                Details = new Dictionary<string, object>
                {
                    ["memory_usage_percent"] = memoryUsagePercent,
                    ["total_current_items"] = totalCurrent,
                    ["total_max_items"] = totalMax,
                    ["packets_usage"] = (double)packetStats.CurrentItems / packetStats.MaxItems * 100,
                    ["events_usage"] = (double)eventStats.CurrentItems / eventStats.MaxItems * 100,
                    ["logs_usage"] = (double)logStats.CurrentItems / logStats.MaxItems * 100,
                    ["sessions_usage"] = (double)sessionStats.CurrentItems / sessionStats.MaxItems * 100
                }
            };
        }

        /// <summary>
        /// Verifica a saúde do desempenho
        /// </summary>
        private HealthCheck CheckPerformanceHealth()
        {
            var metrics = _metricsService.GetMetrics();
            var issues = new List<string>();

            // Verifica latências
            var packetLatencyP99 = metrics.GetValueOrDefault("packet_processing_latency_p99", 0.0);
            var eventLatencyP99 = metrics.GetValueOrDefault("event_processing_latency_p99", 0.0);

            if (packetLatencyP99 > 250)
                issues.Add($"Latência de processamento de pacotes alta (P99: {packetLatencyP99:F0}ms)");

            if (eventLatencyP99 > 250)
                issues.Add($"Latência de processamento de eventos alta (P99: {eventLatencyP99:F0}ms)");

            // Verifica taxas
            var packetsPerSecond = metrics.GetValueOrDefault("packets_per_second", 0.0);
            var eventsPerSecond = metrics.GetValueOrDefault("events_per_second", 0.0);

            if (packetsPerSecond > 1000)
                issues.Add($"Taxa de pacotes muito alta ({packetsPerSecond:F0}/s)");

            if (eventsPerSecond > 500)
                issues.Add($"Taxa de eventos muito alta ({eventsPerSecond:F0}/s)");

            var status = issues.Count == 0 ? HealthStatus.Healthy : 
                        issues.Any(i => i.Contains("alta")) ? HealthStatus.Degraded : HealthStatus.Healthy;

            return new HealthCheck
            {
                Name = "Performance",
                Status = status,
                Message = issues.Count == 0 ? "Desempenho dentro dos parâmetros normais" : string.Join("; ", issues),
                Details = new Dictionary<string, object>
                {
                    ["packet_latency_p50"] = metrics.GetValueOrDefault("packet_processing_latency_p50", 0.0),
                    ["packet_latency_p95"] = metrics.GetValueOrDefault("packet_processing_latency_p95", 0.0),
                    ["packet_latency_p99"] = packetLatencyP99,
                    ["packet_latency_avg"] = metrics.GetValueOrDefault("packet_processing_latency_avg", 0.0),
                    ["event_latency_p50"] = metrics.GetValueOrDefault("event_processing_latency_p50", 0.0),
                    ["event_latency_p95"] = metrics.GetValueOrDefault("event_processing_latency_p95", 0.0),
                    ["event_latency_p99"] = eventLatencyP99,
                    ["event_latency_avg"] = metrics.GetValueOrDefault("event_processing_latency_avg", 0.0),
                    ["packets_per_second"] = packetsPerSecond,
                    ["events_per_second"] = eventsPerSecond
                }
            };
        }

        /// <summary>
        /// Verifica a saúde da taxa de erros
        /// </summary>
        private HealthCheck CheckErrorRateHealth()
        {
            var metrics = _metricsService.GetMetrics();
            var issues = new List<string>();

            var totalPackets = metrics.GetValueOrDefault("total_packets_received", 0L);
            var totalEvents = metrics.GetValueOrDefault("total_events_processed", 0L);
            var totalErrors = metrics.GetValueOrDefault("total_errors", 0L);

            var totalOperations = totalPackets + totalEvents;
            var errorRate = totalOperations > 0 ? (double)totalErrors / totalOperations * 100 : 0;

            if (errorRate > 5)
                issues.Add($"Taxa de erro muito alta ({errorRate:F2}%)");

            if (errorRate > 1)
                issues.Add($"Taxa de erro alta ({errorRate:F2}%)");

            var status = issues.Count == 0 ? HealthStatus.Healthy : 
                        issues.Any(i => i.Contains("muito alta")) ? HealthStatus.Unhealthy : HealthStatus.Degraded;

            return new HealthCheck
            {
                Name = "ErrorRate",
                Status = status,
                Message = issues.Count == 0 ? $"Taxa de erro normal ({errorRate:F2}%)" : string.Join("; ", issues),
                Details = new Dictionary<string, object>
                {
                    ["total_operations"] = totalOperations,
                    ["total_errors"] = totalErrors,
                    ["error_rate_percent"] = errorRate,
                    ["total_packets"] = totalPackets,
                    ["total_events"] = totalEvents
                }
            };
        }

        /// <summary>
        /// Gera um resumo da saúde
        /// </summary>
        private string GenerateHealthSummary(List<HealthCheck> checks)
        {
            var healthy = checks.Count(c => c.Status == HealthStatus.Healthy);
            var degraded = checks.Count(c => c.Status == HealthStatus.Degraded);
            var unhealthy = checks.Count(c => c.Status == HealthStatus.Unhealthy);

            if (unhealthy > 0)
                return $"❌ {unhealthy} problemas críticos, {degraded} avisos";
            if (degraded > 0)
                return $"⚠️ {degraded} avisos, {healthy} OK";
            
            return $"✅ {healthy} verificações OK";
        }
    }

    /// <summary>
    /// Status de saúde geral
    /// </summary>
    public class HealthStatus
    {
        public HealthStatus Status { get; set; }
        public DateTime Timestamp { get; set; }
        public List<HealthCheck> Checks { get; set; } = new();
        public string Summary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Verificação individual de saúde
    /// </summary>
    public class HealthCheck
    {
        public string Name { get; set; } = string.Empty;
        public HealthStatus Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, object> Details { get; set; } = new();
    }

    /// <summary>
    /// Enum para status de saúde
    /// </summary>
    public enum HealthStatus
    {
        Healthy,
        Degraded,
        Unhealthy
    }
}