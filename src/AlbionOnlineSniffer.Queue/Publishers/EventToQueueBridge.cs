using System;
using System.Numerics;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Queue.Interfaces;
using Microsoft.Extensions.Logging;

namespace AlbionOnlineSniffer.Queue.Publishers
{
    /// <summary>
    /// Faz a ponte entre o EventDispatcher (Core) e o IQueuePublisher (Queue)
    /// e publica cada evento recebido em um tópico padronizado.
    /// </summary>
    public sealed class EventToQueueBridge
    {
        private readonly EventDispatcher _eventDispatcher;
        private readonly IQueuePublisher _publisher;
        private readonly ILogger<EventToQueueBridge> _logger;

        public EventToQueueBridge(EventDispatcher eventDispatcher, IQueuePublisher publisher, ILogger<EventToQueueBridge> logger)
        {
            _eventDispatcher = eventDispatcher;
            _publisher = publisher;
            _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<EventToQueueBridge>.Instance;

            _eventDispatcher.RegisterGlobalHandler(OnEventAsync);
        }

        private async Task OnEventAsync(object gameEvent)
        {
            try
            {
                var eventType = gameEvent.GetType().Name;
                var timestamp = DateTime.UtcNow;

                _logger.LogInformation("🎯 EVENTO RECEBIDO: {EventType} em {Timestamp}", eventType, timestamp);

                var eventTypeFormatted = eventType.EndsWith("Event", StringComparison.Ordinal)
                    ? eventType.Substring(0, eventType.Length - "Event".Length)
                    : eventType;

                object? location = null;
                try
                {
                    if (gameEvent is AlbionOnlineSniffer.Core.Models.Events.IHasPosition hasPosition)
                    {
                        var pos = hasPosition.Position;
                        location = new { X = pos.X, Y = pos.Y };
                    }
                    else
                    {
                        var posProp = gameEvent.GetType().GetProperty("Position");
                        if (posProp != null && posProp.PropertyType == typeof(Vector2))
                        {
                            var pos = (Vector2)posProp.GetValue(gameEvent);
                            location = new { X = pos.X, Y = pos.Y };
                        }
                    }
                }
                catch { }

                var topic = $"albion.event.{eventTypeFormatted.ToLowerInvariant()}";
                var message = new
                {
                    EventType = eventType,
                    Timestamp = timestamp,
                    Position = location,
                    Data = gameEvent
                };

                _logger.LogInformation("📤 PUBLICANDO: {EventType} -> {Topic}", eventType, topic);
                await _publisher.PublishAsync(topic, message);
                _logger.LogInformation("✅ Evento publicado na fila: {EventType} -> {Topic}", eventType, topic);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao publicar evento na fila: {EventType} - {Message}", gameEvent.GetType().Name, ex.Message);
            }
        }
    }
}


