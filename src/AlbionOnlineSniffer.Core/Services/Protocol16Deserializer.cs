using System;
using System.Collections.Generic;
using Albion.Network;
using AlbionOnlineSniffer.Core.Models;
using AlbionOnlineSniffer.Core.Services;
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
        private readonly Albion.Network.ReceiverBuilder _builder;
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
        /// Inicializa o deserializador e registra os handlers necessários.
        /// </summary>
        /// <param name="packetEnricher">Serviço para enriquecer pacotes com informações dos bin-dumps</param>
        /// <param name="packetProcessor">Processador de pacotes baseado no albion-radar-deatheye-2pc</param>
        /// <param name="logger">Logger para registrar eventos</param>
        /// <param name="handlers">Lista de handlers customizados para eventos, operações e respostas.</param>
        public Protocol16Deserializer(PhotonPacketEnricher packetEnricher, PacketProcessor packetProcessor, ILogger<Protocol16Deserializer> logger, IEnumerable<object>? handlers = null)
        {
            _packetEnricher = packetEnricher;
            _packetProcessor = packetProcessor;
            _logger = logger;
            
            _builder = Albion.Network.ReceiverBuilder.Create();
            if (handlers != null)
            {
                foreach (var handler in handlers)
                {
                    RegisterHandler(handler);
                }
            }
            _photonReceiver = _builder.Build();
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
                
                _photonReceiver?.ReceivePacket(payload);
                
                // Processar o pacote diretamente com Albion.Network
                // O parsing é feito pela biblioteca Albion.Network
                
                _logger.LogInformation("✅ Pacote processado pela biblioteca Albion.Network");
                
                // O processamento dos eventos é feito pelos handlers registrados
                // na biblioteca Albion.Network
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar pacote: {Message}", ex.Message);
            }
        }

        /// <summary>
        /// Registra um handler customizado no builder (event, request ou response handler).
        /// </summary>
        /// <param name="handler">Handler a ser registrado.</param>
        public void RegisterHandler(object handler)
        {
            // Por enquanto, apenas loga o handler
            _logger.LogDebug("Handler registrado: {HandlerType}", handler.GetType().Name);
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

