namespace AlbionOnlineSniffer.Web.Models
{
    /// <summary>
    /// Modelo de entrada de log para armazenamento em memória
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// ID único da entrada de log
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Timestamp da entrada
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Nível do log
        /// </summary>
        public LogLevel Level { get; set; }

        /// <summary>
        /// Mensagem do log
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Categoria do log
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Dados adicionais do log
        /// </summary>
        public string? Data { get; set; }

        /// <summary>
        /// Exceção (se houver)
        /// </summary>
        public string? Exception { get; set; }

        /// <summary>
        /// ID da sessão/correlation (se disponível)
        /// </summary>
        public string? SessionId { get; set; }

        /// <summary>
        /// ID do cluster (se disponível)
        /// </summary>
        public string? ClusterId { get; set; }

        /// <summary>
        /// Metadados adicionais
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();

        /// <summary>
        /// Cria uma nova entrada de log
        /// </summary>
        public static LogEntry Create(LogLevel level, string message, string category = "", string? data = null, string? exception = null, string? sessionId = null, string? clusterId = null)
        {
            return new LogEntry
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Level = level,
                Message = message,
                Category = category,
                Data = data,
                Exception = exception,
                SessionId = sessionId,
                ClusterId = clusterId
            };
        }

        /// <summary>
        /// Cria uma entrada de log de informação
        /// </summary>
        public static LogEntry Information(string message, string category = "", string? data = null, string? sessionId = null, string? clusterId = null)
        {
            return Create(LogLevel.Information, message, category, data, null, sessionId, clusterId);
        }

        /// <summary>
        /// Cria uma entrada de log de aviso
        /// </summary>
        public static LogEntry Warning(string message, string category = "", string? data = null, string? sessionId = null, string? clusterId = null)
        {
            return Create(LogLevel.Warning, message, category, data, null, sessionId, clusterId);
        }

        /// <summary>
        /// Cria uma entrada de log de erro
        /// </summary>
        public static LogEntry Error(string message, string category = "", string? data = null, string? exception = null, string? sessionId = null, string? clusterId = null)
        {
            return Create(LogLevel.Error, message, category, data, exception, sessionId, clusterId);
        }

        /// <summary>
        /// Cria uma entrada de log de debug
        /// </summary>
        public static LogEntry Debug(string message, string category = "", string? data = null, string? sessionId = null, string? clusterId = null)
        {
            return Create(LogLevel.Debug, message, category, data, null, sessionId, clusterId);
        }

        /// <summary>
        /// Obtém uma representação resumida da entrada de log
        /// </summary>
        public string GetSummary()
        {
            var level = Level.ToString().ToUpperInvariant();
            var category = !string.IsNullOrEmpty(Category) ? $" [{Category}]" : "";
            return $"[{Timestamp:HH:mm:ss.fff}] {level}{category}: {Message}";
        }

        /// <summary>
        /// Verifica se é um log de erro
        /// </summary>
        public bool IsError => Level >= LogLevel.Error;

        /// <summary>
        /// Verifica se é um log de aviso
        /// </summary>
        public bool IsWarning => Level >= LogLevel.Warning;

        /// <summary>
        /// Verifica se é um log de informação
        /// </summary>
        public bool IsInformation => Level >= LogLevel.Information;
    }

    /// <summary>
    /// Níveis de log
    /// </summary>
    public enum LogLevel
    {
        Debug = 0,
        Information = 1,
        Warning = 2,
        Error = 3,
        Critical = 4
    }
}