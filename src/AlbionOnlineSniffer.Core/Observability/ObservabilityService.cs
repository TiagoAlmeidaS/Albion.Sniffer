using Microsoft.Extensions.Logging;
using System.Diagnostics;
using AlbionOnlineSniffer.Core.Observability.Metrics;
using AlbionOnlineSniffer.Core.Observability.HealthChecks;
using AlbionOnlineSniffer.Core.Observability.Tracing;

namespace AlbionOnlineSniffer.Core.Observability
{
    /// <summary>
    /// Serviço principal de observabilidade para o Albion.Sniffer
    /// Coordena métricas, traces, logs e health checks
    /// </summary>
    public class ObservabilityService : IObservabilityService, IDisposable
    {
        private readonly ILogger<ObservabilityService> _logger;
        private readonly IMetricsCollector _metricsCollector;
        private readonly IHealthCheckService _healthCheckService;
        private readonly ITracingService _tracingService;
        private readonly Timer _metricsTimer;
        private readonly Timer _healthCheckTimer;
        private bool _disposed;

        public ObservabilityService(
            ILogger<ObservabilityService> logger,
            IMetricsCollector metricsCollector,
            IHealthCheckService healthCheckService,
            ITracingService tracingService)
        {
            _logger = logger;
            _metricsCollector = metricsCollector;
            _healthCheckService = healthCheckService;
            _tracingService = tracingService;

            // Timer para coleta automática de métricas do sistema
            _metricsTimer = new Timer(CollectSystemMetrics, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));

            // Timer para health checks automáticos
            _healthCheckTimer = new Timer(RunPeriodicHealthChecks, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

            _logger.LogInformation("ObservabilityService inicializado");
        }

        public async Task InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Inicializando sistema de observabilidade...");

                // Inicializar tracing
                await _tracingService.InitializeAsync();

                // Registrar métricas iniciais
                RecordInitialMetrics();

                // Executar health check inicial
                var initialHealth = await _healthCheckService.RunHealthChecksAsync();
                _logger.LogInformation("Health check inicial: {Status}", initialHealth.Status);

                _logger.LogInformation("Sistema de observabilidade inicializado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inicializar sistema de observabilidade");
                throw;
            }
        }

        public Activity? StartTrace(string operationName, string? operationId = null)
        {
            try
            {
                var activity = _tracingService.StartTrace(operationName, operationId);
                
                // Registrar métrica de trace iniciado
                _metricsCollector.IncrementCounter("traces.started", 
                    new KeyValuePair<string, object?>("operation", operationName));

                return activity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao iniciar trace {OperationName}", operationName);
                return null;
            }
        }

        public void RecordMetric(string name, double value, params KeyValuePair<string, object?>[] tags)
        {
            try
            {
                _metricsCollector.RecordValue(name, value, tags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar métrica {Name}", name);
            }
        }

        public void IncrementCounter(string name, params KeyValuePair<string, object?>[] tags)
        {
            try
            {
                _metricsCollector.IncrementCounter(name, tags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao incrementar contador {Name}", name);
            }
        }

        public void RecordBusinessEvent(string eventName, string category, params KeyValuePair<string, object?>[] tags)
        {
            try
            {
                // Registrar como métrica de contador
                _metricsCollector.IncrementCounter("business.events", 
                    new KeyValuePair<string, object?>("event", eventName),
                    new KeyValuePair<string, object?>("category", category));

                // Adicionar evento ao trace atual se disponível
                var currentTrace = _tracingService.GetCurrentTrace();
                if (currentTrace != null)
                {
                    _tracingService.AddEvent(eventName, 
                        new KeyValuePair<string, object?>("category", category),
                        new KeyValuePair<string, object?>("timestamp", DateTime.UtcNow));
                }

                _logger.LogDebug("Evento de negócio registrado: {EventName} na categoria {Category}", 
                    eventName, category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar evento de negócio {EventName}", eventName);
            }
        }

        public async Task<HealthReport> CheckHealthAsync()
        {
            try
            {
                var healthCheckReport = await _healthCheckService.RunHealthChecksAsync();
                
                // Converter do HealthReport do HealthCheckService para o HealthReport da interface
                var report = new HealthReport
                {
                    Status = healthCheckReport.Status,
                    Timestamp = healthCheckReport.Timestamp,
                    Duration = healthCheckReport.Duration,
                    Checks = healthCheckReport.Checks.Select(c => new HealthCheckResult
                    {
                        Name = c.Name,
                        Status = c.Status,
                        Description = c.Description,
                        Duration = c.Duration,
                        Data = c.Data,
                        Tags = c.Tags
                    }).ToList(),
                    Metadata = healthCheckReport.Metadata
                };
                
                // Registrar métrica de health check
                _metricsCollector.IncrementCounter("health.checks.executed");
                
                // Registrar status como gauge
                var statusValue = report.Status switch
                {
                    "Healthy" => 1.0,
                    "Degraded" => 0.5,
                    "Unhealthy" => 0.0,
                    _ => 0.0
                };
                _metricsCollector.SetGauge("health.status", statusValue);

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar health checks");
                throw;
            }
        }

        public string GetPrometheusMetrics()
        {
            try
            {
                return _metricsCollector.GetPrometheusMetrics();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter métricas Prometheus");
                return "# Erro ao obter métricas";
            }
        }

        public object GetJsonMetrics()
        {
            try
            {
                return _metricsCollector.GetJsonMetrics();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter métricas JSON");
                return new { error = ex.Message };
            }
        }

        public void ClearOldMetrics(TimeSpan age)
        {
            try
            {
                _metricsCollector.ClearOldMetrics(age);
                _logger.LogDebug("Métricas antigas limpas (idade > {Age})", age);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao limpar métricas antigas");
            }
        }

        private void CollectSystemMetrics(object? state)
        {
            try
            {
                // Métricas de memória
                var memoryInfo = GC.GetGCMemoryInfo();
                var totalMemory = GC.GetTotalMemory(false);
                var memoryUsagePercent = (double)totalMemory / memoryInfo.TotalAvailableMemoryBytes * 100;

                _metricsCollector.SetGauge("system.memory.usage.bytes", totalMemory);
                _metricsCollector.SetGauge("system.memory.usage.percent", memoryUsagePercent);
                _metricsCollector.SetGauge("system.memory.available.bytes", memoryInfo.TotalAvailableMemoryBytes);

                // Métricas de GC
                _metricsCollector.SetGauge("system.gc.collections.gen0", GC.CollectionCount(0));
                _metricsCollector.SetGauge("system.gc.collections.gen1", GC.CollectionCount(1));
                _metricsCollector.SetGauge("system.gc.collections.gen2", GC.CollectionCount(2));

                // Métricas de processo
                var process = System.Diagnostics.Process.GetCurrentProcess();
                _metricsCollector.SetGauge("system.process.threads", process.Threads.Count);
                _metricsCollector.SetGauge("system.process.handles", process.HandleCount);
                _metricsCollector.SetGauge("system.process.uptime.seconds", 
                    (DateTime.UtcNow - process.StartTime.ToUniversalTime()).TotalSeconds);

                _logger.LogDebug("Métricas do sistema coletadas");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao coletar métricas do sistema");
            }
        }

        private async void RunPeriodicHealthChecks(object? state)
        {
            try
            {
                var report = await _healthCheckService.RunHealthChecksAsync();
                
                // Registrar métricas de health check periódico
                _metricsCollector.IncrementCounter("health.checks.periodic");
                
                if (report.Status != "Healthy")
                {
                    _logger.LogWarning("Health check periódico detectou problemas: {Status}", report.Status);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar health checks periódicos");
            }
        }

        private void RecordInitialMetrics()
        {
            try
            {
                // Métricas de inicialização
                _metricsCollector.IncrementCounter("system.startup");
                _metricsCollector.SetGauge("system.startup.timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                // Métricas de versão
                var version = typeof(ObservabilityService).Assembly.GetName().Version;
                if (version != null)
                {
                    _metricsCollector.SetGauge("system.version.major", version.Major);
                    _metricsCollector.SetGauge("system.version.minor", version.Minor);
                    _metricsCollector.SetGauge("system.version.patch", version.Build);
                }

                _logger.LogDebug("Métricas iniciais registradas");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar métricas iniciais");
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _metricsTimer?.Dispose();
                _healthCheckTimer?.Dispose();
                _disposed = true;
            }
        }
    }
}
