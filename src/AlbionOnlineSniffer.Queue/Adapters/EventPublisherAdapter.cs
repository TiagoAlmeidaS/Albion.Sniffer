using AlbionOnlineSniffer.Core.Interfaces;
using AlbionOnlineSniffer.Queue.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AlbionOnlineSniffer.Queue.Adapters
{
    /// <summary>
    /// Adaptador que implementa IEventPublisher do Core usando IQueuePublisher do Queue
    /// Este adaptador resolve a dependência circular entre Core e Queue
    /// </summary>
    public class EventPublisherAdapter : IEventPublisher
    {
        private readonly IQueuePublisher _queuePublisher;
        private readonly ILogger<EventPublisherAdapter> _logger;
        private bool _disposed = false;

        public EventPublisherAdapter(IQueuePublisher queuePublisher, ILogger<EventPublisherAdapter> logger)
        {
            _queuePublisher = queuePublisher;
            _logger = logger;
            _logger.LogInformation("🔗 EventPublisherAdapter criado - conectando Core ao Queue");
        }

        public Task PublishEventAsync(string topic, object eventData)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(EventPublisherAdapter));
            }

            try
            {
                // ✅ DELEGAR PARA O IQueuePublisher REAL
                _logger.LogInformation("📤 EventPublisherAdapter: Enviando para fila '{Topic}' - Tipo: {EventType}", topic, eventData?.GetType().Name);
                return _queuePublisher.PublishAsync(topic, eventData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro no adaptador ao publicar evento: {Topic}", topic);
                throw;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _logger.LogInformation("🔗 EventPublisherAdapter finalizado");
            }
        }
    }
}
