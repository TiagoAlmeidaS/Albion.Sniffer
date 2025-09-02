using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AlbionOnlineSniffer.Core.Services
{
    /// <summary>
    /// Gerencia estatísticas em tempo real dos pacotes descobertos pelo DiscoveryService
    /// </summary>
    public class DiscoveryStatistics : IDisposable
    {
        private readonly ConcurrentDictionary<int, PacketStats> _packetStats = new();
        private readonly ConcurrentDictionary<string, int> _packetTypes = new();
        private readonly Timer _displayTimer;
        private readonly object _lockObject = new();
        private bool _disposed = false;
        private int _totalPackets = 0;
        private int _hiddenPackets = 0;
        private int _visiblePackets = 0;
        private DateTime _startTime = DateTime.UtcNow;

        // Propriedades públicas para acesso via web
        public int TotalPackets => _totalPackets;
        public int HiddenPackets => _hiddenPackets;
        public int VisiblePackets => _visiblePackets;
        public TimeSpan Uptime => DateTime.UtcNow - _startTime;
        public IReadOnlyDictionary<int, PacketStats> PacketStatistics => _packetStats;
        public IReadOnlyDictionary<string, int> PacketTypes => _packetTypes;

        public DiscoveryStatistics()
        {
            // Atualizar estatísticas a cada 5 segundos
            _displayTimer = new Timer(DisplayStatistics, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        }

        public void RecordPacket(int packetCode, string packetType, bool isHidden)
        {
            Interlocked.Increment(ref _totalPackets);
            
            if (isHidden)
            {
                Interlocked.Increment(ref _hiddenPackets);
            }
            else
            {
                Interlocked.Increment(ref _visiblePackets);
            }

            // Atualizar estatísticas por código
            _packetStats.AddOrUpdate(packetCode, 
                new PacketStats { Code = packetCode, Type = packetType, Count = 1, IsHidden = isHidden },
                (key, existing) => 
                {
                    existing.Count++;
                    existing.LastSeen = DateTime.UtcNow;
                    return existing;
                });

            // Atualizar estatísticas por tipo
            _packetTypes.AddOrUpdate(packetType, 1, (key, existing) => existing + 1);
        }

        private void DisplayStatistics(object state)
        {
            if (_disposed) return;

            lock (_lockObject)
            {
                Console.Clear();
                Console.WriteLine("🔍 DISCOVERY SERVICE - ESTATÍSTICAS EM TEMPO REAL");
                Console.WriteLine(new string('=', 60));
                Console.WriteLine($"⏱️  Tempo de execução: {DateTime.UtcNow - _startTime:hh\\:mm\\:ss}");
                Console.WriteLine($"📊 Total de pacotes: {_totalPackets:N0}");
                Console.WriteLine($"👁️  Pacotes visíveis: {_visiblePackets:N0} ({(_totalPackets > 0 ? (double)_visiblePackets / _totalPackets * 100 : 0):F1}%)");
                Console.WriteLine($"🙈 Pacotes escondidos: {_hiddenPackets:N0} ({(_totalPackets > 0 ? (double)_hiddenPackets / _totalPackets * 100 : 0):F1}%)");
                Console.WriteLine();

                // Top 10 pacotes mais frequentes
                var topPackets = _packetStats.Values
                    .OrderByDescending(p => p.Count)
                    .Take(10)
                    .ToList();

                Console.WriteLine("🏆 TOP 10 PACOTES MAIS FREQUENTES:");
                Console.WriteLine(new string('-', 60));
                Console.WriteLine($"{"Código",-8} {"Tipo",-20} {"Count",-8} {"Status",-10} {"Último",-12}");
                Console.WriteLine(new string('-', 60));

                foreach (var packet in topPackets)
                {
                    var status = packet.IsHidden ? "🙈 Escondido" : "👁️  Visível";
                    var lastSeen = packet.LastSeen.ToString("HH:mm:ss");
                    Console.WriteLine($"{packet.Code,-8} {packet.Type,-20} {packet.Count,-8} {status,-10} {lastSeen,-12}");
                }

                Console.WriteLine();

                // Estatísticas por tipo
                var topTypes = _packetTypes
                    .OrderByDescending(t => t.Value)
                    .Take(5)
                    .ToList();

                Console.WriteLine("📈 TOP 5 TIPOS DE PACOTES:");
                Console.WriteLine(new string('-', 40));
                Console.WriteLine($"{"Tipo",-25} {"Count",-8}");
                Console.WriteLine(new string('-', 40));

                foreach (var type in topTypes)
                {
                    Console.WriteLine($"{type.Key,-25} {type.Value,-8}");
                }

                Console.WriteLine();
                Console.WriteLine("🔄 Atualizando a cada 5 segundos... (Ctrl+C para parar)");
                Console.WriteLine(new string('=', 60));
            }
        }

        public object GetWebStats()
        {
            return new
            {
                timestamp = DateTime.UtcNow,
                totalPackets = _totalPackets,
                visiblePackets = _visiblePackets,
                hiddenPackets = _hiddenPackets,
                visiblePercentage = _totalPackets > 0 ? (double)_visiblePackets / _totalPackets * 100 : 0,
                hiddenPercentage = _totalPackets > 0 ? (double)_hiddenPackets / _totalPackets * 100 : 0,
                uptime = Uptime,
                topPackets = GetTopPacketsForWeb(10),
                topTypes = GetTopTypesForWeb(5)
            };
        }

        public object GetTopPacketsForWeb(int limit = 10)
        {
            return _packetStats.Values
                .OrderByDescending(p => p.Count)
                .Take(limit)
                .Select(p => new
                {
                    code = p.Code,
                    type = p.Type,
                    count = p.Count,
                    isHidden = p.IsHidden,
                    lastSeen = p.LastSeen
                })
                .ToList();
        }

        public object GetTopTypesForWeb(int limit = 5)
        {
            return _packetTypes
                .OrderByDescending(t => t.Value)
                .Take(limit)
                .Select(t => new
                {
                    type = t.Key,
                    count = t.Value
                })
                .ToList();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _displayTimer?.Dispose();
            }
        }

        public class PacketStats
        {
            public int Code { get; set; }
            public string Type { get; set; } = string.Empty;
            public int Count { get; set; }
            public bool IsHidden { get; set; }
            public DateTime LastSeen { get; set; } = DateTime.UtcNow;
        }
    }
}
