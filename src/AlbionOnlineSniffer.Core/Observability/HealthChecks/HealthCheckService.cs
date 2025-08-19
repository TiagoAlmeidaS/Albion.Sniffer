using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using AlbionOnlineSniffer.Core.Pipeline;

namespace AlbionOnlineSniffer.Core.Observability.HealthChecks
{
    /// <summary>
    /// Implementação do serviço de health checks
    /// </summary>
    public class HealthCheckService : IHealthCheckService
    {
        private readonly ILogger<HealthCheckService> _logger;
        private readonly ConcurrentDictionary<string, Func<Task<HealthCheckResult>>> _healthChecks;
        private readonly IServiceProvider _serviceProvider;

        public HealthCheckService(ILogger<HealthCheckService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _healthChecks = new ConcurrentDictionary<string, Func<Task<HealthCheckResult>>>();

            // Registrar health checks padrão
            RegisterDefaultHealthChecks();
        }

        public async Task<HealthReport> RunHealthChecksAsync()
        {
            var startTime = DateTime.UtcNow;
            var results = new List<HealthCheckResult>();

            try
            {
                _logger.LogDebug("Executando {CheckCount} health checks", _healthChecks.Count);

                var tasks = _healthChecks.Select(async kvp =>
                {
                    var checkName = kvp.Key;
                    var checkFunction = kvp.Value;

                    try
                    {
                        var result = await checkFunction();
                        result.Name = checkName;
                        return result;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao executar health check {CheckName}", checkName);
                        return new HealthCheckResult
                        {
                            Name = checkName,
                            Status = HealthStatus.Unhealthy,
                            Description = $"Erro ao executar health check: {ex.Message}",
                            Duration = TimeSpan.Zero,
                            Exception = ex
                        };
                    }
                });

                results = (await Task.WhenAll(tasks)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro crítico ao executar health checks");
                results.Add(new HealthCheckResult
                {
                    Name = "System",
                    Status = HealthStatus.Unhealthy,
                    Description = $"Erro crítico: {ex.Message}",
                    Duration = TimeSpan.Zero,
                    Exception = ex
                });
            }

            var duration = DateTime.UtcNow - startTime;
            var overallStatus = DetermineOverallStatus(results);

            var report = new HealthReport
            {
                Status = overallStatus,
                Timestamp = DateTime.UtcNow,
                Duration = duration,
                Checks = results,
                Summary = GenerateSummary(results, overallStatus)
            };

            _logger.LogInformation("Health checks concluídos: {Status} em {Duration}ms", overallStatus, duration.TotalMilliseconds);

            return report;
        }

        public async Task<HealthCheckResult> RunHealthCheckAsync(string checkName)
        {
            if (!_healthChecks.TryGetValue(checkName, out var checkFunction))
            {
                throw new ArgumentException($"Health check '{checkName}' não encontrado", nameof(checkName));
            }

            var startTime = DateTime.UtcNow;
            try
            {
                var result = await checkFunction();
                result.Name = checkName;
                result.Duration = DateTime.UtcNow - startTime;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar health check {CheckName}", checkName);
                return new HealthCheckResult
                {
                    Name = checkName,
                    Status = HealthStatus.Unhealthy,
                    Description = $"Erro ao executar health check: {ex.Message}",
                    Duration = DateTime.UtcNow - startTime,
                    Exception = ex
                };
            }
        }

        public void RegisterHealthCheck(string name, Func<Task<HealthCheckResult>> checkFunction)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Nome do health check não pode ser vazio", nameof(name));

            if (checkFunction == null)
                throw new ArgumentNullException(nameof(checkFunction));

            _healthChecks.AddOrUpdate(name, checkFunction, (_, _) => checkFunction);
            _logger.LogInformation("Health check registrado: {Name}", name);
        }

        public void UnregisterHealthCheck(string name)
        {
            if (_healthChecks.TryRemove(name, out _))
            {
                _logger.LogInformation("Health check removido: {Name}", name);
            }
        }

        public IEnumerable<string> GetRegisteredHealthChecks()
        {
            return _healthChecks.Keys.ToArray();
        }

        public bool IsSystemHealthy()
        {
            try
            {
                // Executar health checks de forma síncrona para verificação rápida
                var report = RunHealthChecksAsync().GetAwaiter().GetResult();
                return report.Status == HealthStatus.Healthy;
            }
            catch
            {
                return false;
            }
        }

        private void RegisterDefaultHealthChecks()
        {
            // Health check de memória
            RegisterHealthCheck("Memory", async () =>
            {
                var memoryInfo = GC.GetGCMemoryInfo();
                var totalMemory = GC.GetTotalMemory(false);
                var maxMemory = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;

                var memoryUsagePercent = (double)totalMemory / maxMemory * 100;

                var status = memoryUsagePercent switch
                {
                    < 70 => HealthStatus.Healthy,
                    < 85 => HealthStatus.Degraded,
                    _ => HealthStatus.Unhealthy
                };

                return new HealthCheckResult
                {
                    Status = status,
                    Description = $"Uso de memória: {memoryUsagePercent:F1}%",
                    Data = new Dictionary<string, object>
                    {
                        ["TotalMemory"] = totalMemory,
                        ["MaxMemory"] = maxMemory,
                        ["MemoryUsagePercent"] = memoryUsagePercent,
                        ["Gen0Collections"] = GC.CollectionCount(0),
                        ["Gen1Collections"] = GC.CollectionCount(1),
                        ["Gen2Collections"] = GC.CollectionCount(2)
                    },
                    Tags = new List<string> { "memory", "system" }
                };
            });

            // Health check de CPU
            RegisterHealthCheck("CPU", async () =>
            {
                var process = System.Diagnostics.Process.GetCurrentProcess();
                var cpuTime = process.TotalProcessorTime;
                var uptime = DateTime.UtcNow - process.StartTime.ToUniversalTime();

                return new HealthCheckResult
                {
                    Status = HealthStatus.Healthy,
                    Description = $"Processo ativo há {uptime.TotalMinutes:F1} minutos",
                    Data = new Dictionary<string, object>
                    {
                        ["ProcessId"] = process.Id,
                        ["ProcessName"] = process.ProcessName,
                        ["StartTime"] = process.StartTime,
                        ["Uptime"] = uptime,
                        ["CpuTime"] = cpuTime,
                        ["ThreadCount"] = process.Threads.Count,
                        ["HandleCount"] = process.HandleCount
                    },
                    Tags = new List<string> { "cpu", "process", "system" }
                };
            });

            // Health check de pipeline
            RegisterHealthCheck("Pipeline", async () =>
            {
                try
                {
                    var pipeline = _serviceProvider.GetService(typeof(IEventPipeline)) as IEventPipeline;
                    if (pipeline == null)
                    {
                        return new HealthCheckResult
                        {
                            Status = HealthStatus.Unhealthy,
                            Description = "Pipeline não encontrado",
                            Tags = new List<string> { "pipeline", "core", "missing" }
                        };
                    }

                    var metrics = pipeline.GetMetrics();
                    var bufferUsage = pipeline.GetBufferUsagePercentage();

                    var status = bufferUsage switch
                    {
                        < 80 => HealthStatus.Healthy,
                        < 95 => HealthStatus.Degraded,
                        _ => HealthStatus.Unhealthy
                    };

                    return new HealthCheckResult
                    {
                        Status = status,
                        Description = $"Pipeline ativo, buffer: {bufferUsage:F1}%",
                        Data = new Dictionary<string, object>
                        {
                            ["BufferUsagePercent"] = bufferUsage,
                            ["ProcessedEvents"] = metrics.TotalEventsProcessed,
                            ["DroppedEvents"] = metrics.TotalEventsDropped,
                            ["ErrorCount"] = metrics.TotalErrors,
                            ["ProcessingRate"] = metrics.AverageProcessingTimeMs
                        },
                        Tags = new List<string> { "pipeline", "core", "events" }
                    };
                }
                catch (Exception ex)
                {
                    return new HealthCheckResult
                    {
                        Status = HealthStatus.Unhealthy,
                        Description = $"Erro ao verificar pipeline: {ex.Message}",
                        Exception = ex,
                        Tags = new List<string> { "pipeline", "core", "error" }
                    };
                }
            });

            _logger.LogInformation("Health checks padrão registrados: {CheckCount}", _healthChecks.Count);
        }

        private string DetermineOverallStatus(List<HealthCheckResult> results)
        {
            if (!results.Any())
                return HealthStatus.Unknown;

            if (results.Any(r => r.Status == HealthStatus.Unhealthy))
                return HealthStatus.Unhealthy;

            if (results.Any(r => r.Status == HealthStatus.Degraded))
                return HealthStatus.Degraded;

            return HealthStatus.Healthy;
        }

        private string GenerateSummary(List<HealthCheckResult> results, string overallStatus)
        {
            var healthyCount = results.Count(r => r.Status == HealthStatus.Healthy);
            var degradedCount = results.Count(r => r.Status == HealthStatus.Degraded);
            var unhealthyCount = results.Count(r => r.Status == HealthStatus.Unhealthy);

            return $"Sistema {overallStatus.ToLower()}: {healthyCount} saudável, {degradedCount} degradado, {unhealthyCount} não saudável";
        }
    }
}
