using AlbionOnlineSniffer.Core.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace AlbionOnlineSniffer.Web.Services
{
    /// <summary>
    /// Serviço web que expõe estatísticas do DiscoveryService para a interface web
    /// </summary>
    public class DiscoveryWebStatisticsService : IHostedService, IDisposable
    {
        private readonly ILogger<DiscoveryWebStatisticsService> _logger;
        private readonly DiscoveryStatistics _discoveryStatistics;
        private readonly Timer _updateTimer;
        private readonly ConcurrentDictionary<string, object> _cachedStats = new();
        private bool _disposed = false;

        public DiscoveryWebStatisticsService(ILogger<DiscoveryWebStatisticsService> logger, DiscoveryStatistics discoveryStatistics)
        {
            _logger = logger;
            _discoveryStatistics = discoveryStatistics;
            
            // Atualizar cache a cada 2 segundos
            _updateTimer = new Timer(UpdateCache, null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🔍 DiscoveryWebStatisticsService iniciado");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🔍 DiscoveryWebStatisticsService parado");
            return Task.CompletedTask;
        }

        private void UpdateCache(object? state)
        {
            try
            {
                // Usar dados reais do DiscoveryStatistics
                var stats = _discoveryStatistics.GetWebStats();
                _cachedStats["current"] = stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar cache de estatísticas");
            }
        }

        public object GetCurrentStats()
        {
            return _cachedStats.TryGetValue("current", out var stats) ? stats : _discoveryStatistics.GetWebStats();
        }

        public object GetTopPackets(int limit = 10)
        {
            return _discoveryStatistics.GetTopPacketsForWeb(limit);
        }

        public object GetTopTypes(int limit = 5)
        {
            return _discoveryStatistics.GetTopTypesForWeb(limit);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _updateTimer?.Dispose();
            }
        }
    }
}
