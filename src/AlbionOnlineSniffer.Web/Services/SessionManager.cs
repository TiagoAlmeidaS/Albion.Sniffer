using AlbionOnlineSniffer.Web.Interfaces;
using AlbionOnlineSniffer.Web.Models;

namespace AlbionOnlineSniffer.Web.Services
{
    /// <summary>
    /// Serviço para gerenciar sessões de eventos
    /// </summary>
    public class SessionManager
    {
        private readonly IInMemoryRepository<Session> _sessions;
        private readonly IInMemoryRepository<Event> _events;
        private readonly IInMemoryRepository<Packet> _packets;
        private readonly IInMemoryRepository<LogEntry> _logs;
        private readonly ILogger<SessionManager> _logger;
        private readonly Dictionary<string, Session> _activeSessions = new();

        public SessionManager(
            IInMemoryRepository<Session> sessions,
            IInMemoryRepository<Event> events,
            IInMemoryRepository<Packet> packets,
            IInMemoryRepository<LogEntry> logs,
            ILogger<SessionManager> logger)
        {
            _sessions = sessions;
            _events = events;
            _packets = packets;
            _logs = logs;
            _logger = logger;
        }

        /// <summary>
        /// Obtém ou cria uma sessão para um evento
        /// </summary>
        public Session GetOrCreateSession(Event evt)
        {
            var sessionId = evt.SessionId ?? evt.ClusterId ?? "default";
            var sessionType = DetermineSessionType(evt);
            var sessionName = DetermineSessionName(evt);

            if (_activeSessions.TryGetValue(sessionId, out var existingSession))
            {
                existingSession.IncrementEventCount();
                existingSession.UpdateActivity();
                return existingSession;
            }

            var newSession = Session.Create(sessionId, sessionType, sessionName, evt.ClusterId);
            _activeSessions[sessionId] = newSession;
            _sessions.Add(newSession);
            
            _logger.LogInformation("Nova sessão criada: {SessionId} ({Type}: {Name})", sessionId, sessionType, sessionName);
            
            return newSession;
        }

        /// <summary>
        /// Obtém ou cria uma sessão para um pacote
        /// </summary>
        public Session GetOrCreateSession(Packet packet)
        {
            var sessionId = packet.Metadata.TryGetValue("SessionId", out var sessionIdObj) 
                ? sessionIdObj?.ToString() ?? "default" 
                : "default";
            
            var sessionType = "network";
            var sessionName = $"UDP {packet.SourceIp}:{packet.SourcePort}";

            if (_activeSessions.TryGetValue(sessionId, out var existingSession))
            {
                existingSession.IncrementPacketCount();
                existingSession.UpdateActivity();
                return existingSession;
            }

            var newSession = Session.Create(sessionId, sessionType, sessionName);
            _activeSessions[sessionId] = newSession;
            _sessions.Add(newSession);
            
            _logger.LogDebug("Nova sessão de rede criada: {SessionId} ({Type}: {Name})", sessionId, sessionType, sessionName);
            
            return newSession;
        }

        /// <summary>
        /// Obtém ou cria uma sessão para um log
        /// </summary>
        public Session GetOrCreateSession(LogEntry log)
        {
            var sessionId = log.SessionId ?? log.ClusterId ?? "default";
            var sessionType = "logging";
            var sessionName = $"Logs {log.Category}";

            if (_activeSessions.TryGetValue(sessionId, out var existingSession))
            {
                existingSession.IncrementLogCount();
                existingSession.UpdateActivity();
                return existingSession;
            }

            var newSession = Session.Create(sessionId, sessionType, sessionName, log.ClusterId);
            _activeSessions[sessionId] = newSession;
            _sessions.Add(newSession);
            
            _logger.LogDebug("Nova sessão de log criada: {SessionId} ({Type}: {Name})", sessionId, sessionType, sessionName);
            
            return newSession;
        }

        /// <summary>
        /// Encerra uma sessão
        /// </summary>
        public void EndSession(string sessionId)
        {
            if (_activeSessions.TryGetValue(sessionId, out var session))
            {
                session.End();
                _activeSessions.Remove(sessionId);
                _logger.LogInformation("Sessão encerrada: {SessionId}", sessionId);
            }
        }

        /// <summary>
        /// Limpa sessões inativas
        /// </summary>
        public void CleanupStaleSessions(TimeSpan threshold)
        {
            var staleSessions = _activeSessions.Values
                .Where(s => s.IsStale(threshold))
                .ToList();

            foreach (var session in staleSessions)
            {
                session.End();
                _activeSessions.Remove(session.Id);
                _logger.LogDebug("Sessão inativa removida: {SessionId}", session.Id);
            }
        }

        /// <summary>
        /// Obtém todas as sessões ativas
        /// </summary>
        public IReadOnlyList<Session> GetActiveSessions()
        {
            return _activeSessions.Values.ToList();
        }

        /// <summary>
        /// Obtém uma sessão específica
        /// </summary>
        public Session? GetSession(string sessionId)
        {
            return _activeSessions.TryGetValue(sessionId, out var session) ? session : null;
        }

        /// <summary>
        /// Determina o tipo de sessão baseado no evento
        /// </summary>
        private string DetermineSessionType(Event evt)
        {
            var eventType = evt.EventType.ToLowerInvariant();
            
            if (eventType.Contains("cluster"))
                return "cluster";
            if (eventType.Contains("dungeon"))
                return "dungeon";
            if (eventType.Contains("fishing"))
                return "fishing";
            if (eventType.Contains("harvest"))
                return "harvesting";
            if (eventType.Contains("mob") || eventType.Contains("combat"))
                return "combat";
            if (eventType.Contains("move"))
                return "movement";
            
            return "general";
        }

        /// <summary>
        /// Determina o nome da sessão baseado no evento
        /// </summary>
        private string DetermineSessionName(Event evt)
        {
            var eventType = evt.EventType;
            
            // Tenta extrair informações específicas do evento
            if (evt.EventData.Contains("cluster"))
            {
                // Procura por nome do cluster nos dados do evento
                // Implementar lógica específica conforme necessário
            }
            
            return $"{eventType} Session";
        }
    }
}