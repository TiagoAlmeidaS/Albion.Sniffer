using System;
using AlbionOnlineSniffer.Capture.Interfaces;
using AlbionOnlineSniffer.Core.Services;
using Microsoft.Extensions.Logging;

namespace AlbionOnlineSniffer.App.Services
{
    /// <summary>
    /// Conecta a captura UDP ao desserializador de protocolo.
    /// </summary>
    public sealed class CapturePipeline
    {
        private readonly IPacketCaptureService _udpCaptureService;
        private readonly Protocol16Deserializer _deserializer;
        private readonly ILogger<CapturePipeline> _logger;

        public CapturePipeline(
            IPacketCaptureService udpCaptureService, 
            Protocol16Deserializer deserializer, 
            ILogger<CapturePipeline> logger)
        {
            _udpCaptureService = udpCaptureService;
            _deserializer = deserializer;
            _logger = logger;

            // Configurar eventos UDP
            _udpCaptureService.OnUdpPayloadCaptured += OnPacket;
            _logger.LogInformation("📡 CapturePipeline configurado com serviço UDP na porta 5050");

            // Configurar métricas se disponível
            if (_udpCaptureService is AlbionOnlineSniffer.Capture.PacketCaptureService packetCapture)
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
            _logger.LogInformation("🚀 Iniciando captura de pacotes UDP...");
            _udpCaptureService.Start();
            _logger.LogInformation("✅ Captura UDP iniciada com sucesso! 📡 Aguardando pacotes na porta 5050...");
        }

        public void Stop()
        {
            _logger.LogInformation("🛑 Parando captura UDP...");
            _udpCaptureService.Stop();
            _logger.LogInformation("✅ Captura UDP parada.");
        }

        private void OnPacket(byte[] packetData)
        {
            try
            {
                _logger.LogInformation("📡 PACOTE UDP CAPTURADO: {Length} bytes", packetData?.Length ?? 0);
                _deserializer.ReceivePacket(packetData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar pacote UDP");
            }
        }
    }
}


