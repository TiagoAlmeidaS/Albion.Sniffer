using AlbionOnlineSniffer.Core.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace AlbionOnlineSniffer.Web.Services
{
    /// <summary>
    /// Serviço web que expõe estatísticas UDP para a interface web
    /// </summary>
    public class UDPWebStatisticsService : IHostedService, IDisposable
    {
        private readonly ILogger<UDPWebStatisticsService> _logger;
        private readonly UDPStatistics _udpStatistics;
        private readonly Timer _updateTimer;
        private readonly ConcurrentDictionary<string, object> _cachedStats = new();
        private bool _disposed = false;

        public UDPWebStatisticsService(ILogger<UDPWebStatisticsService> logger, UDPStatistics udpStatistics)
        {
            _logger = logger;
            _udpStatistics = udpStatistics;
            _updateTimer = new Timer(UpdateCache, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🌐 UDPWebStatisticsService iniciado");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🌐 UDPWebStatisticsService parado");
            return Task.CompletedTask;
        }

        private void UpdateCache(object? state)
        {
            try
            {
                var stats = _udpStatistics.GetWebStats();
                _cachedStats["current"] = stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar cache de estatísticas UDP");
            }
        }

        public object GetCurrentStats()
        {
            return _cachedStats.TryGetValue("current", out var stats) ? stats : new { error = "Nenhuma estatística disponível" };
        }

        public object GetTopEvents(int limit = 10)
        {
            return _udpStatistics.GetTopEventsForWeb(limit);
        }

        public object GetTopTypes(int limit = 5)
        {
            return _udpStatistics.GetTopTypesForWeb(limit);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _updateTimer?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
