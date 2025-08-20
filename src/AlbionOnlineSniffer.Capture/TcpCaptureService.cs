using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Capture.Interfaces;
using AlbionOnlineSniffer.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AlbionOnlineSniffer.Capture
{
    /// <summary>
    /// Serviço para captura de dados via TCP (conexões remotas)
    /// </summary>
    public class TcpCaptureService : ITcpCaptureService, IDisposable
    {
        private readonly string _endpoint;
        private readonly int _port;
        private readonly IAlbionEventLogger _eventLogger;
        private readonly ILogger<TcpCaptureService>? _logger;
        private TcpClient? _tcpClient;
        private NetworkStream? _stream;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _disposed;

        public event Action<byte[]>? OnTcpDataCaptured;

        public bool IsCapturing { get; private set; }

        public string Endpoint => _endpoint;

        public TcpCaptureService(string endpoint, IAlbionEventLogger? eventLogger = null, ILogger<TcpCaptureService>? logger = null)
        {
            var parts = endpoint.Split(':');
            if (parts.Length != 2 || !int.TryParse(parts[1], out _port))
            {
                throw new ArgumentException("Endpoint deve estar no formato 'host:port'", nameof(endpoint));
            }

            _endpoint = endpoint;
            _eventLogger = eventLogger ?? new AlbionOnlineSniffer.Core.Services.AlbionEventLogger();
            _logger = logger;
        }

        public void Start()
        {
            if (IsCapturing)
                return;

            try
            {
                _logger?.LogInformation("🚀 Iniciando captura TCP para endpoint: {Endpoint}", _endpoint);
                
                _cancellationTokenSource = new CancellationTokenSource();
                _tcpClient = new TcpClient();
                
                // Conectar ao endpoint TCP
                var parts = _endpoint.Split(':');
                _tcpClient.ConnectAsync(parts[0], _port).Wait();
                _stream = _tcpClient.GetStream();
                
                IsCapturing = true;
                _logger?.LogInformation("✅ Conectado ao endpoint TCP: {Endpoint}", _endpoint);

                // Iniciar captura em background
                _ = Task.Run(() => CaptureLoop(_cancellationTokenSource.Token));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ Erro ao iniciar captura TCP para {Endpoint}", _endpoint);
                _eventLogger.LogCaptureError($"Erro ao conectar TCP: {ex.Message}", "StartTcpCapture", ex);
                throw;
            }
        }

        public void Stop()
        {
            if (!IsCapturing)
                return;

            try
            {
                _logger?.LogInformation("🛑 Parando captura TCP...");
                
                _cancellationTokenSource?.Cancel();
                _stream?.Close();
                _tcpClient?.Close();
                
                IsCapturing = false;
                _logger?.LogInformation("✅ Captura TCP parada");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ Erro ao parar captura TCP");
            }
        }

        private async Task CaptureLoop(CancellationToken cancellationToken)
        {
            var buffer = new byte[8192]; // Buffer de 8KB

            try
            {
                while (!cancellationToken.IsCancellationRequested && _stream != null)
                {
                    var bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    
                    if (bytesRead > 0)
                    {
                        var data = new byte[bytesRead];
                        Array.Copy(buffer, data, bytesRead);
                        
                        _logger?.LogInformation("📡 Dados TCP capturados: {Length} bytes", bytesRead);
                        
                        // Disparar evento com dados capturados
                        OnTcpDataCaptured?.Invoke(data);
                        
                        // Log para o sistema de eventos
                        _eventLogger.LogUdpPacketCapture(
                            data,
                            "0.0.0.0", // IP origem (não disponível em TCP)
                            0,          // Porta origem (não disponível em TCP)
                            _endpoint.Split(':')[0], // IP destino
                            _port       // Porta destino
                        );
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Cancelação normal, não é erro
                _logger?.LogInformation("🔄 Captura TCP cancelada");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ Erro na captura TCP");
                _eventLogger.LogCaptureError($"Erro na captura TCP: {ex.Message}", "CaptureLoop", ex);
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            try
            {
                Stop();
                _cancellationTokenSource?.Dispose();
                _tcpClient?.Dispose();
                _stream?.Dispose();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ Erro ao fazer dispose do TcpCaptureService");
            }

            _disposed = true;
        }
    }
}
