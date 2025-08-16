using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Capture.Models;

namespace AlbionOnlineSniffer.Capture.Services
{
    public class PacketCaptureMonitor : IDisposable
    {
        private readonly PacketCaptureMetrics _metrics;
        private readonly object _lockObject = new object();
        private Timer? _metricsTimer;
        private bool _disposed;

        /// <summary>
        /// Evento disparado quando as métricas são atualizadas
        /// </summary>
        public event Action<PacketCaptureMetrics>? OnMetricsUpdated;

        public PacketCaptureMonitor()
        {
            _metrics = new PacketCaptureMetrics();
        }

        /// <summary>
        /// Registra o início da captura
        /// </summary>
        public void LogCaptureStarted(string interfaceName, string filter)
        {
            lock (_lockObject)
            {
                _metrics.Status = "Running";
                _metrics.LastInterface = interfaceName;
                _metrics.LastFilter = filter;
            }
        }

        /// <summary>
        /// Registra a parada da captura
        /// </summary>
        public void LogCaptureStopped()
        {
            lock (_lockObject)
            {
                _metrics.Status = "Stopped";
                _metrics.CalculateRates();
            }
        }

        /// <summary>
        /// Registra um pacote capturado
        /// </summary>
        public void LogPacketCaptured(byte[] payload, bool isValid = true)
        {
            lock (_lockObject)
            {
                _metrics.TotalPacketsCaptured++;
                _metrics.LastCaptureTime = DateTime.UtcNow;
                _metrics.TotalBytesCapturated += payload?.Length ?? 0;

                if (isValid)
                {
                    _metrics.ValidPacketsCaptured++;
                }
                else
                {
                    _metrics.PacketsDropped++;
                }
            }
        }

        /// <summary>
        /// Registra um erro de captura
        /// </summary>
        public void LogCaptureError(Exception exception, string context)
        {
            lock (_lockObject)
            {
                _metrics.CaptureErrors++;
                _metrics.LastError = exception.Message;
                _metrics.LastErrorTime = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Registra dispositivos de rede encontrados
        /// </summary>
        public void LogNetworkDevices(int deviceCount, string[] deviceNames)
        {
            lock (_lockObject)
            {
                _metrics.NetworkDevicesFound = deviceCount;
            }
        }

        /// <summary>
        /// Registra filtro aplicado
        /// </summary>
        public void LogFilterApplied(string filter, string device)
        {
            lock (_lockObject)
            {
                _metrics.LastFilter = filter;
            }
        }

        /// <summary>
        /// Verifica taxa de captura baixa e emite alertas
        /// </summary>
        public void CheckLowCaptureRate()
        {
            lock (_lockObject)
            {
                if (_metrics.Status != "Running") return;

                var timeSinceLastCapture = DateTime.UtcNow - _metrics.LastCaptureTime;
                if (timeSinceLastCapture.TotalSeconds > 30)
                {
                    // Alerta silencioso - será exibido na interface web
                }

                if (_metrics.PacketsPerSecond < 0.1)
                {
                    // Alerta silencioso - será exibido na interface web
                }
            }
        }

        /// <summary>
        /// Obtém métricas atuais
        /// </summary>
        public PacketCaptureMetrics GetMetrics()
        {
            lock (_lockObject)
            {
                _metrics.CalculateRates();
                var metrics = _metrics.Clone();
                OnMetricsUpdated?.Invoke(metrics);
                return metrics;
            }
        }

        /// <summary>
        /// Força atualização das métricas
        /// </summary>
        public void ForceMetricsUpdate()
        {
            lock (_lockObject)
            {
                _metrics.CalculateRates();
            }
        }

        /// <summary>
        /// Log das métricas detalhadas
        /// </summary>
        public void LogDetailedMetrics()
        {
            lock (_lockObject)
            {
                _metrics.CalculateRates();
            }
        }

        /// <summary>
        /// Inicia o timer de atualização de métricas
        /// </summary>
        public void StartMetricsTimer()
        {
            if (_metricsTimer != null) return;

            _metricsTimer = new Timer(async _ =>
            {
                try
                {
                    await UpdateMetricsAsync();
                }
                catch (Exception ex)
                {
                    // Tratamento silencioso de erro
                }
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
        }

        /// <summary>
        /// Para o timer de atualização de métricas
        /// </summary>
        public void StopMetricsTimer()
        {
            _metricsTimer?.Dispose();
            _metricsTimer = null;
        }

        /// <summary>
        /// Atualiza métricas de forma assíncrona
        /// </summary>
        private async Task UpdateMetricsAsync()
        {
            await Task.Run(() =>
            {
                lock (_lockObject)
                {
                    _metrics.CalculateRates();
                }
            });
        }

        public void Dispose()
        {
            if (_disposed) return;

            StopMetricsTimer();
            _disposed = true;
        }
    }
}