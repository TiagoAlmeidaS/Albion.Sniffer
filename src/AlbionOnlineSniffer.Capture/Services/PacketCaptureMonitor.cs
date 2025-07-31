using System;
using System.Threading;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Capture.Models;
using Microsoft.Extensions.Logging;

namespace AlbionOnlineSniffer.Capture.Services
{
    /// <summary>
    /// Serviço de monitoramento para captura de pacotes
    /// Fornece logging estruturado e métricas em tempo real
    /// </summary>
    public class PacketCaptureMonitor : IDisposable
    {
        private readonly ILogger<PacketCaptureMonitor> _logger;
        private readonly PacketCaptureMetrics _metrics;
        private readonly Timer _metricsTimer;
        private readonly object _lockObject = new object();
        private bool _disposed = false;

        /// <summary>
        /// Métricas atuais de captura
        /// </summary>
        public PacketCaptureMetrics Metrics => _metrics;

        /// <summary>
        /// Evento disparado quando as métricas são atualizadas
        /// </summary>
        public event Action<PacketCaptureMetrics>? OnMetricsUpdated;

        public PacketCaptureMonitor(ILogger<PacketCaptureMonitor> logger)
        {
            _logger = logger;
            _metrics = new PacketCaptureMetrics();
            
            // Timer para atualizar métricas a cada 5 segundos
            _metricsTimer = new Timer(UpdateMetrics, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
            
            _logger.LogInformation("PacketCaptureMonitor inicializado");
        }

        /// <summary>
        /// Registra o início da captura
        /// </summary>
        public void LogCaptureStarted(string interfaceName, string filter)
        {
            lock (_lockObject)
            {
                _metrics.Reset();
                _metrics.Status = "Running";
                _metrics.LastInterface = interfaceName;
                _metrics.LastFilter = filter;
            }

            _logger.LogInformation("Captura iniciada - Interface: {Interface}, Filtro: {Filter}", 
                interfaceName, filter);
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

            _logger.LogInformation("Captura parada - Estatísticas finais: {Metrics}", _metrics.ToString());
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

            // Log detalhado apenas para pacotes válidos (evita spam)
            if (isValid && payload != null)
            {
                _logger.LogDebug("Pacote capturado - Tamanho: {Size} bytes, Total: {Total}", 
                    payload.Length, _metrics.ValidPacketsCaptured);
                
                // Log hexadecimal dos primeiros bytes para debug (apenas se necessário)
                if (_logger.IsEnabled(LogLevel.Trace) && payload.Length > 0)
                {
                    var hexPreview = Convert.ToHexString(payload[..Math.Min(payload.Length, 32)]);
                    _logger.LogTrace("Pacote hex preview: {HexData}...", hexPreview);
                }
            }
        }

        /// <summary>
        /// Registra um erro de captura
        /// </summary>
        public void LogCaptureError(Exception exception, string context = "")
        {
            lock (_lockObject)
            {
                _metrics.CaptureErrors++;
                _metrics.LastError = exception.Message;
                _metrics.LastErrorTime = DateTime.UtcNow;
            }

            _logger.LogError(exception, "Erro na captura de pacotes - Contexto: {Context}, Total de erros: {ErrorCount}", 
                context, _metrics.CaptureErrors);
        }

        /// <summary>
        /// Registra informações sobre dispositivos de rede
        /// </summary>
        public void LogNetworkDevices(int deviceCount, string[] deviceNames)
        {
            _logger.LogInformation("Dispositivos de rede encontrados: {DeviceCount}", deviceCount);
            
            for (int i = 0; i < deviceNames.Length; i++)
            {
                _logger.LogDebug("Dispositivo {Index}: {DeviceName}", i, deviceNames[i]);
            }
        }

        /// <summary>
        /// Registra estatísticas periódicas
        /// </summary>
        public void LogPeriodicStats()
        {
            lock (_lockObject)
            {
                _metrics.CalculateRates();
            }

            _logger.LogInformation("Estatísticas de captura: {Metrics}", _metrics.ToString());
        }

        /// <summary>
        /// Registra informações sobre filtros aplicados
        /// </summary>
        public void LogFilterApplied(string filter, string deviceName)
        {
            _logger.LogInformation("Filtro aplicado - Dispositivo: {Device}, Filtro: {Filter}", 
                deviceName, filter);
        }

        /// <summary>
        /// Registra alerta quando a taxa de captura está baixa
        /// </summary>
        public void CheckAndLogLowCaptureRate()
        {
            lock (_lockObject)
            {
                _metrics.CalculateRates();
                
                // Alerta se não houve pacotes nos últimos 30 segundos
                var timeSinceLastCapture = DateTime.UtcNow - _metrics.LastCaptureTime;
                if (timeSinceLastCapture.TotalSeconds > 30 && _metrics.Status == "Running")
                {
                    _logger.LogWarning("Taxa de captura baixa - Nenhum pacote capturado nos últimos {Seconds} segundos", 
                        timeSinceLastCapture.TotalSeconds);
                }
                
                // Alerta se a taxa está muito baixa
                if (_metrics.PacketsPerSecond < 0.1 && _metrics.TotalCaptureTime.TotalMinutes > 1)
                {
                    _logger.LogWarning("Taxa de captura muito baixa: {Rate:F2} pacotes/segundo", 
                        _metrics.PacketsPerSecond);
                }
            }
        }

        /// <summary>
        /// Registra resumo detalhado das métricas
        /// </summary>
        public void LogDetailedMetrics()
        {
            lock (_lockObject)
            {
                _metrics.CalculateRates();
            }

            _logger.LogInformation("=== RESUMO DETALHADO DE CAPTURA ===");
            _logger.LogInformation("Status: {Status}", _metrics.Status);
            _logger.LogInformation("Interface: {Interface}", _metrics.LastInterface ?? "N/A");
            _logger.LogInformation("Filtro: {Filter}", _metrics.LastFilter ?? "N/A");
            _logger.LogInformation("Tempo ativo: {Uptime}", _metrics.TotalCaptureTime);
            _logger.LogInformation("Pacotes totais: {Total}", _metrics.TotalPacketsCaptured);
            _logger.LogInformation("Pacotes válidos: {Valid}", _metrics.ValidPacketsCaptured);
            _logger.LogInformation("Pacotes descartados: {Dropped}", _metrics.PacketsDropped);
            _logger.LogInformation("Bytes capturados: {Bytes:N0}", _metrics.TotalBytesCapturated);
            _logger.LogInformation("Taxa de pacotes: {Rate:F2} pkt/s", _metrics.PacketsPerSecond);
            _logger.LogInformation("Taxa de bytes: {Rate:F2} B/s", _metrics.BytesPerSecond);
            _logger.LogInformation("Erros de captura: {Errors}", _metrics.CaptureErrors);
            
            if (_metrics.LastErrorTime.HasValue)
            {
                _logger.LogInformation("Último erro: {Error} em {Time}", 
                    _metrics.LastError, _metrics.LastErrorTime.Value);
            }
            
            _logger.LogInformation("=== FIM DO RESUMO ===");
        }

        /// <summary>
        /// Atualiza métricas periodicamente
        /// </summary>
        private void UpdateMetrics(object? state)
        {
            if (_disposed) return;

            try
            {
                CheckAndLogLowCaptureRate();
                
                // Dispara evento de atualização de métricas
                OnMetricsUpdated?.Invoke(_metrics);
                
                // Log periódico apenas se houver atividade
                if (_metrics.Status == "Running" && _metrics.ValidPacketsCaptured > 0)
                {
                    LogPeriodicStats();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar métricas de monitoramento");
            }
        }

        /// <summary>
        /// Força uma atualização imediata das métricas
        /// </summary>
        public void ForceMetricsUpdate()
        {
            UpdateMetrics(null);
        }

        public void Dispose()
        {
            if (_disposed) return;
            
            _disposed = true;
            _metricsTimer?.Dispose();
            
            // Log final das métricas
            if (_metrics.Status == "Running")
            {
                LogCaptureStopped();
                LogDetailedMetrics();
            }
            
            _logger.LogInformation("PacketCaptureMonitor finalizado");
        }
    }
}