using System;
using System.Collections.Generic;
using Albion.Network;
using Albion.Sniffer.Core.Models;
using Albion.Sniffer.Core.Services;
using Microsoft.Extensions.Logging;

namespace Albion.Sniffer.Core.Services
{
    /// <summary>
    /// Deserializador para o protocolo 16 do Albion Online
    /// </summary>
    public class Protocol16Deserializer : IDisposable
    {
        private readonly ILogger<Protocol16Deserializer> _logger;
        private readonly IPhotonReceiver _photonReceiver;

        public Protocol16Deserializer(
            IPhotonReceiver photonReceiver,
            ILogger<Protocol16Deserializer> logger)
        {
            _logger = logger;
            _photonReceiver = photonReceiver;
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

                // Processar o pacote através do receiver do Albion.Network
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

