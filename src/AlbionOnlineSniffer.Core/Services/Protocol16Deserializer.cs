using System;
using System.Collections.Generic;
using Albion.Network;
using AlbionOnlineSniffer.Core.Models;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Handlers.AlbionNetworkHandlers;
using Microsoft.Extensions.Logging;

namespace AlbionOnlineSniffer.Core.Services
{
    /// <summary>
    /// Serviço responsável por decodificar pacotes do protocolo Photon (Protocol16) e disparar eventos para handlers registrados.
    /// Integrado com o sistema baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class Protocol16Deserializer : IDisposable
    {
        private readonly Albion.Network.IPhotonReceiver _photonReceiver;
        private readonly PhotonPacketEnricher _packetEnricher;
        private readonly PacketProcessor _packetProcessor;
        private readonly ILogger<Protocol16Deserializer> _logger;
        private bool _disposed;

        /// <summary>
        /// Evento disparado quando um pacote parseado gera um resultado relevante (ex: evento do jogo).
        /// </summary>
        public event Action<object>? OnParsedEvent;
        
        /// <summary>
        /// Evento disparado quando um pacote Photon é enriquecido com informações dos bin-dumps.
        /// </summary>
        public event Action<EnrichedPhotonPacket>? OnEnrichedPacket;

        /// <summary>
        /// Inicializa o deserializador com o ReceiverBuilder configurado externamente.
        /// </summary>
        /// <param name="packetEnricher">Serviço para enriquecer pacotes com informações dos bin-dumps</param>
        /// <param name="packetProcessor">Processador de pacotes baseado no albion-radar-deatheye-2pc</param>
        /// <param name="photonReceiver">Receiver do Albion.Network já configurado com handlers</param>
        /// <param name="logger">Logger para registrar eventos</param>
        public Protocol16Deserializer(PhotonPacketEnricher packetEnricher, PacketProcessor packetProcessor, Albion.Network.IPhotonReceiver photonReceiver, ILogger<Protocol16Deserializer> logger)
        {
            _packetEnricher = packetEnricher;
            _packetProcessor = packetProcessor;
            _photonReceiver = photonReceiver;
            _logger = logger;
        }

        /// <summary>
        /// Recebe o payload UDP bruto e encaminha para o parser Photon.
        /// </summary>
        /// <param name="payload">Payload UDP capturado.</param>
        public void ReceivePacket(byte[] payload)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(Protocol16Deserializer));
            try
            {
                _logger.LogInformation("📥 RECEBENDO PACOTE UDP: {Length} bytes", payload.Length);
                
                if (_photonReceiver == null)
                {
                    _logger.LogWarning("⚠️ PhotonReceiver não configurado!");
                    return;
                }
                
                _photonReceiver.ReceivePacket(payload);
                
                // Processar o pacote diretamente com Albion.Network
                // O parsing é feito pela biblioteca Albion.Network
                // Os handlers registrados processam os eventos e disparam para o EventDispatcher
                
                _logger.LogInformation("✅ Pacote processado pela biblioteca Albion.Network");
                
                // O processamento dos eventos é feito pelos handlers registrados
                // na biblioteca Albion.Network, que disparam para o EventDispatcher
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar pacote: {Message}", ex.Message);
            }
        }

        /// <summary>
        /// Libera os recursos utilizados pelo deserializador.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                // Por enquanto, apenas marca como disposed
                _disposed = true;
            }
        }
    }
}

