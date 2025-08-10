using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.ResponseObj;

namespace AlbionOnlineSniffer.Core.Interfaces
{
    /// <summary>
    /// Factory interface para criação de eventos com injeção de dependência
    /// </summary>
    public interface IEventFactory
    {
        /// <summary>
        /// Cria uma instância de evento com injeção automática de PacketOffsets
        /// </summary>
        /// <typeparam name="T">Tipo do evento a ser criado</typeparam>
        /// <param name="parameters">Parâmetros do pacote</param>
        /// <returns>Instância do evento criado</returns>
        T CreateEvent<T>(Dictionary<byte, object> parameters) where T : class;
        
        /// <summary>
        /// Cria uma instância de evento específica
        /// </summary>
        /// <param name="eventType">Tipo do evento</param>
        /// <param name="parameters">Parâmetros do pacote</param>
        /// <returns>Instância do evento criado</returns>
        object CreateEvent(Type eventType, Dictionary<byte, object> parameters);
    }
}