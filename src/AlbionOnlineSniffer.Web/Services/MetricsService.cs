using AlbionOnlineSniffer.Web.Interfaces;

namespace AlbionOnlineSniffer.Web.Services
{
    /// <summary>
    /// Serviço para coletar e expor métricas da aplicação
    /// </summary>
    public class MetricsService
    {
        private readonly IInMemoryRepository<Packet> _packets;
        private readonly IInMemoryRepository<Event> _events;
        private readonly IInMemoryRepository<LogEntry> _logs;
        private readonly IInMemoryRepository<Session> _sessions;
        private readonly ILogger<MetricsService> _logger;

        // Contadores de métricas
        private long _totalPacketsReceived;
        private long _totalEventsProcessed;
        private long _totalLogsCreated;
        private long _totalSessionsCreated;
        private long _totalErrors;
        private long _totalItemsDiscarded;

        // Histogramas de latência
        private readonly List<long> _packetProcessingLatencies = new();
        private readonly List<long> _eventProcessingLatencies = new();
        private readonly object _metricsLock = new();

        public MetricsService(
            IInMemoryRepository<Packet> packets,
            IInMemoryRepository<Event> events,
            IInMemoryRepository<LogEntry> logs,
            IInMemoryRepository<Session> sessions,
            ILogger<MetricsService> logger)
        {
            _packets = packets;
            _events = events;
            _logs = logs;
            _sessions = sessions;
            _logger = logger;
        }

        /// <summary>
        /// Incrementa o contador de pacotes recebidos
        /// </summary>
        public void IncrementPacketsReceived()
        {
            Interlocked.Increment(ref _totalPacketsReceived);
        }

        /// <summary>
        /// Incrementa o contador de eventos processados
        /// </summary>
        public void IncrementEventsProcessed()
        {
            Interlocked.Increment(ref _totalEventsProcessed);
        }

        /// <summary>
        /// Incrementa o contador de logs criados
        /// </summary>
        public void IncrementLogsCreated()
        {
            Interlocked.Increment(ref _totalLogsCreated);
        }

        /// <summary>
        /// Incrementa o contador de sessões criadas
        /// </summary>
        public void IncrementSessionsCreated()
        {
            Interlocked.Increment(ref _totalSessionsCreated);
        }

        /// <summary>
        /// Incrementa o contador de erros
        /// </summary>
        public void IncrementErrors()
        {
            Interlocked.Increment(ref _totalErrors);
        }

        /// <summary>
        /// Incrementa o contador de itens descartados
        /// </summary>
        public void IncrementItemsDiscarded()
        {
            Interlocked.Increment(ref _totalItemsDiscarded);
        }

        /// <summary>
        /// Registra a latência de processamento de um pacote
        /// </summary>
        public void RecordPacketProcessingLatency(long latencyMs)
        {
            lock (_metricsLock)
            {
                _packetProcessingLatencies.Add(latencyMs);
                
                // Mantém apenas os últimos 1000 valores para evitar crescimento infinito
                if (_packetProcessingLatencies.Count > 1000)
                {
                    _packetProcessingLatencies.RemoveAt(0);
                }
            }
        }

        /// <summary>
        /// Registra a latência de processamento de um evento
        /// </summary>
        public void RecordEventProcessingLatency(long latencyMs)
        {
            lock (_metricsLock)
            {
                _eventProcessingLatencies.Add(latencyMs);
                
                // Mantém apenas os últimos 1000 valores para evitar crescimento infinito
                if (_eventProcessingLatencies.Count > 1000)
                {
                    _eventProcessingLatencies.RemoveAt(0);
                }
            }
        }

        /// <summary>
        /// Obtém todas as métricas em formato de dicionário
        /// </summary>
        public Dictionary<string, object> GetMetrics()
        {
            var packetStats = _packets.GetStats();
            var eventStats = _events.GetStats();
            var logStats = _logs.GetStats();
            var sessionStats = _sessions.GetStats();

            var metrics = new Dictionary<string, object>
            {
                // Contadores totais
                ["total_packets_received"] = _totalPacketsReceived,
                ["total_events_processed"] = _totalEventsProcessed,
                ["total_logs_created"] = _totalLogsCreated,
                ["total_sessions_created"] = _totalSessionsCreated,
                ["total_errors"] = _totalErrors,
                ["total_items_discarded"] = _totalItemsDiscarded,

                // Estatísticas dos repositórios
                ["packets_current"] = packetStats.CurrentItems,
                ["packets_max"] = packetStats.MaxItems,
                ["packets_total"] = packetStats.TotalItems,
                ["packets_discarded"] = packetStats.ItemsDiscarded,

                ["events_current"] = eventStats.CurrentItems,
                ["events_max"] = eventStats.MaxItems,
                ["events_total"] = eventStats.TotalItems,
                ["events_discarded"] = eventStats.ItemsDiscarded,

                ["logs_current"] = logStats.CurrentItems,
                ["logs_max"] = logStats.MaxItems,
                ["logs_total"] = logStats.TotalItems,
                ["logs_discarded"] = logStats.ItemsDiscarded,

                ["sessions_current"] = sessionStats.CurrentItems,
                ["sessions_max"] = sessionStats.MaxItems,
                ["sessions_total"] = sessionStats.TotalItems,
                ["sessions_discarded"] = sessionStats.ItemsDiscarded,

                // Timestamps
                ["packets_last_added"] = packetStats.LastItemAdded,
                ["events_last_added"] = eventStats.LastItemAdded,
                ["logs_last_added"] = logStats.LastItemAdded,
                ["sessions_last_added"] = sessionStats.LastItemAdded,

                // Latências (estatísticas)
                ["packet_processing_latency_p50"] = GetPercentile(_packetProcessingLatencies, 50),
                ["packet_processing_latency_p95"] = GetPercentile(_packetProcessingLatencies, 95),
                ["packet_processing_latency_p99"] = GetPercentile(_packetProcessingLatencies, 99),
                ["packet_processing_latency_avg"] = GetAverage(_packetProcessingLatencies),

                ["event_processing_latency_p50"] = GetPercentile(_eventProcessingLatencies, 50),
                ["event_processing_latency_p95"] = GetPercentile(_eventProcessingLatencies, 95),
                ["event_processing_latency_p99"] = GetPercentile(_eventProcessingLatencies, 99),
                ["event_processing_latency_avg"] = GetAverage(_eventProcessingLatencies),

                // Taxas (por segundo)
                ["packets_per_second"] = CalculateRate(_totalPacketsReceived, packetStats.LastItemAdded),
                ["events_per_second"] = CalculateRate(_totalEventsProcessed, eventStats.LastItemAdded),
                ["logs_per_second"] = CalculateRate(_totalLogsCreated, logStats.LastItemAdded),
                ["sessions_per_second"] = CalculateRate(_totalSessionsCreated, sessionStats.LastItemAdded)
            };

            return metrics;
        }

        /// <summary>
        /// Obtém métricas em formato Prometheus
        /// </summary>
        public string GetPrometheusMetrics()
        {
            var metrics = GetMetrics();
            var prometheus = new List<string>();

            foreach (var metric in metrics)
            {
                var value = metric.Value;
                var metricName = metric.Key.ToLowerInvariant().Replace('.', '_');

                if (value is long longValue)
                {
                    prometheus.Add($"# TYPE {metricName} counter");
                    prometheus.Add($"{metricName} {longValue}");
                }
                else if (value is double doubleValue)
                {
                    prometheus.Add($"# TYPE {metricName} gauge");
                    prometheus.Add($"{metricName} {doubleValue:F3}");
                }
                else if (value is DateTime dateTimeValue)
                {
                    var unixTimestamp = ((DateTimeOffset)dateTimeValue).ToUnixTimeSeconds();
                    prometheus.Add($"# TYPE {metricName} gauge");
                    prometheus.Add($"{metricName} {unixTimestamp}");
                }
            }

            return string.Join("\n", prometheus);
        }

        /// <summary>
        /// Calcula o percentil de uma lista de valores
        /// </summary>
        private double GetPercentile(List<long> values, int percentile)
        {
            if (values.Count == 0) return 0;

            lock (_metricsLock)
            {
                var sorted = values.OrderBy(x => x).ToList();
                var index = (int)Math.Ceiling((percentile / 100.0) * sorted.Count) - 1;
                return index >= 0 && index < sorted.Count ? sorted[index] : 0;
            }
        }

        /// <summary>
        /// Calcula a média de uma lista de valores
        /// </summary>
        private double GetAverage(List<long> values)
        {
            if (values.Count == 0) return 0;

            lock (_metricsLock)
            {
                return values.Average();
            }
        }

        /// <summary>
        /// Calcula a taxa por segundo baseada no total e último timestamp
        /// </summary>
        private double CalculateRate(long total, DateTime lastAdded)
        {
            if (lastAdded == DateTime.MinValue) return 0;

            var elapsed = DateTime.UtcNow - lastAdded;
            if (elapsed.TotalSeconds <= 0) return 0;

            return total / elapsed.TotalSeconds;
        }

        /// <summary>
        /// Reseta todas as métricas
        /// </summary>
        public void ResetMetrics()
        {
            Interlocked.Exchange(ref _totalPacketsReceived, 0);
            Interlocked.Exchange(ref _totalEventsProcessed, 0);
            Interlocked.Exchange(ref _totalLogsCreated, 0);
            Interlocked.Exchange(ref _totalSessionsCreated, 0);
            Interlocked.Exchange(ref _totalErrors, 0);
            Interlocked.Exchange(ref _totalItemsDiscarded, 0);

            lock (_metricsLock)
            {
                _packetProcessingLatencies.Clear();
                _eventProcessingLatencies.Clear();
            }

            _logger.LogInformation("Métricas resetadas");
        }
    }
}