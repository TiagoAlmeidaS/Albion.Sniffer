using System;
using System.Collections.Generic;
using Albion.Network;
using AlbionOnlineSniffer.Core.Models;
using AlbionOnlineSniffer.Core.Services;
using Microsoft.Extensions.Logging;

namespace AlbionOnlineSniffer.Core.Services
{
    /// <summary>
    /// Deserializador para o protocolo 16 do Albion Online
    /// </summary>
    public class Protocol16Deserializer : IDisposable
    {
        private readonly ILogger<Protocol16Deserializer> _logger;
        private readonly IPhotonReceiver _photonReceiver;
        private readonly DiscoveryService? _discoveryService;

        public Protocol16Deserializer(
            IPhotonReceiver photonReceiver,
            ILogger<Protocol16Deserializer> logger,
            DiscoveryService? discoveryService = null)
        {
            _logger = logger;
            _photonReceiver = photonReceiver;
            _discoveryService = discoveryService;
            
            if (_discoveryService != null)
            {
                _logger.LogInformation("🔍 DiscoveryService conectado ao Protocol16Deserializer");
            }
        }

        /// <summary>
        /// Recebe e processa um pacote UDP
        /// </summary>
        /// <param name="payload">Payload UDP</param>
        public void ReceivePacket(byte[] payload)
        {
            try
            {
                _logger.LogDebug("Recebendo pacote UDP de {PayloadLength} bytes", payload.Length);

                // ✅ PROCESSAR O PACOTE ATRAVÉS DO ALBION.NETWORK
                // O DiscoveryDebugHandler irá interceptar automaticamente após descriptografia
                _photonReceiver.ReceivePacket(payload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar pacote UDP: {Message}", ex.Message);
            }
        }

        public void Dispose()
        {
            _logger.LogInformation("Protocol16Deserializer finalizado");
        }
    }
}

