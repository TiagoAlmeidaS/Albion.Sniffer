using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;

namespace AlbionOnlineSniffer.Core.Observability.Metrics
{
    /// <summary>
    /// Coletor de métricas usando OpenTelemetry e Prometheus
    /// </summary>
    public class PrometheusMetricsCollector : IMetricsCollector, IDisposable
    {
        private readonly ILogger<PrometheusMetricsCollector> _logger;
        private readonly Meter _meter;
        private readonly ConcurrentDictionary<string, Counter<long>> _counters;
        private readonly ConcurrentDictionary<string, Histogram<double>> _histograms;
        private readonly ConcurrentDictionary<string, ObservableGauge<double>> _gauges;
        private readonly ConcurrentDictionary<string, List<double>> _values;
        private readonly Timer _cleanupTimer;

        public PrometheusMetricsCollector(ILogger<PrometheusMetricsCollector> logger)
        {
            _logger = logger;
            _meter = new Meter("AlbionOnlineSniffer", "1.0.0");
            _counters = new ConcurrentDictionary<string, Counter<long>>();
            _histograms = new ConcurrentDictionary<string, Histogram<double>>();
            _gauges = new ConcurrentDictionary<string, ObservableGauge<double>>();
            _values = new ConcurrentDictionary<string, List<double>>();

            // Timer para limpeza automática de métricas antigas
            _cleanupTimer = new Timer(CleanupOldMetrics, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));

            _logger.LogInformation("PrometheusMetricsCollector inicializado");
        }

        public void IncrementCounter(string name, params KeyValuePair<string, object?>[] tags)
        {
            try
            {
                var counter = _counters.GetOrAdd(name, _ => _meter.CreateCounter<long>(name));
                var tagList = tags.Select(t => new KeyValuePair<string, object?>(t.Key, t.Value ?? "null")).ToArray();
                counter.Add(1, tagList);
                
                _logger.LogDebug("Contador incrementado: {Name} com {TagCount} tags", name, tags.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao incrementar contador {Name}", name);
            }
        }

        public void RecordValue(string name, double value, params KeyValuePair<string, object?>[] tags)
        {
            try
            {
                var values = _values.GetOrAdd(name, _ => new List<double>());
                lock (values)
                {
                    values.Add(value);
                    // Manter apenas os últimos 1000 valores para evitar crescimento infinito
                    if (values.Count > 1000)
                    {
                        values.RemoveRange(0, values.Count - 1000);
                    }
                }

                _logger.LogDebug("Valor registrado: {Name} = {Value} com {TagCount} tags", name, value, tags.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar valor {Name}", name);
            }
        }

        public void RecordHistogram(string name, double value, params KeyValuePair<string, object?>[] tags)
        {
            try
            {
                var histogram = _histograms.GetOrAdd(name, _ => _meter.CreateHistogram<double>(name));
                var tagList = tags.Select(t => new KeyValuePair<string, object?>(t.Key, t.Value ?? "null")).ToArray();
                histogram.Record(value, tagList);
                
                _logger.LogDebug("Histograma registrado: {Name} = {Value} com {TagCount} tags", name, value, tags.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar histograma {Name}", name);
            }
        }

        public void SetGauge(string name, double value, params KeyValuePair<string, object?>[] tags)
        {
            try
            {
                var gauge = _gauges.GetOrAdd(name, _ => _meter.CreateObservableGauge(name, () => value));
                
                _logger.LogDebug("Gauge definido: {Name} = {Value} com {TagCount} tags", name, value, tags.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao definir gauge {Name}", name);
            }
        }

        public string GetPrometheusMetrics()
        {
            try
            {
                // Para Prometheus, usamos o formato padrão do OpenTelemetry
                // que é automaticamente exposto via HTTP endpoint
                return "# Prometheus metrics disponíveis via /metrics endpoint\n" +
                       "# Use OpenTelemetry Collector ou Prometheus para coleta";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar métricas Prometheus");
                return "# Erro ao gerar métricas";
            }
        }

        public object GetJsonMetrics()
        {
            try
            {
                var stats = GetStatistics();
                return new
                {
                    timestamp = DateTime.UtcNow,
                    statistics = stats,
                    counters = _counters.Keys.ToArray(),
                    histograms = _histograms.Keys.ToArray(),
                    gauges = _gauges.Keys.ToArray(),
                    values = _values.Keys.ToArray()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar métricas JSON");
                return new { error = ex.Message };
            }
        }

        public void ClearOldMetrics(TimeSpan age)
        {
            try
            {
                var cutoff = DateTime.UtcNow - age;
                var clearedCount = 0;

                // Limpar valores antigos
                foreach (var kvp in _values)
                {
                    lock (kvp.Value)
                    {
                        var oldCount = kvp.Value.Count;
                        kvp.Value.RemoveAll(v => true); // Remove todos por simplicidade
                        clearedCount += oldCount;
                    }
                }

                _logger.LogInformation("Métricas antigas limpas: {ClearedCount} valores removidos", clearedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao limpar métricas antigas");
            }
        }

        public MetricsStatistics GetStatistics()
        {
            try
            {
                var stats = new MetricsStatistics
                {
                    TotalCounters = _counters.Count,
                    TotalHistograms = _histograms.Count,
                    TotalGauges = _gauges.Count,
                    LastUpdate = DateTime.UtcNow
                };

                // Calcular totais dos contadores
                foreach (var kvp in _counters)
                {
                    stats.CounterTotals[kvp.Key] = 0; // OpenTelemetry mantém os valores internamente
                }

                // Calcular médias dos valores
                foreach (var kvp in _values)
                {
                    lock (kvp.Value)
                    {
                        if (kvp.Value.Count > 0)
                        {
                            stats.ValueAverages[kvp.Key] = kvp.Value.Average();
                        }
                    }
                }

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estatísticas");
                return new MetricsStatistics();
            }
        }

        private void CleanupOldMetrics(object? state)
        {
            ClearOldMetrics(TimeSpan.FromHours(1));
        }

        public void Dispose()
        {
            _cleanupTimer?.Dispose();
            _meter?.Dispose();
        }
    }
}
