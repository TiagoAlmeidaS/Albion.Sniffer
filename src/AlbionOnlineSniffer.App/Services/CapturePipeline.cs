using System;
using AlbionOnlineSniffer.Capture.Interfaces;
using AlbionOnlineSniffer.Core.Services;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Capture;

namespace AlbionOnlineSniffer.App.Services
{
    /// <summary>
    /// Conecta a captura UDP ao desserializador de protocolo.
    /// </summary>
    public sealed class CapturePipeline
    {
        private readonly IPacketCaptureService _captureService;
        private readonly Protocol16Deserializer _deserializer;
        private readonly ILogger<CapturePipeline> _logger;

        public CapturePipeline(IPacketCaptureService captureService, Protocol16Deserializer deserializer, ILogger<CapturePipeline> logger)
        {
            _captureService = captureService;
            _deserializer = deserializer;
            _logger = logger;

            _captureService.OnUdpPayloadCaptured += OnPacket;

            if (_captureService is PacketCaptureService packetCapture)
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
            _captureService.Start();
            _logger.LogInformation("✅ Captura iniciada com sucesso! 📡 Aguardando pacotes...");
        }
        public void Stop()
        {
            _logger.LogInformation("🛑 Parando captura...");
            _captureService.Stop();
            _logger.LogInformation("✅ Captura parada.");
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
                _logger.LogError(ex, "Erro ao processar pacote");
            }
        }
    }
}


