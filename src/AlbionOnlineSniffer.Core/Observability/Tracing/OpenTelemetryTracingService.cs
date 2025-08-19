using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace AlbionOnlineSniffer.Core.Observability.Tracing
{
    /// <summary>
    /// Implementação do serviço de tracing usando OpenTelemetry
    /// </summary>
    public class OpenTelemetryTracingService : ITracingService
    {
        private readonly ILogger<OpenTelemetryTracingService> _logger;
        private readonly ActivitySource _activitySource;
        private readonly AsyncLocal<Activity?> _currentActivity;
        private readonly AsyncLocal<string?> _currentCorrelationId;

        public OpenTelemetryTracingService(ILogger<OpenTelemetryTracingService> logger)
        {
            _logger = logger;
            _activitySource = new ActivitySource("AlbionOnlineSniffer", "1.0.0");
            _currentActivity = new AsyncLocal<Activity?>();
            _currentCorrelationId = new AsyncLocal<string?>();

            _logger.LogInformation("OpenTelemetryTracingService inicializado");
        }

        public Task InitializeAsync()
        {
            try
            {
                // Configuração básica do OpenTelemetry
                // Em produção, isso seria configurado no Program.cs
                _logger.LogInformation("Sistema de tracing inicializado");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inicializar sistema de tracing");
                throw;
            }
        }

        public Activity? StartTrace(string operationName, string? operationId = null)
        {
            try
            {
                var activity = _activitySource.StartActivity(operationName);
                if (activity != null)
                {
                    _currentActivity.Value = activity;
                    
                    // Definir correlation ID se fornecido
                    if (!string.IsNullOrEmpty(operationId))
                    {
                        _currentCorrelationId.Value = operationId;
                        activity.SetTag("correlation.id", operationId);
                    }
                    else
                    {
                        // Gerar correlation ID automático
                        var correlationId = Guid.NewGuid().ToString("N");
                        _currentCorrelationId.Value = correlationId;
                        activity.SetTag("correlation.id", correlationId);
                    }

                    // Adicionar tags padrão
                    activity.SetTag("service.name", "AlbionOnlineSniffer");
                    activity.SetTag("service.version", "1.0.0");
                    activity.SetTag("operation.name", operationName);

                    _logger.LogDebug("Trace iniciado: {OperationName} com correlation ID {CorrelationId}", 
                        operationName, _currentCorrelationId.Value);
                }

                return activity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao iniciar trace {OperationName}", operationName);
                return null;
            }
        }

        public Activity? StartSpan(string spanName, ActivityKind kind = ActivityKind.Internal)
        {
            try
            {
                var parentActivity = _currentActivity.Value;
                if (parentActivity == null)
                {
                    _logger.LogWarning("Tentativa de criar span sem trace pai ativo");
                    return null;
                }

                var span = _activitySource.StartActivity(spanName, kind, parentActivity.Context);
                if (span != null)
                {
                    span.SetTag("span.name", spanName);
                    span.SetTag("span.kind", kind.ToString());
                    
                    _logger.LogDebug("Span iniciado: {SpanName} do tipo {Kind}", spanName, kind);
                }

                return span;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao iniciar span {SpanName}", spanName);
                return null;
            }
        }

        public void AddEvent(string eventName, params KeyValuePair<string, object?>[] attributes)
        {
            try
            {
                var activity = _currentActivity.Value;
                if (activity == null)
                {
                    _logger.LogWarning("Tentativa de adicionar evento sem trace ativo");
                    return;
                }

                var eventAttributes = attributes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                activity.AddEvent(new ActivityEvent(eventName, default, new ActivityTagsCollection(eventAttributes)));

                _logger.LogDebug("Evento adicionado ao trace: {EventName} com {AttributeCount} atributos", 
                    eventName, attributes.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar evento {EventName}", eventName);
            }
        }

        public void AddTag(string key, object? value)
        {
            try
            {
                var activity = _currentActivity.Value;
                if (activity == null)
                {
                    _logger.LogWarning("Tentativa de adicionar tag sem trace ativo");
                    return;
                }

                activity.SetTag(key, value);
                _logger.LogDebug("Tag adicionada ao trace: {Key} = {Value}", key, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar tag {Key}", key);
            }
        }

        public void RecordException(Exception exception)
        {
            try
            {
                var activity = _currentActivity.Value;
                if (activity == null)
                {
                    _logger.LogWarning("Tentativa de registrar exceção sem trace ativo");
                    return;
                }

                activity.RecordException(exception);
                activity.SetTag("error", true);
                activity.SetTag("error.message", exception.Message);
                activity.SetTag("error.type", exception.GetType().Name);

                _logger.LogDebug("Exceção registrada no trace: {ExceptionType}: {Message}", 
                    exception.GetType().Name, exception.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar exceção no trace");
            }
        }

        public void EndTrace(string? status = null)
        {
            try
            {
                var activity = _currentActivity.Value;
                if (activity == null)
                {
                    _logger.LogWarning("Tentativa de finalizar trace sem trace ativo");
                    return;
                }

                // Definir status se fornecido
                if (!string.IsNullOrEmpty(status))
                {
                    activity.SetTag("status", status);
                }

                // Finalizar a atividade
                activity.Stop();

                _logger.LogDebug("Trace finalizado: {OperationName} com status {Status}", 
                    activity.OperationName, status ?? "não especificado");

                // Limpar referências
                _currentActivity.Value = null;
                _currentCorrelationId.Value = null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao finalizar trace");
            }
        }

        public Activity? GetCurrentTrace()
        {
            return _currentActivity.Value;
        }

        public string? GetCurrentCorrelationId()
        {
            return _currentCorrelationId.Value;
        }

        public void SetCorrelationId(string correlationId)
        {
            try
            {
                if (string.IsNullOrEmpty(correlationId))
                {
                    _logger.LogWarning("Tentativa de definir correlation ID vazio");
                    return;
                }

                var activity = _currentActivity.Value;
                if (activity != null)
                {
                    activity.SetTag("correlation.id", correlationId);
                }

                _currentCorrelationId.Value = correlationId;
                _logger.LogDebug("Correlation ID definido: {CorrelationId}", correlationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao definir correlation ID {CorrelationId}", correlationId);
            }
        }

        public void Dispose()
        {
            _activitySource?.Dispose();
        }
    }
}
