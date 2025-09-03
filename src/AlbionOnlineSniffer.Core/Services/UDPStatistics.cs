using AlbionOnlineSniffer.Core.Models.EventCategories;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AlbionOnlineSniffer.Core.Services
{
    /// <summary>
    /// Gerencia estatísticas em tempo real dos eventos UDP processados
    /// </summary>
    public class UDPStatistics : IDisposable
    {
        private readonly ConcurrentDictionary<string, UDPEventStats> _eventStats = new();
        private readonly ConcurrentDictionary<string, int> _eventTypes = new();
        private readonly Timer _displayTimer;
        private readonly object _lockObject = new();
        private bool _disposed = false;
        private int _totalEvents = 0;
        private int _successfulEvents = 0;
        private int _failedEvents = 0;
        private DateTime _startTime = DateTime.UtcNow;

        public UDPStatistics()
        {
            _displayTimer = new Timer(DisplayStatistics, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }

        public void RecordEvent(string eventName, string eventType, bool isSuccessful, Dictionary<string, object>? parameters = null)
        {
            Interlocked.Increment(ref _totalEvents);
            if (isSuccessful)
            {
                Interlocked.Increment(ref _successfulEvents);
            }
            else
            {
                Interlocked.Increment(ref _failedEvents);
            }

            _eventStats.AddOrUpdate(eventName,
                _ => new UDPEventStats 
                { 
                    EventName = eventName, 
                    EventType = eventType, 
                    Count = 1, 
                    IsSuccessful = isSuccessful, 
                    LastSeen = DateTimeOffset.UtcNow,
                    Parameters = parameters ?? new Dictionary<string, object>()
                },
                (_, stats) =>
                {
                    stats.Count++;
                    stats.LastSeen = DateTimeOffset.UtcNow;
                    if (parameters != null)
                    {
                        stats.Parameters = parameters;
                    }
                    return stats;
                });

            _eventTypes.AddOrUpdate(eventType, 1, (_, count) => count + 1);
        }

        private void DisplayStatistics(object state)
        {
            if (_disposed) return;

            lock (_lockObject)
            {
                Console.Clear();
                Console.WriteLine("🌐 UDP EVENTS - ESTATÍSTICAS EM TEMPO REAL");
                Console.WriteLine(new string('=', 60));
                Console.WriteLine($"Tempo de Execução: {(DateTime.UtcNow - _startTime):hh\\:mm\\:ss}");
                Console.WriteLine($"Total de Eventos: {_totalEvents}");
                Console.WriteLine($"Eventos Bem-sucedidos: {_successfulEvents} ({(_totalEvents == 0 ? 0 : (double)_successfulEvents / _totalEvents * 100):F2}%)");
                Console.WriteLine($"Eventos Falharam: {_failedEvents} ({(_totalEvents == 0 ? 0 : (double)_failedEvents / _totalEvents * 100):F2}%)");
                Console.WriteLine(new string('=', 60));

                var top10Events = _eventStats.Values
                    .OrderByDescending(e => e.Count)
                    .Take(10)
                    .ToList();

                Console.WriteLine("🏆 TOP 10 EVENTOS MAIS FREQUENTES:");
                Console.WriteLine(new string('-', 60));
                Console.WriteLine($"{"Evento",-25} {"Tipo",-15} {"Count",-8} {"Status",-10} {"Último",-12}");
                Console.WriteLine(new string('-', 60));
                foreach (var evt in top10Events)
                {
                    Console.WriteLine($"{evt.EventName,-25} {evt.EventType,-15} {evt.Count,-8} {(evt.IsSuccessful ? "Sucesso" : "Falha"),-10} {evt.LastSeen.ToString("HH:mm:ss"),-12}");
                }
                Console.WriteLine(new string('-', 60));

                var top5Types = _eventTypes
                    .OrderByDescending(kv => kv.Value)
                    .Take(5)
                    .ToList();

                Console.WriteLine("📈 TOP 5 TIPOS DE EVENTOS:");
                Console.WriteLine(new string('-', 40));
                Console.WriteLine($"{"Tipo",-25} {"Count",-8}");
                Console.WriteLine(new string('-', 40));
                foreach (var type in top5Types)
                {
                    Console.WriteLine($"{type.Key,-25} {type.Value,-8}");
                }
                Console.WriteLine(new string('-', 40));

                Console.WriteLine("🔄 Atualizando a cada 5 segundos... (Ctrl+C para parar)");
                Console.WriteLine(new string('=', 60));
            }
        }

        public object GetWebStats()
        {
            return new
            {
                timestamp = DateTime.UtcNow,
                totalEvents = _totalEvents,
                successfulEvents = _successfulEvents,
                failedEvents = _failedEvents,
                successPercentage = _totalEvents > 0 ? (double)_successfulEvents / _totalEvents * 100 : 0,
                failurePercentage = _totalEvents > 0 ? (double)_failedEvents / _totalEvents * 100 : 0,
                uptime = Uptime,
                topEvents = GetTopEventsForWeb(10),
                topTypes = GetTopTypesForWeb(5)
            };
        }

        public object GetTopEventsForWeb(int limit)
        {
            return _eventStats.Values
                .OrderByDescending(e => e.Count)
                .Take(limit)
                .Select(e => new
                {
                    eventName = e.EventName,
                    eventType = e.EventType,
                    count = e.Count,
                    isSuccessful = e.IsSuccessful,
                    lastSeen = e.LastSeen,
                    parameters = e.Parameters
                })
                .ToList();
        }

        public object GetTopTypesForWeb(int limit)
        {
            return _eventTypes
                .OrderByDescending(kv => kv.Value)
                .Take(limit)
                .Select(kv => new
                {
                    type = kv.Key,
                    count = kv.Value
                })
                .ToList();
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
                    _displayTimer?.Dispose();
                }
                _disposed = true;
            }
        }

        // Propriedades públicas para acesso via web
        public int TotalEvents => _totalEvents;
        public int SuccessfulEvents => _successfulEvents;
        public int FailedEvents => _failedEvents;
        public TimeSpan Uptime => DateTime.UtcNow - _startTime;
        public IReadOnlyDictionary<string, UDPEventStats> EventStatistics => _eventStats;
        public IReadOnlyDictionary<string, int> EventTypes => _eventTypes;

        public class UDPEventStats
        {
            public string EventName { get; set; } = string.Empty;
            public string EventType { get; set; } = string.Empty;
            public int Count { get; set; }
            public bool IsSuccessful { get; set; }
            public DateTimeOffset LastSeen { get; set; }
            public Dictionary<string, object> Parameters { get; set; } = new();
        }
    }
}
