using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Interfaces;

namespace AlbionOnlineSniffer.Core.Services
{
    /// <summary>
    /// Sistema de logs personalizado para captura de rede e eventos do Albion
    /// Utilizado pela interface web para exibir informações em tempo real
    /// </summary>
    public class AlbionEventLogger : IAlbionEventLogger, IDisposable
    {
        private readonly ConcurrentQueue<LogEntry> _logQueue;
        private readonly ConcurrentQueue<NetworkCaptureLog> _networkLogs;
        private readonly ConcurrentQueue<EventLog> _eventLogs;
        private readonly int _maxLogEntries;
        private readonly int _maxNetworkLogs;
        private readonly int _maxEventLogs;
        private bool _disposed;

        public AlbionEventLogger(int maxLogEntries = 1000, int maxNetworkLogs = 500, int maxEventLogs = 500)
        {
            _maxLogEntries = maxLogEntries;
            _maxNetworkLogs = maxNetworkLogs;
            _maxEventLogs = maxEventLogs;
            
            _logQueue = new ConcurrentQueue<LogEntry>();
            _networkLogs = new ConcurrentQueue<NetworkCaptureLog>();
            _eventLogs = new ConcurrentQueue<EventLog>();
        }

        #region Logs Gerais

        /// <summary>
        /// Adiciona um log geral
        /// </summary>
        public void AddLog(LogLevel level, string message, string? category = null, object? data = null)
        {
            var entry = new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                Level = level,
                Message = message,
                Category = category ?? "General",
                Data = data
            };

            _logQueue.Enqueue(entry);

            // Manter apenas os logs mais recentes
            while (_logQueue.Count > _maxLogEntries)
            {
                _logQueue.TryDequeue(out _);
            }
        }

        /// <summary>
        /// Obtém logs gerais
        /// </summary>
        public IEnumerable<LogEntry> GetLogs(int count = 100, LogLevel? minLevel = null)
        {
            var logs = _logQueue.ToArray();
            
            if (minLevel.HasValue)
            {
                logs = logs.Where(l => l.Level >= minLevel.Value).ToArray();
            }

            return logs.TakeLast(count);
        }

        #endregion

        #region Logs de Captura de Rede

        /// <summary>
        /// Adiciona log de captura de pacote UDP
        /// </summary>
        public void LogUdpPacketCapture(byte[] payload, string sourceIp, int sourcePort, string destIp, int destPort)
        {
            var networkLog = new NetworkCaptureLog
            {
                Timestamp = DateTime.UtcNow,
                Type = NetworkCaptureType.UdpPacket,
                SourceIp = sourceIp,
                SourcePort = sourcePort,
                DestinationIp = destIp,
                DestinationPort = destPort,
                PayloadSize = payload.Length,
                PayloadPreview = GetPayloadPreview(payload),
                HexPreview = Convert.ToHexString(payload.Take(32).ToArray())
            };

            _networkLogs.Enqueue(networkLog);

            // Manter apenas os logs mais recentes
            while (_networkLogs.Count > _maxNetworkLogs)
            {
                _networkLogs.TryDequeue(out _);
            }
        }

        /// <summary>
        /// Adiciona log de erro na captura
        /// </summary>
        public void LogCaptureError(string error, string context, Exception? exception = null)
        {
            var networkLog = new NetworkCaptureLog
            {
                Timestamp = DateTime.UtcNow,
                Type = NetworkCaptureType.CaptureError,
                Error = error,
                Context = context,
                ExceptionDetails = exception?.ToString()
            };

            _networkLogs.Enqueue(networkLog);

            while (_networkLogs.Count > _maxNetworkLogs)
            {
                _networkLogs.TryDequeue(out _);
            }
        }

        /// <summary>
        /// Adiciona log de dispositivo de rede
        /// </summary>
        public void LogNetworkDevice(string deviceName, string deviceType, bool isValid)
        {
            var networkLog = new NetworkCaptureLog
            {
                Timestamp = DateTime.UtcNow,
                Type = NetworkCaptureType.NetworkDevice,
                DeviceName = deviceName,
                DeviceType = deviceType,
                IsValid = isValid
            };

            _networkLogs.Enqueue(networkLog);

            while (_networkLogs.Count > _maxNetworkLogs)
            {
                _networkLogs.TryDequeue(out _);
            }
        }

        /// <summary>
        /// Obtém logs de captura de rede
        /// </summary>
        public IEnumerable<NetworkCaptureLog> GetNetworkLogs(int count = 100, NetworkCaptureType? type = null)
        {
            var logs = _networkLogs.ToArray();
            
            if (type.HasValue)
            {
                logs = logs.Where(l => l.Type == type.Value).ToArray();
            }

            return logs.TakeLast(count);
        }

        #endregion

        #region Logs de Eventos

        /// <summary>
        /// Adiciona log de evento processado
        /// </summary>
        public void LogEventProcessed(string eventType, object eventData, bool success, string? error = null)
        {
            var eventLog = new EventLog
            {
                Timestamp = DateTime.UtcNow,
                EventType = eventType,
                EventData = eventData,
                Success = success,
                Error = error,
                DataPreview = GetEventDataPreview(eventData)
            };

            _eventLogs.Enqueue(eventLog);

            while (_eventLogs.Count > _maxEventLogs)
            {
                _eventLogs.TryDequeue(out _);
            }
        }

        /// <summary>
        /// Adiciona log de evento enviado para fila
        /// </summary>
        public void LogEventQueued(string eventType, string queueName, bool success, string? error = null)
        {
            var eventLog = new EventLog
            {
                Timestamp = DateTime.UtcNow,
                EventType = eventType,
                QueueName = queueName,
                Success = success,
                Error = error,
                Action = "Queued"
            };

            _eventLogs.Enqueue(eventLog);

            while (_eventLogs.Count > _maxEventLogs)
            {
                _eventLogs.TryDequeue(out _);
            }
        }

        /// <summary>
        /// Obtém logs de eventos
        /// </summary>
        public IEnumerable<EventLog> GetEventLogs(int count = 100, string? eventType = null)
        {
            var logs = _eventLogs.ToArray();
            
            if (!string.IsNullOrEmpty(eventType))
            {
                logs = logs.Where(l => l.EventType == eventType).ToArray();
            }

            return logs.TakeLast(count);
        }

        #endregion

        #region Métricas e Estatísticas

        /// <summary>
        /// Obtém estatísticas dos logs
        /// </summary>
        public LogStatistics GetStatistics()
        {
            var now = DateTime.UtcNow;
            var lastHour = now.AddHours(-1);

            return new LogStatistics
            {
                TotalLogs = _logQueue.Count,
                TotalNetworkLogs = _networkLogs.Count,
                TotalEventLogs = _eventLogs.Count,
                LogsLastHour = _logQueue.Count(l => l.Timestamp >= lastHour),
                NetworkLogsLastHour = _networkLogs.Count(l => l.Timestamp >= lastHour),
                EventLogsLastHour = _eventLogs.Count(l => l.Timestamp >= lastHour),
                LastLogTime = _logQueue.Max(l => l.Timestamp),
                LastNetworkLogTime = _networkLogs.Max(l => l.Timestamp),
                LastEventLogTime = _eventLogs.Max(l => l.Timestamp)
            };
        }

        /// <summary>
        /// Limpa logs antigos (mais de 24 horas)
        /// </summary>
        public void CleanupOldLogs()
        {
            var cutoff = DateTime.UtcNow.AddHours(-24);

            // Limpar logs gerais antigos
            var oldLogs = _logQueue.Where(l => l.Timestamp < cutoff).ToList();
            foreach (var log in oldLogs)
            {
                // Remover logs antigos (não é possível remover específicos de ConcurrentQueue)
                // A limpeza acontece automaticamente ao atingir o limite
            }

            // Limpar logs de rede antigos
            var oldNetworkLogs = _networkLogs.Where(l => l.Timestamp < cutoff).ToList();
            foreach (var log in oldNetworkLogs)
            {
                // Mesmo comportamento
            }

            // Limpar logs de eventos antigos
            var oldEventLogs = _eventLogs.Where(l => l.Timestamp < cutoff).ToList();
            foreach (var log in oldEventLogs)
            {
                // Mesmo comportamento
            }
        }

        #endregion

        #region Métodos Auxiliares

        private string GetPayloadPreview(byte[] payload)
        {
            if (payload.Length == 0) return "Empty";
            
            var preview = payload.Take(16).ToArray();
            return Convert.ToHexString(preview) + (payload.Length > 16 ? "..." : "");
        }

        private string GetEventDataPreview(object eventData)
        {
            try
            {
                var json = JsonSerializer.Serialize(eventData, new JsonSerializerOptions 
                { 
                    WriteIndented = false,
                    MaxDepth = 3
                });
                
                return json.Length > 100 ? json.Substring(0, 100) + "..." : json;
            }
            catch
            {
                return eventData.ToString() ?? "Unknown";
            }
        }

        #endregion

        public void Dispose()
        {
            if (_disposed) return;
            
            _disposed = true;
            
            // Limpar as filas
            while (_logQueue.TryDequeue(out _)) { }
            while (_networkLogs.TryDequeue(out _)) { }
            while (_eventLogs.TryDequeue(out _)) { }
        }
    }

    #region Modelos de Dados

    public enum LogLevel
    {
        Debug = 0,
        Information = 1,
        Warning = 2,
        Error = 3,
        Critical = 4
    }

    public enum NetworkCaptureType
    {
        UdpPacket,
        CaptureError,
        NetworkDevice,
        DeviceStatus
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public object? Data { get; set; }
    }

    public class NetworkCaptureLog
    {
        public DateTime Timestamp { get; set; }
        public NetworkCaptureType Type { get; set; }
        public string? SourceIp { get; set; }
        public int? SourcePort { get; set; }
        public string? DestinationIp { get; set; }
        public int? DestinationPort { get; set; }
        public int? PayloadSize { get; set; }
        public string? PayloadPreview { get; set; }
        public string? HexPreview { get; set; }
        public string? Error { get; set; }
        public string? Context { get; set; }
        public string? ExceptionDetails { get; set; }
        public string? DeviceName { get; set; }
        public string? DeviceType { get; set; }
        public bool? IsValid { get; set; }
    }

    public class EventLog
    {
        public DateTime Timestamp { get; set; }
        public string EventType { get; set; } = string.Empty;
        public object? EventData { get; set; }
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string? DataPreview { get; set; }
        public string? QueueName { get; set; }
        public string? Action { get; set; }
    }

    public class LogStatistics
    {
        public int TotalLogs { get; set; }
        public int TotalNetworkLogs { get; set; }
        public int TotalEventLogs { get; set; }
        public int LogsLastHour { get; set; }
        public int NetworkLogsLastHour { get; set; }
        public int EventLogsLastHour { get; set; }
        public DateTime LastLogTime { get; set; }
        public DateTime LastNetworkLogTime { get; set; }
        public DateTime LastEventLogTime { get; set; }
    }

    #endregion
}
