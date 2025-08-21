using System;
using System.Threading.Tasks;

namespace AlbionOnlineSniffer.Core.Interfaces
{
    /// <summary>
    /// Interface genérica para publicação de eventos
    /// Esta interface fica no Core para evitar dependência circular
    /// </summary>
    public interface IEventPublisher : IDisposable
    {
        /// <summary>
        /// Publica um evento em um tópico específico
        /// </summary>
        /// <param name="topic">Tópico do evento</param>
        /// <param name="eventData">Dados do evento</param>
        /// <returns>Task de conclusão</returns>
        Task PublishEventAsync(string topic, object eventData);
    }
}
