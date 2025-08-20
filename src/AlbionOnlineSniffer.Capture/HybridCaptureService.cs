using System;
using AlbionOnlineSniffer.Capture.Interfaces;
using AlbionOnlineSniffer.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AlbionOnlineSniffer.Capture
{
    /// <summary>
    /// Serviço de captura híbrido que pode alternar entre UDP e TCP
    /// </summary>
    public class HybridCaptureService : IDisposable
    {
        private readonly IPacketCaptureService _udpCaptureService;
        private readonly ITcpCaptureService _tcpCaptureService;
        private readonly ILogger<HybridCaptureService>? _logger;
        private readonly string _mode;
        private bool _disposed;

        public event Action<byte[]>? OnDataCaptured;

        public bool IsCapturing { get; private set; }

        public HybridCaptureService(
            IPacketCaptureService udpCaptureService,
            ITcpCaptureService tcpCaptureService,
            string mode,
            ILogger<HybridCaptureService>? logger = null)
        {
            _udpCaptureService = udpCaptureService;
            _tcpCaptureService = tcpCaptureService;
            _mode = mode.ToUpperInvariant();
            _logger = logger;

            // Configurar eventos baseado no modo
            if (_mode == "UDP" || _mode == "HYBRID")
            {
                _udpCaptureService.OnUdpPayloadCaptured += OnUdpDataCaptured;
            }

            if (_mode == "TCP" || _mode == "HYBRID")
            {
                _tcpCaptureService.OnTcpDataCaptured += OnTcpDataCaptured;
            }
        }

        public void Start()
        {
            if (IsCapturing)
                return;

            try
            {
                _logger?.LogInformation("🚀 Iniciando captura híbrida no modo: {Mode}", _mode);

                if (_mode == "UDP" || _mode == "HYBRID")
                {
                    _udpCaptureService.Start();
                    _logger?.LogInformation("✅ Captura UDP iniciada");
                }

                if (_mode == "TCP" || _mode == "HYBRID")
                {
                    _tcpCaptureService.Start();
                    _logger?.LogInformation("✅ Captura TCP iniciada");
                }

                IsCapturing = true;
                _logger?.LogInformation("🎯 Captura híbrida iniciada com sucesso!");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ Erro ao iniciar captura híbrida");
                throw;
            }
        }

        public void Stop()
        {
            if (!IsCapturing)
                return;

            try
            {
                _logger?.LogInformation("🛑 Parando captura híbrida...");

                if (_mode == "UDP" || _mode == "HYBRID")
                {
                    _udpCaptureService.Stop();
                }

                if (_mode == "TCP" || _mode == "HYBRID")
                {
                    _tcpCaptureService.Stop();
                }

                IsCapturing = false;
                _logger?.LogInformation("✅ Captura híbrida parada");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ Erro ao parar captura híbrida");
            }
        }

        private void OnUdpDataCaptured(byte[] data)
        {
            _logger?.LogInformation("📡 Dados UDP capturados: {Length} bytes", data.Length);
            OnDataCaptured?.Invoke(data);
        }

        private void OnTcpDataCaptured(byte[] data)
        {
            _logger?.LogInformation("📡 Dados TCP capturados: {Length} bytes", data.Length);
            OnDataCaptured?.Invoke(data);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            try
            {
                Stop();
                _udpCaptureService.OnUdpPayloadCaptured -= OnUdpDataCaptured;
                _tcpCaptureService.OnTcpDataCaptured -= OnTcpDataCaptured;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ Erro ao fazer dispose do HybridCaptureService");
            }

            _disposed = true;
        }
    }
}
