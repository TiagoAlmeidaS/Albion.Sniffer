using System;
using AlbionOnlineSniffer.Capture.Interfaces;
using AlbionOnlineSniffer.Core.Services;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Capture;

namespace AlbionOnlineSniffer.App.Services
{
    /// <summary>
    /// Conecta a captura UDP/TCP ao desserializador de protocolo.
    /// </summary>
    public sealed class CapturePipeline
    {
        private readonly IPacketCaptureService _udpCaptureService;
        private readonly HybridCaptureService? _hybridCaptureService;
        private readonly Protocol16Deserializer _deserializer;
        private readonly ILogger<CapturePipeline> _logger;

        public CapturePipeline(
            IPacketCaptureService udpCaptureService, 
            HybridCaptureService? hybridCaptureService,
            Protocol16Deserializer deserializer, 
            ILogger<CapturePipeline> logger)
        {
            _udpCaptureService = udpCaptureService;
            _hybridCaptureService = hybridCaptureService;
            _deserializer = deserializer;
            _logger = logger;

            // Configurar eventos baseado no serviço disponível
            if (_hybridCaptureService != null)
            {
                // Usar serviço híbrido
                _hybridCaptureService.OnDataCaptured += OnPacket;
                _logger.LogInformation("🎯 CapturePipeline configurado com serviço híbrido");
            }
            else
            {
                // Fallback para serviço UDP tradicional
                _udpCaptureService.OnUdpPayloadCaptured += OnPacket;
                _logger.LogInformation("📡 CapturePipeline configurado com serviço UDP tradicional");
            }

            // Configurar métricas se disponível
            if (_udpCaptureService is PacketCaptureService packetCapture)
            {
                packetCapture.Monitor.OnMetricsUpdated += metrics =>
                {
                    _logger.LogInformation("📊 CAPTURE MÉTRICAS: {Packets} pacotes válidos, {Rate} B/s, Filtro={Filter}",
                        metrics.ValidPacketsCaptured, metrics.BytesPerSecond, metrics.LastFilter);
                };
            }
        }

        public void Start()
        {
            _logger.LogInformation("🚀 Iniciando captura de pacotes...");
            
            if (_hybridCaptureService != null)
            {
                _hybridCaptureService.Start();
            }
            else
            {
                _udpCaptureService.Start();
            }
            
            _logger.LogInformation("✅ Captura iniciada com sucesso! 📡 Aguardando pacotes...");
        }

        public void Stop()
        {
            _logger.LogInformation("🛑 Parando captura...");
            
            if (_hybridCaptureService != null)
            {
                _hybridCaptureService.Stop();
            }
            else
            {
                _udpCaptureService.Stop();
            }
            
            _logger.LogInformation("✅ Captura parada.");
        }

        private void OnPacket(byte[] packetData)
        {
            try
            {
                _logger.LogInformation("📡 PACOTE CAPTURADO: {Length} bytes", packetData?.Length ?? 0);
                _deserializer.ReceivePacket(packetData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar pacote");
            }
        }
    }
}


