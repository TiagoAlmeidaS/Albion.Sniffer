namespace AlbionOnlineSniffer.Core.Observability.HealthChecks
{
    /// <summary>
    /// Serviço de health checks para monitorar a saúde do sistema
    /// </summary>
    public interface IHealthCheckService
    {
        /// <summary>
        /// Executa todos os health checks
        /// </summary>
        Task<HealthReport> RunHealthChecksAsync();

        /// <summary>
        /// Executa um health check específico
        /// </summary>
        Task<HealthCheckResult> RunHealthCheckAsync(string checkName);

        /// <summary>
        /// Registra um novo health check customizado
        /// </summary>
        void RegisterHealthCheck(string name, Func<Task<HealthCheckResult>> checkFunction);

        /// <summary>
        /// Remove um health check
        /// </summary>
        void UnregisterHealthCheck(string name);

        /// <summary>
        /// Obtém a lista de health checks registrados
        /// </summary>
        IEnumerable<string> GetRegisteredHealthChecks();

        /// <summary>
        /// Verifica se o sistema está saudável
        /// </summary>
        bool IsSystemHealthy();
    }

    /// <summary>
    /// Relatório de saúde do sistema
    /// </summary>
    public class HealthReport
    {
        public string Status { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public TimeSpan Duration { get; set; }
        public List<HealthCheckResult> Checks { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
        public string? Summary { get; set; }
    }

    /// <summary>
    /// Resultado de um health check individual
    /// </summary>
    public class HealthCheckResult
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TimeSpan Duration { get; set; }
        public Dictionary<string, object> Data { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        public Exception? Exception { get; set; }
    }

    /// <summary>
    /// Status de um health check
    /// </summary>
    public static class HealthStatus
    {
        public const string Healthy = "Healthy";
        public const string Degraded = "Degraded";
        public const string Unhealthy = "Unhealthy";
        public const string Unknown = "Unknown";
    }
}
