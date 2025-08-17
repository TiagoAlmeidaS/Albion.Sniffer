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
        private readonly IPacketCaptureService _captureService;
        private readonly Protocol16Deserializer _deserializer;
        private readonly ILogger<CapturePipeline> _logger;

        public CapturePipeline(IPacketCaptureService captureService, Protocol16Deserializer deserializer, ILogger<CapturePipeline> logger)
        {
            _captureService = captureService;
            _deserializer = deserializer;
            _logger = logger;

            _captureService.OnUdpPayloadCaptured += OnPacket;
        }

        public void Start() => _captureService.Start();
        public void Stop() => _captureService.Stop();

        private void OnPacket(byte[] packetData)
        {
            try
            {
                _deserializer.ReceivePacket(packetData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar pacote");
            }
        }
    }
}


