using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Models.Events;
using Microsoft.Extensions.Logging;

namespace AlbionOnlineSniffer.Core.Services
{
    /// <summary>
    /// Disparador de eventos para diferentes tipos de serviços
    /// </summary>
    public class EventDispatcher
    {
        private readonly ILogger<EventDispatcher> _logger;
        private readonly Dictionary<string, List<Func<GameEvent, Task>>> _eventHandlers = new();

        public EventDispatcher(ILogger<EventDispatcher> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Registra um handler para um tipo específico de evento
        /// </summary>
        /// <param name="eventType">Tipo do evento</param>
        /// <param name="handler">Handler do evento</param>
        public void RegisterHandler(string eventType, Func<GameEvent, Task> handler)
        {
            if (!_eventHandlers.ContainsKey(eventType))
            {
                _eventHandlers[eventType] = new List<Func<GameEvent, Task>>();
            }

            _eventHandlers[eventType].Add(handler);
            _logger.LogDebug("Handler registrado para evento: {EventType}", eventType);
        }

        /// <summary>
        /// Registra um handler para todos os eventos
        /// </summary>
        /// <param name="handler">Handler do evento</param>
        public void RegisterGlobalHandler(Func<GameEvent, Task> handler)
        {
            RegisterHandler("*", handler);
        }

        /// <summary>
        /// Dispara um evento para todos os handlers registrados
        /// </summary>
        /// <param name="gameEvent">Evento a ser disparado</param>
        public async Task DispatchEvent(GameEvent gameEvent)
        {
            try
            {
                var eventType = gameEvent.EventType;
                var tasks = new List<Task>();

                // Disparar para handlers específicos do tipo
                if (_eventHandlers.ContainsKey(eventType))
                {
                    foreach (var handler in _eventHandlers[eventType])
                    {
                        tasks.Add(handler(gameEvent));
                    }
                }

                // Disparar para handlers globais
                if (_eventHandlers.ContainsKey("*"))
                {
                    foreach (var handler in _eventHandlers["*"])
                    {
                        tasks.Add(handler(gameEvent));
                    }
                }

                if (tasks.Count > 0)
                {
                    await Task.WhenAll(tasks);
                    _logger.LogDebug("Evento disparado: {EventType} para {HandlerCount} handlers", 
                        eventType, tasks.Count);
                }
                else
                {
                    _logger.LogDebug("Evento disparado: {EventType} (sem handlers registrados)", eventType);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao disparar evento: {EventType}", gameEvent.EventType);
            }
        }

        /// <summary>
        /// Remove todos os handlers de um tipo específico
        /// </summary>
        /// <param name="eventType">Tipo do evento</param>
        public void UnregisterHandlers(string eventType)
        {
            if (_eventHandlers.ContainsKey(eventType))
            {
                _eventHandlers.Remove(eventType);
                _logger.LogDebug("Handlers removidos para evento: {EventType}", eventType);
            }
        }

        /// <summary>
        /// Remove todos os handlers
        /// </summary>
        public void ClearHandlers()
        {
            _eventHandlers.Clear();
            _logger.LogDebug("Todos os handlers foram removidos");
        }

        /// <summary>
        /// Obtém a contagem de handlers para um tipo específico
        /// </summary>
        /// <param name="eventType">Tipo do evento</param>
        /// <returns>Número de handlers</returns>
        public int GetHandlerCount(string eventType)
        {
            return _eventHandlers.ContainsKey(eventType) ? _eventHandlers[eventType].Count : 0;
        }

        /// <summary>
        /// Obtém todos os tipos de eventos registrados
        /// </summary>
        /// <returns>Lista de tipos de eventos</returns>
        public IEnumerable<string> GetRegisteredEventTypes()
        {
            return _eventHandlers.Keys;
        }
    }
} 