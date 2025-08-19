using System.Diagnostics;

namespace AlbionOnlineSniffer.Core.Observability.Tracing
{
    /// <summary>
    /// Serviço de tracing distribuído usando OpenTelemetry
    /// </summary>
    public interface ITracingService
    {
        /// <summary>
        /// Inicializa o sistema de tracing
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Cria um novo trace para uma operação
        /// </summary>
        Activity? StartTrace(string operationName, string? operationId = null);

        /// <summary>
        /// Cria um span filho do trace atual
        /// </summary>
        Activity? StartSpan(string spanName, ActivityKind kind = ActivityKind.Internal);

        /// <summary>
        /// Adiciona um evento ao trace atual
        /// </summary>
        void AddEvent(string eventName, params KeyValuePair<string, object?>[] attributes);

        /// <summary>
        /// Adiciona um tag ao trace atual
        /// </summary>
        void AddTag(string key, object? value);

        /// <summary>
        /// Adiciona um erro ao trace atual
        /// </summary>
        void RecordException(Exception exception);

        /// <summary>
        /// Finaliza o trace atual
        /// </summary>
        void EndTrace(string? status = null);

        /// <summary>
        /// Obtém o trace atual
        /// </summary>
        Activity? GetCurrentTrace();

        /// <summary>
        /// Obtém o correlation ID atual
        /// </summary>
        string? GetCurrentCorrelationId();

        /// <summary>
        /// Define o correlation ID para o trace atual
        /// </summary>
        void SetCorrelationId(string correlationId);
    }

    /// <summary>
    /// Status de um trace
    /// </summary>
    public static class TraceStatus
    {
        public const string Ok = "Ok";
        public const string Error = "Error";
        public const string Unset = "Unset";
    }
}
