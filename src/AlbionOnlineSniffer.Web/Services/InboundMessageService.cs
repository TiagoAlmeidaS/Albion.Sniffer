using AlbionOnlineSniffer.Web.Interfaces;
using AlbionOnlineSniffer.Web.Models;
using Microsoft.AspNetCore.SignalR;
using AlbionOnlineSniffer.Web.Hubs;

namespace AlbionOnlineSniffer.Web.Services
{
    /// <summary>
    /// Serviço para processar mensagens recebidas da fila
    /// </summary>
    public class InboundMessageService
    {
        private readonly IInMemoryRepository<Packet> _packets;
        private readonly IInMemoryRepository<Event> _events;
        private readonly IInMemoryRepository<LogEntry> _logs;
        private readonly SessionManager _sessionManager;
        private readonly IHubContext<SnifferHub> _hub;
        private readonly ILogger<InboundMessageService> _logger;

        public InboundMessageService(
            IInMemoryRepository<Packet> packets,
            IInMemoryRepository<Event> events,
            IInMemoryRepository<LogEntry> logs,
            SessionManager sessionManager,
            IHubContext<SnifferHub> hub,
            ILogger<InboundMessageService> logger)
        {
            _packets = packets;
            _events = events;
            _logs = logs;
            _sessionManager = sessionManager;
            _hub = hub;
            _logger = logger;
        }

        /// <summary>
        /// Processa uma mensagem de pacote recebida
        /// </summary>
        public async Task HandlePacketAsync(ReadOnlyMemory<byte> raw, string routingKey, string? sourceIp = null, int? sourcePort = null)
        {
            try
            {
                var packet = Packet.Create(raw.ToArray(), routingKey, sourceIp, sourcePort);
                
                // Adiciona metadados adicionais
                packet.Metadata["ProcessingTimestamp"] = DateTime.UtcNow;
                packet.Metadata["QueueRoutingKey"] = routingKey;

                // Salva no repositório
                _packets.Add(packet);

                // Cria ou atualiza sessão
                var session = _sessionManager.GetOrCreateSession(packet);

                // Log da operação
                var logEntry = LogEntry.Information(
                    $"Pacote recebido: {packet.Size} bytes via {routingKey}",
                    "PacketProcessor",
                    $"Size: {packet.Size}, Session: {session.Id}",
                    sessionId: session.Id,
                    clusterId: session.ClusterId
                );
                _logs.Add(logEntry);

                // Emite via SignalR
                await _hub.Clients.All.SendAsync("packet:new", new
                {
                    packet.Id,
                    packet.Timestamp,
                    packet.Size,
                    packet.RoutingKey,
                    packet.HexPreview,
                    packet.SourceIp,
                    packet.SourcePort,
                    SessionId = session.Id,
                    SessionType = session.Type,
                    SessionName = session.Name
                });

                _logger.LogDebug("Pacote processado: {PacketId} ({Size} bytes) via {RoutingKey}", 
                    packet.Id, packet.Size, routingKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar pacote: {Message}", ex.Message);
                
                // Log do erro
                var errorLog = LogEntry.Error(
                    $"Erro ao processar pacote: {ex.Message}",
                    "PacketProcessor",
                    $"RoutingKey: {routingKey}, Size: {raw.Length}",
                    ex.ToString()
                );
                _logs.Add(errorLog);
            }
        }

        /// <summary>
        /// Processa um evento de jogo recebido
        /// </summary>
        public async Task HandleGameEventAsync(object gameEvent, string? sessionId = null, string? clusterId = null)
        {
            try
            {
                var evt = Event.Create(gameEvent, sessionId, clusterId);
                
                // Salva no repositório
                _events.Add(evt);

                // Cria ou atualiza sessão
                var session = _sessionManager.GetOrCreateSession(evt);

                // Log da operação
                var logEntry = LogEntry.Information(
                    $"Evento processado: {evt.EventType}",
                    "EventProcessor",
                    $"EventType: {evt.EventType}, Session: {session.Id}",
                    sessionId: session.Id,
                    clusterId: session.ClusterId
                );
                _logs.Add(logEntry);

                // Emite via SignalR
                await _hub.Clients.All.SendAsync("gameEvent:new", new
                {
                    evt.Id,
                    evt.Timestamp,
                    evt.EventType,
                    evt.EventClassName,
                    evt.EventData,
                    evt.PositionX,
                    evt.PositionY,
                    SessionId = session.Id,
                    SessionType = session.Type,
                    SessionName = session.Name
                });

                _logger.LogDebug("Evento processado: {EventId} ({EventType}) na sessão {SessionId}", 
                    evt.Id, evt.EventType, session.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar evento: {Message}", ex.Message);
                
                // Log do erro
                var errorLog = LogEntry.Error(
                    $"Erro ao processar evento: {ex.Message}",
                    "EventProcessor",
                    $"EventType: {gameEvent.GetType().Name}",
                    ex.ToString(),
                    sessionId,
                    clusterId
                );
                _logs.Add(errorLog);
            }
        }

        /// <summary>
        /// Processa uma entrada de log
        /// </summary>
        public async Task HandleLogEntryAsync(LogEntry logEntry)
        {
            try
            {
                // Salva no repositório
                _logs.Add(logEntry);

                // Cria ou atualiza sessão
                var session = _sessionManager.GetOrCreateSession(logEntry);

                // Emite via SignalR
                await _hub.Clients.All.SendAsync("log:new", new
                {
                    logEntry.Id,
                    logEntry.Timestamp,
                    logEntry.Level,
                    logEntry.Message,
                    logEntry.Category,
                    logEntry.Data,
                    logEntry.Exception,
                    SessionId = session.Id,
                    SessionType = session.Type,
                    SessionName = session.Name
                });

                _logger.LogDebug("Log processado: {LogId} ({Level}) na sessão {SessionId}", 
                    logEntry.Id, logEntry.Level, session.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar log: {Message}", ex.Message);
            }
        }

        /// <summary>
        /// Processa uma mensagem de erro
        /// </summary>
        public async Task HandleErrorAsync(string message, string category, string? details = null, string? sessionId = null, string? clusterId = null)
        {
            var logEntry = LogEntry.Error(message, category, details, sessionId: sessionId, clusterId: clusterId);
            await HandleLogEntryAsync(logEntry);
        }

        /// <summary>
        /// Processa uma mensagem de informação
        /// </summary>
        public async Task HandleInformationAsync(string message, string category, string? details = null, string? sessionId = null, string? clusterId = null)
        {
            var logEntry = LogEntry.Information(message, category, details, sessionId: sessionId, clusterId: clusterId);
            await HandleLogEntryAsync(logEntry);
        }
    }
}