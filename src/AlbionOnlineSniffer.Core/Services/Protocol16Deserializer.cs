using System;
using Albion.Network; // Supondo que a dependência Albion.Network está disponível
using System.Collections.Generic;

namespace AlbionOnlineSniffer.Core.Services
{
    /// <summary>
    /// Serviço responsável por decodificar pacotes do protocolo Photon (Protocol16) e disparar eventos para handlers registrados.
    /// </summary>
    public class Protocol16Deserializer : IDisposable
    {
        private readonly IPhotonReceiver _photonReceiver;
        private readonly ReceiverBuilder _builder;
        private bool _disposed;

        /// <summary>
        /// Evento disparado quando um pacote parseado gera um resultado relevante (ex: evento do jogo).
        /// </summary>
        public event Action<object>? OnParsedEvent;

        /// <summary>
        /// Inicializa o deserializador e registra os handlers necessários.
        /// </summary>
        /// <param name="handlers">Lista de handlers customizados para eventos, operações e respostas.</param>
        public Protocol16Deserializer(IEnumerable<object>? handlers = null)
        {
            _builder = ReceiverBuilder.Create();
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
                _photonReceiver.ReceivePacket(payload);
            }
            catch (Exception ex)
            {
                // TODO: Adicionar logging estruturado
                Console.Error.WriteLine($"[Protocol16Deserializer] Error parsing packet: {ex.Message}");
            }
        }

        /// <summary>
        /// Registra um handler customizado no builder (event, request ou response handler).
        /// </summary>
        /// <param name="handler">Handler a ser registrado.</param>
        public void RegisterHandler(object handler)
        {
            // O tipo do handler define o método de registro
            // Exemplo: AddEventHandler, AddRequestHandler, AddResponseHandler
            // Aqui, usamos reflection para simplificar
            var method = handler.GetType().Name switch
            {
                var n when n.EndsWith("EventHandler") => "AddEventHandler",
                var n when n.EndsWith("RequestHandler") => "AddRequestHandler",
                var n when n.EndsWith("ResponseHandler") => "AddResponseHandler",
                _ => null
            };
            if (method != null)
            {
                var mi = _builder.GetType().GetMethod(method);
                mi?.Invoke(_builder, new[] { handler });
            }
            else
            {
                throw new ArgumentException($"Handler type not recognized: {handler.GetType().Name}");
            }
        }

        /// <summary>
        /// Libera recursos do deserializador.
        /// </summary>
        public void Dispose()
        {
            _disposed = true;
            // Se necessário, liberar recursos dos handlers ou do builder
        }
    }
}

