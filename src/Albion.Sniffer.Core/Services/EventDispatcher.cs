using Microsoft.Extensions.Logging;
using Albion.Network;

namespace Albion.Sniffer.Core.Services
{
    /// <summary>
    /// Sistema centralizado de despacho de eventos do jogo.
    /// Permite registrar handlers para tipos específicos de eventos ou para todos os eventos.
    /// </summary>
    public class EventDispatcher
    {
        private readonly ILogger<EventDispatcher> _logger;
        private readonly Dictionary<string, List<Func<object, Task>>> _eventHandlers = new();
        private readonly List<Func<object, Task>> _globalHandlers = new();

        public EventDispatcher(ILogger<EventDispatcher> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Registra um handler para um tipo específico de evento
        /// </summary>
        /// <param name="eventType">Tipo do evento (ex: "NewCharacter", "Move")</param>
        /// <param name="handler">Função que processa o evento</param>
        public void RegisterHandler(string eventType, Func<object, Task> handler)
        {
            if (!_eventHandlers.ContainsKey(eventType))
                _eventHandlers[eventType] = new List<Func<object, Task>>();

            _eventHandlers[eventType].Add(handler);
            _logger.LogDebug("Handler registrado para evento: {EventType}", eventType);
        }

        /// <summary>
        /// Registra um handler global que recebe todos os eventos
        /// </summary>
        /// <param name="handler">Função que processa todos os eventos</param>
        public void RegisterGlobalHandler(Func<object, Task> handler)
        {
            _globalHandlers.Add(handler);
            _logger.LogDebug("Handler global registrado");
        }

        /// <summary>
        /// Despacha um evento para todos os handlers registrados
        /// </summary>
        /// <param name="gameEvent">Evento a ser disparado (BaseEvent ou BaseOperation)</param>
        public async Task DispatchEvent(object gameEvent)
        {
            try
            {
                var tasks = new List<Task>();

                // Handlers específicos para o tipo de evento
                var eventType = gameEvent.GetType().Name;
                if (_eventHandlers.ContainsKey(eventType))
                {
                    foreach (var handler in _eventHandlers[eventType])
                    {
                        tasks.Add(handler(gameEvent));
                    }
                }

                // Handlers globais
                foreach (var handler in _globalHandlers)
                {
                    tasks.Add(handler(gameEvent));
                }

                // Executa todos os handlers em paralelo
                if (tasks.Any())
                {
                    await Task.WhenAll(tasks);
                }

                _logger.LogDebug("Evento disparado com sucesso: {EventType} para {HandlerCount} handlers", 
                    eventType, tasks.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao disparar evento: {EventType}", gameEvent.GetType().Name);
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
        /// Retorna o número de handlers registrados para um tipo específico de evento
        /// </summary>
        /// <param name="eventType">Tipo do evento ou "*" para todos</param>
        /// <returns>Número de handlers</returns>
        public int GetHandlerCount(string eventType)
        {
            if (eventType == "*")
                return _globalHandlers.Count;

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