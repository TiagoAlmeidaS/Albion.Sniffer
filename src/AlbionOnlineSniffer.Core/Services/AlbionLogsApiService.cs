using System;
using System.Collections.Generic;
using System.Linq;
using AlbionOnlineSniffer.Core.Interfaces;

namespace AlbionOnlineSniffer.Core.Services
{
    /// <summary>
    /// Serviço para expor logs via API para a interface web
    /// </summary>
    public class AlbionLogsApiService
    {
        private readonly IAlbionEventLogger _eventLogger;

        public AlbionLogsApiService(IAlbionEventLogger eventLogger)
        {
            _eventLogger = eventLogger;
        }

        #region Logs Gerais

        /// <summary>
        /// Obtém logs gerais com filtros
        /// </summary>
        public LogsResponse<LogEntry> GetLogs(int count = 100, LogLevel? minLevel = null, string? category = null)
        {
            try
            {
                var logs = _eventLogger.GetLogs(count, minLevel);
                
                if (!string.IsNullOrEmpty(category))
                {
                    logs = logs.Where(l => l.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
                }

                return new LogsResponse<LogEntry>
                {
                    Success = true,
                    Data = logs.ToList(),
                    Count = logs.Count(),
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                return new LogsResponse<LogEntry>
                {
                    Success = false,
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        #endregion

        #region Logs de Captura de Rede

        /// <summary>
        /// Obtém logs de captura de rede
        /// </summary>
        public LogsResponse<NetworkCaptureLog> GetNetworkLogs(int count = 100, NetworkCaptureType? type = null)
        {
            try
            {
                var logs = _eventLogger.GetNetworkLogs(count, type);

                return new LogsResponse<NetworkCaptureLog>
                {
                    Success = true,
                    Data = logs.ToList(),
                    Count = logs.Count(),
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                return new LogsResponse<NetworkCaptureLog>
                {
                    Success = false,
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        /// <summary>
        /// Obtém logs de pacotes UDP capturados
        /// </summary>
        public LogsResponse<NetworkCaptureLog> GetUdpPacketLogs(int count = 100)
        {
            return GetNetworkLogs(count, NetworkCaptureType.UdpPacket);
        }

        /// <summary>
        /// Obtém logs de erros de captura
        /// </summary>
        public LogsResponse<NetworkCaptureLog> GetCaptureErrorLogs(int count = 100)
        {
            return GetNetworkLogs(count, NetworkCaptureType.CaptureError);
        }

        /// <summary>
        /// Obtém logs de dispositivos de rede
        /// </summary>
        public LogsResponse<NetworkCaptureLog> GetNetworkDeviceLogs(int count = 100)
        {
            return GetNetworkLogs(count, NetworkCaptureType.NetworkDevice);
        }

        #endregion

        #region Logs de Eventos

        /// <summary>
        /// Obtém logs de eventos
        /// </summary>
        public LogsResponse<EventLog> GetEventLogs(int count = 100, string? eventType = null)
        {
            try
            {
                var logs = _eventLogger.GetEventLogs(count, eventType);

                return new LogsResponse<EventLog>
                {
                    Success = true,
                    Data = logs.ToList(),
                    Count = logs.Count(),
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                return new LogsResponse<EventLog>
                {
                    Success = false,
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        /// <summary>
        /// Obtém logs de eventos processados
        /// </summary>
        public LogsResponse<EventLog> GetProcessedEventLogs(int count = 100, string? eventType = null)
        {
            try
            {
                var logs = _eventLogger.GetEventLogs(count, eventType)
                    .Where(l => string.IsNullOrEmpty(l.Action) || l.Action != "Queued");

                return new LogsResponse<EventLog>
                {
                    Success = true,
                    Data = logs.ToList(),
                    Count = logs.Count(),
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                return new LogsResponse<EventLog>
                {
                    Success = false,
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        /// <summary>
        /// Obtém logs de eventos enviados para fila
        /// </summary>
        public LogsResponse<EventLog> GetQueuedEventLogs(int count = 100, string? eventType = null)
        {
            try
            {
                var logs = _eventLogger.GetEventLogs(count, eventType)
                    .Where(l => l.Action == "Queued");

                return new LogsResponse<EventLog>
                {
                    Success = true,
                    Data = logs.ToList(),
                    Count = logs.Count(),
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                return new LogsResponse<EventLog>
                {
                    Success = false,
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        #endregion

        #region Estatísticas e Métricas

        /// <summary>
        /// Obtém estatísticas dos logs
        /// </summary>
        public LogsResponse<LogStatistics> GetLogStatistics()
        {
            try
            {
                var stats = _eventLogger.GetStatistics();

                return new LogsResponse<LogStatistics>
                {
                    Success = true,
                    Data = new List<LogStatistics> { stats },
                    Count = 1,
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                return new LogsResponse<LogStatistics>
                {
                    Success = false,
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        /// <summary>
        /// Obtém resumo dos logs em tempo real
        /// </summary>
        public LogsSummaryResponse GetLogsSummary()
        {
            try
            {
                var stats = _eventLogger.GetStatistics();
                var recentNetworkLogs = _eventLogger.GetNetworkLogs(10);
                var recentEventLogs = _eventLogger.GetEventLogs(10);

                return new LogsSummaryResponse
                {
                    Success = true,
                    Statistics = stats,
                    RecentNetworkLogs = recentNetworkLogs.Take(5).ToList(),
                    RecentEventLogs = recentEventLogs.Take(5).ToList(),
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                return new LogsSummaryResponse
                {
                    Success = false,
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        #endregion

        #region Limpeza e Manutenção

        /// <summary>
        /// Limpa logs antigos
        /// </summary>
        public LogsResponse<string> CleanupOldLogs()
        {
            try
            {
                _eventLogger.CleanupOldLogs();

                return new LogsResponse<string>
                {
                    Success = true,
                    Data = new List<string> { "Logs antigos limpos com sucesso" },
                    Count = 1,
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                return new LogsResponse<string>
                {
                    Success = false,
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        #endregion
    }

    #region Modelos de Resposta da API

    public class LogsResponse<T>
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public List<T> Data { get; set; } = new();
        public int Count { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class LogsSummaryResponse
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public LogStatistics? Statistics { get; set; }
        public List<NetworkCaptureLog> RecentNetworkLogs { get; set; } = new();
        public List<EventLog> RecentEventLogs { get; set; } = new();
        public DateTime Timestamp { get; set; }
    }

    #endregion
}
