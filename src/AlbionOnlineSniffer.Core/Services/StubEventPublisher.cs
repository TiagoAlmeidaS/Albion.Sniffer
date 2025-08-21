using AlbionOnlineSniffer.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AlbionOnlineSniffer.Core.Services
{
    /// <summary>
    /// Implementação stub do IEventPublisher para o projeto Core
    /// Esta implementação é usada quando o projeto Queue não está disponível
    /// </summary>
    public class StubEventPublisher : IEventPublisher
    {
        private readonly ILogger<StubEventPublisher> _logger;
        private bool _disposed = false;

        public StubEventPublisher(ILogger<StubEventPublisher> logger)
        {
            _logger = logger;
            _logger.LogInformation("🔍 StubEventPublisher criado - DiscoveryService funcionará em modo stub");
        }

        public Task PublishEventAsync(string topic, object eventData)
        {
            _logger.LogDebug("🚫 StubEventPublisher: Publicação para o tópico '{Topic}' com mensagem (tipo: {MessageType}) ignorada.", topic, eventData?.GetType().Name);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _logger.LogInformation("🗑️ StubEventPublisher descartado.");
                }
                _disposed = true;
            }
        }
    }
}
