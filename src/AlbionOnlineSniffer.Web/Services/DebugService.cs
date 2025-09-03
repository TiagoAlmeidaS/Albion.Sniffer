using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;

namespace AlbionOnlineSniffer.Web.Services
{
    /// <summary>
    /// Serviço para debug e monitoramento de dados não preenchidos
    /// </summary>
    public class DebugService
    {
        private readonly ILogger<DebugService> _logger;
        private readonly ConcurrentDictionary<string, DebugInfo> _debugData = new();
        private readonly ConcurrentQueue<DebugLogEntry> _recentLogs = new();
        private readonly object _lock = new object();

        public DebugService(ILogger<DebugService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Registra informações de debug para um evento
        /// </summary>
        public void LogEventDebug(string eventType, string eventName, object eventData, string source = "Unknown")
        {
            try
            {
                var debugInfo = new DebugInfo
                {
                    EventType = eventType,
                    EventName = eventName,
                    Source = source,
                    Timestamp = DateTimeOffset.UtcNow,
                    Data = eventData,
                    IsEmpty = IsEventDataEmpty(eventData),
                    ParameterCount = GetParameterCount(eventData),
                    HasOpcode = HasOpcode(eventData)
                };

                var key = $"{eventType}_{eventName}_{source}";
                _debugData.AddOrUpdate(key, debugInfo, (k, v) => debugInfo);

                // Adicionar ao log recente
                var logEntry = new DebugLogEntry
                {
                    Timestamp = DateTimeOffset.UtcNow,
                    EventType = eventType,
                    EventName = eventName,
                    Source = source,
                    IsEmpty = debugInfo.IsEmpty,
                    ParameterCount = debugInfo.ParameterCount,
                    HasOpcode = debugInfo.HasOpcode,
                    Message = GenerateDebugMessage(debugInfo)
                };

                _recentLogs.Enqueue(logEntry);
                
                // Manter apenas os últimos 100 logs
                while (_recentLogs.Count > 100)
                {
                    _recentLogs.TryDequeue(out _);
                }

                // Log detalhado se dados estão vazios
                if (debugInfo.IsEmpty)
                {
                    _logger.LogWarning("🔍 DEBUG: Evento com dados vazios detectado - {EventType}:{EventName} - Source: {Source} - Params: {ParamCount} - Opcode: {HasOpcode}", 
                        eventType, eventName, source, debugInfo.ParameterCount, debugInfo.HasOpcode);
                }
                else
                {
                    _logger.LogDebug("🔍 DEBUG: Evento processado - {EventType}:{EventName} - Source: {Source} - Params: {ParamCount} - Opcode: {HasOpcode}", 
                        eventType, eventName, source, debugInfo.ParameterCount, debugInfo.HasOpcode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar debug para evento {EventType}:{EventName}", eventType, eventName);
            }
        }

        /// <summary>
        /// Obtém estatísticas de debug
        /// </summary>
        public object GetDebugStatistics()
        {
            try
            {
                var totalEvents = _debugData.Count;
                var emptyEvents = _debugData.Values.Count(d => d.IsEmpty);
                var eventsWithOpcode = _debugData.Values.Count(d => d.HasOpcode);
                var eventsWithParams = _debugData.Values.Count(d => d.ParameterCount > 0);

                var sourceStats = _debugData.Values
                    .GroupBy(d => d.Source)
                    .Select(g => new
                    {
                        source = g.Key,
                        total = g.Count(),
                        empty = g.Count(x => x.IsEmpty),
                        withOpcode = g.Count(x => x.HasOpcode),
                        withParams = g.Count(x => x.ParameterCount > 0)
                    })
                    .ToList();

                var recentLogs = _recentLogs.Take(20).Select(log => new
                {
                    timestamp = log.Timestamp,
                    eventType = log.EventType,
                    eventName = log.EventName,
                    source = log.Source,
                    isEmpty = log.IsEmpty,
                    parameterCount = log.ParameterCount,
                    hasOpcode = log.HasOpcode,
                    message = log.Message
                }).ToList();

                return new
                {
                    timestamp = DateTimeOffset.UtcNow,
                    summary = new
                    {
                        totalEvents,
                        emptyEvents,
                        eventsWithOpcode,
                        eventsWithParams,
                        emptyPercentage = totalEvents > 0 ? (double)emptyEvents / totalEvents * 100 : 0,
                        opcodePercentage = totalEvents > 0 ? (double)eventsWithOpcode / totalEvents * 100 : 0,
                        paramsPercentage = totalEvents > 0 ? (double)eventsWithParams / totalEvents * 100 : 0
                    },
                    sourceStatistics = sourceStats,
                    recentLogs
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estatísticas de debug");
                return new { error = ex.Message };
            }
        }

        /// <summary>
        /// Obtém detalhes de debug para um evento específico
        /// </summary>
        public object GetEventDebugDetails(string eventType, string eventName, string source = null)
        {
            try
            {
                var key = source != null ? $"{eventType}_{eventName}_{source}" : $"{eventType}_{eventName}";
                
                if (_debugData.TryGetValue(key, out var debugInfo))
                {
                    return new
                    {
                        eventType = debugInfo.EventType,
                        eventName = debugInfo.EventName,
                        source = debugInfo.Source,
                        timestamp = debugInfo.Timestamp,
                        isEmpty = debugInfo.IsEmpty,
                        parameterCount = debugInfo.ParameterCount,
                        hasOpcode = debugInfo.HasOpcode,
                        data = debugInfo.Data,
                        message = GenerateDebugMessage(debugInfo)
                    };
                }

                return new { error = "Evento não encontrado" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter detalhes de debug para {EventType}:{EventName}", eventType, eventName);
                return new { error = ex.Message };
            }
        }

        /// <summary>
        /// Limpa dados de debug
        /// </summary>
        public void ClearDebugData()
        {
            _debugData.Clear();
            while (_recentLogs.TryDequeue(out _)) { }
            _logger.LogInformation("🧹 Dados de debug limpos");
        }

        private bool IsEventDataEmpty(object eventData)
        {
            if (eventData == null) return true;

            try
            {
                // Verificar se é um objeto com propriedades
                var type = eventData.GetType();
                var properties = type.GetProperties();
                
                if (properties.Length == 0) return true;

                // Verificar se todas as propriedades são nulas ou vazias
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(eventData);
                    if (value != null && !IsEmptyValue(value))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool IsEmptyValue(object value)
        {
            if (value == null) return true;
            if (value is string str) return string.IsNullOrWhiteSpace(str);
            if (value is System.Collections.ICollection collection) return collection.Count == 0;
            if (value is System.Collections.IDictionary dict) return dict.Count == 0;
            return false;
        }

        private int GetParameterCount(object eventData)
        {
            if (eventData == null) return 0;

            try
            {
                var type = eventData.GetType();
                var parametersProperty = type.GetProperty("Parameters");
                if (parametersProperty != null)
                {
                    var parameters = parametersProperty.GetValue(eventData);
                    if (parameters is System.Collections.ICollection collection)
                    {
                        return collection.Count;
                    }
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        private bool HasOpcode(object eventData)
        {
            if (eventData == null) return false;

            try
            {
                var type = eventData.GetType();
                var packetCodeProperty = type.GetProperty("PacketCode");
                if (packetCodeProperty != null)
                {
                    var opcode = packetCodeProperty.GetValue(eventData);
                    return opcode != null && !string.IsNullOrEmpty(opcode.ToString()) && opcode.ToString() != "N/A";
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private string GenerateDebugMessage(DebugInfo debugInfo)
        {
            var issues = new List<string>();
            
            if (debugInfo.IsEmpty)
                issues.Add("Dados vazios");
            
            if (!debugInfo.HasOpcode)
                issues.Add("Sem opcode");
            
            if (debugInfo.ParameterCount == 0)
                issues.Add("Sem parâmetros");

            if (issues.Count == 0)
                return "✅ Evento processado corretamente";
            
            return $"⚠️ Problemas detectados: {string.Join(", ", issues)}";
        }
    }

    /// <summary>
    /// Informações de debug para um evento
    /// </summary>
    public class DebugInfo
    {
        public string EventType { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public DateTimeOffset Timestamp { get; set; }
        public object Data { get; set; } = new object();
        public bool IsEmpty { get; set; }
        public int ParameterCount { get; set; }
        public bool HasOpcode { get; set; }
    }

    /// <summary>
    /// Entrada de log de debug
    /// </summary>
    public class DebugLogEntry
    {
        public DateTimeOffset Timestamp { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public bool IsEmpty { get; set; }
        public int ParameterCount { get; set; }
        public bool HasOpcode { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}