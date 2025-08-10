using AlbionOnlineSniffer.Core.Interfaces;
using AlbionOnlineSniffer.Core.Models.ResponseObj;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AlbionOnlineSniffer.Core.Services
{
    /// <summary>
    /// Factory para criação de eventos com injeção automática de PacketOffsets
    /// </summary>
    public class EventFactory : IEventFactory
    {
        private readonly PacketOffsets _packetOffsets;

        public EventFactory(PacketOffsets packetOffsets)
        {
            _packetOffsets = packetOffsets ?? throw new ArgumentNullException(nameof(packetOffsets));
        }

        /// <summary>
        /// Cria uma instância de evento com injeção automática de PacketOffsets
        /// </summary>
        /// <typeparam name="T">Tipo do evento a ser criado</typeparam>
        /// <param name="parameters">Parâmetros do pacote</param>
        /// <returns>Instância do evento criado</returns>
        public T CreateEvent<T>(Dictionary<byte, object> parameters) where T : class
        {
            return (T)CreateEvent(typeof(T), parameters);
        }

        /// <summary>
        /// Cria uma instância de evento específica
        /// </summary>
        /// <param name="eventType">Tipo do evento</param>
        /// <param name="parameters">Parâmetros do pacote</param>
        /// <returns>Instância do evento criado</returns>
        public object CreateEvent(Type eventType, Dictionary<byte, object> parameters)
        {
            if (eventType == null)
                throw new ArgumentNullException(nameof(eventType));
            
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            try
            {
                // Busca o construtor que aceita Dictionary<byte, object> e PacketOffsets
                var constructor = eventType.GetConstructor(new Type[] { typeof(Dictionary<byte, object>), typeof(PacketOffsets) });
                
                if (constructor != null)
                {
                    // Cria a instância passando os parâmetros e o PacketOffsets
                    return Activator.CreateInstance(eventType, parameters, _packetOffsets);
                }
                
                // Fallback: tenta o construtor antigo apenas com Dictionary<byte, object>
                var oldConstructor = eventType.GetConstructor(new Type[] { typeof(Dictionary<byte, object>) });
                if (oldConstructor != null)
                {
                    // Para compatibilidade com eventos que ainda não foram refatorados
                    return Activator.CreateInstance(eventType, parameters);
                }
                
                throw new InvalidOperationException($"Não foi possível encontrar um construtor adequado para o tipo {eventType.Name}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erro ao criar instância do evento {eventType.Name}: {ex.Message}", ex);
            }
        }
    }
}
