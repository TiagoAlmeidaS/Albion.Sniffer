using System;
using System.Threading.Tasks;

namespace AlbionOnlineSniffer.Queue.Interfaces
{
    /// <summary>
    /// Interface para publicação de mensagens em filas
    /// </summary>
    public interface IQueuePublisher : IDisposable
    {
        /// <summary>
        /// Publica uma mensagem em um tópico específico
        /// </summary>
        /// <param name="topic">Tópico da fila</param>
        /// <param name="message">Mensagem a ser publicada</param>
        /// <returns>Task de conclusão</returns>
        Task PublishAsync(string topic, object message);
    }
}
