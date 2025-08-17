using Microsoft.AspNetCore.SignalR;
using AlbionOnlineSniffer.Web.Interfaces;
using AlbionOnlineSniffer.Web.Models;
using AlbionOnlineSniffer.Web.Services;

namespace AlbionOnlineSniffer.Web.Hubs
{
    /// <summary>
    /// Hub SignalR para comunicação em tempo real com o cliente
    /// </summary>
    public class SnifferHub : Hub
    {
        private readonly IInMemoryRepository<Packet> _packets;
        private readonly IInMemoryRepository<Event> _events;
        private readonly IInMemoryRepository<LogEntry> _logs;
        private readonly IInMemoryRepository<Session> _sessions;
        private readonly MetricsService _metricsService;
        private readonly HealthCheckService _healthService;
        private readonly ILogger<SnifferHub> _logger;

        public SnifferHub(
            IInMemoryRepository<Packet> packets,
            IInMemoryRepository<Event> events,
            IInMemoryRepository<LogEntry> logs,
            IInMemoryRepository<Session> sessions,
            MetricsService metricsService,
            HealthCheckService healthService,
            ILogger<SnifferHub> logger)
        {
            _packets = packets;
            _events = events;
            _logs = logs;
            _sessions = sessions;
            _metricsService = metricsService;
            _healthService = healthService;
            _logger = logger;
        }

        /// <summary>
        /// Cliente conectado
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            _logger.LogInformation("Cliente conectado: {ConnectionId}", connectionId);

            // Envia dados iniciais para o cliente
            await SendInitialDataAsync();

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Cliente desconectado
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            _logger.LogInformation("Cliente desconectado: {ConnectionId}", connectionId);

            if (exception != null)
            {
                _logger.LogWarning(exception, "Cliente desconectado com exceção: {ConnectionId}", connectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Envia dados iniciais para o cliente
        /// </summary>
        private async Task SendInitialDataAsync()
        {
            try
            {
                // Envia estatísticas dos repositórios
                var packetStats = _packets.GetStats();
                var eventStats = _events.GetStats();
                var logStats = _logs.GetStats();
                var sessionStats = _sessions.GetStats();

                await Clients.Caller.SendAsync("initialData", new
                {
                    repositories = new
                    {
                        packets = packetStats,
                        events = eventStats,
                        logs = logStats,
                        sessions = sessionStats
                    },
                    timestamp = DateTime.UtcNow
                });

                _logger.LogDebug("Dados iniciais enviados para cliente {ConnectionId}", Context.ConnectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar dados iniciais para cliente {ConnectionId}", Context.ConnectionId);
            }
        }

        /// <summary>
        /// Cliente solicita dados paginados de pacotes
        /// </summary>
        public async Task GetPackets(int skip = 0, int take = 100)
        {
            try
            {
                var packets = _packets.GetPaged(skip, take);
                var total = _packets.Count;

                await Clients.Caller.SendAsync("packetsResponse", new
                {
                    packets,
                    total,
                    skip,
                    take,
                    hasMore = skip + take < total
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter pacotes para cliente {ConnectionId}", Context.ConnectionId);
                await Clients.Caller.SendAsync("error", "Erro ao obter pacotes");
            }
        }

        /// <summary>
        /// Cliente solicita dados paginados de eventos
        /// </summary>
        public async Task GetEvents(int skip = 0, int take = 100)
        {
            try
            {
                var events = _events.GetPaged(skip, take);
                var total = _events.Count;

                await Clients.Caller.SendAsync("eventsResponse", new
                {
                    events,
                    total,
                    skip,
                    take,
                    hasMore = skip + take < total
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter eventos para cliente {ConnectionId}", Context.ConnectionId);
                await Clients.Caller.SendAsync("error", "Erro ao obter eventos");
            }
        }

        /// <summary>
        /// Cliente solicita dados paginados de logs
        /// </summary>
        public async Task GetLogs(int skip = 0, int take = 100)
        {
            try
            {
                var logs = _logs.GetPaged(skip, take);
                var total = _logs.Count;

                await Clients.Caller.SendAsync("logsResponse", new
                {
                    logs,
                    total,
                    skip,
                    take,
                    hasMore = skip + take < total
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter logs para cliente {ConnectionId}", Context.ConnectionId);
                await Clients.Caller.SendAsync("error", "Erro ao obter logs");
            }
        }

        /// <summary>
        /// Cliente solicita dados paginados de sessões
        /// </summary>
        public async Task GetSessions(int skip = 0, int take = 100)
        {
            try
            {
                var sessions = _sessions.GetPaged(skip, take);
                var total = _sessions.Count;

                await Clients.Caller.SendAsync("sessionsResponse", new
                {
                    sessions,
                    total,
                    skip,
                    take,
                    hasMore = skip + take < total
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter sessões para cliente {ConnectionId}", Context.ConnectionId);
                await Clients.Caller.SendAsync("error", "Erro ao obter sessões");
            }
        }

        /// <summary>
        /// Cliente solicita métricas
        /// </summary>
        public async Task GetMetrics()
        {
            try
            {
                var metrics = _metricsService.GetMetrics();
                await Clients.Caller.SendAsync("metricsResponse", metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter métricas para cliente {ConnectionId}", Context.ConnectionId);
                await Clients.Caller.SendAsync("error", "Erro ao obter métricas");
            }
        }

        /// <summary>
        /// Cliente solicita status de saúde
        /// </summary>
        public async Task GetHealth()
        {
            try
            {
                var health = _healthService.CheckOverallHealth();
                await Clients.Caller.SendAsync("healthResponse", health);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter status de saúde para cliente {ConnectionId}", Context.ConnectionId);
                await Clients.Caller.SendAsync("error", "Erro ao obter status de saúde");
            }
        }

        /// <summary>
        /// Cliente solicita limpeza de repositórios
        /// </summary>
        public async Task ClearRepositories()
        {
            try
            {
                _packets.Clear();
                _events.Clear();
                _logs.Clear();
                _sessions.Clear();

                await Clients.All.SendAsync("repositoriesCleared", new
                {
                    timestamp = DateTime.UtcNow,
                    message = "Todos os repositórios foram limpos"
                });

                _logger.LogInformation("Repositórios limpos por solicitação do cliente {ConnectionId}", Context.ConnectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao limpar repositórios para cliente {ConnectionId}", Context.ConnectionId);
                await Clients.Caller.SendAsync("error", "Erro ao limpar repositórios");
            }
        }

        /// <summary>
        /// Cliente solicita reset das métricas
        /// </summary>
        public async Task ResetMetrics()
        {
            try
            {
                _metricsService.ResetMetrics();

                await Clients.All.SendAsync("metricsReset", new
                {
                    timestamp = DateTime.UtcNow,
                    message = "Métricas foram resetadas"
                });

                _logger.LogInformation("Métricas resetadas por solicitação do cliente {ConnectionId}", Context.ConnectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao resetar métricas para cliente {ConnectionId}", Context.ConnectionId);
                await Clients.Caller.SendAsync("error", "Erro ao resetar métricas");
            }
        }
    }
}