using System.Diagnostics;

namespace AlbionOnlineSniffer.Core.Observability
{
    /// <summary>
    /// Serviço principal de observabilidade para o Albion.Sniffer
    /// Coordena métricas, traces, logs e health checks
    /// </summary>
    public interface IObservabilityService
    {
        /// <summary>
        /// Inicializa o sistema de observabilidade
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Cria um novo trace para uma operação
        /// </summary>
        Activity? StartTrace(string operationName, string? operationId = null);

        /// <summary>
        /// Registra uma métrica customizada
        /// </summary>
        void RecordMetric(string name, double value, params KeyValuePair<string, object?>[] tags);

        /// <summary>
        /// Incrementa um contador
        /// </summary>
        void IncrementCounter(string name, params KeyValuePair<string, object?>[] tags);

        /// <summary>
        /// Registra um evento de negócio
        /// </summary>
        void RecordBusinessEvent(string eventName, string category, params KeyValuePair<string, object?>[] properties);

        /// <summary>
        /// Verifica a saúde geral do sistema
        /// </summary>
        Task<HealthReport> CheckHealthAsync();

        /// <summary>
        /// Obtém métricas em formato Prometheus
        /// </summary>
        string GetPrometheusMetrics();

        /// <summary>
        /// Obtém métricas em formato JSON
        /// </summary>
        object GetJsonMetrics();

        /// <summary>
        /// Limpa métricas antigas
        /// </summary>
        void ClearOldMetrics(TimeSpan age);
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
    }
}
