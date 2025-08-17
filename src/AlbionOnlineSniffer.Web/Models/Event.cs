using System.Numerics;

namespace AlbionOnlineSniffer.Web.Models
{
    /// <summary>
    /// Modelo de evento para armazenamento em memória
    /// </summary>
    public class Event
    {
        /// <summary>
        /// ID único do evento
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Timestamp de criação
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Tipo do evento
        /// </summary>
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// Nome da classe do evento
        /// </summary>
        public string EventClassName { get; set; } = string.Empty;

        /// <summary>
        /// Dados do evento (serializados)
        /// </summary>
        public string EventData { get; set; } = string.Empty;

        /// <summary>
        /// Indica se o evento foi processado com sucesso
        /// </summary>
        public bool IsProcessed { get; set; }

        /// <summary>
        /// Erro de processamento (se houver)
        /// </summary>
        public string? ProcessingError { get; set; }

        /// <summary>
        /// ID da sessão/correlation (se disponível)
        /// </summary>
        public string? SessionId { get; set; }

        /// <summary>
        /// ID do cluster (se disponível)
        /// </summary>
        public string? ClusterId { get; set; }

        /// <summary>
        /// Posição X (se o evento tiver posição)
        /// </summary>
        public float? PositionX { get; set; }

        /// <summary>
        /// Posição Y (se o evento tiver posição)
        /// </summary>
        public float? PositionY { get; set; }

        /// <summary>
        /// Metadados adicionais do evento
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();

        /// <summary>
        /// Cria um novo evento a partir de um objeto de evento do Albion.Network
        /// </summary>
        public static Event Create(object gameEvent, string? sessionId = null, string? clusterId = null)
        {
            var eventType = gameEvent.GetType().Name;
            var eventData = SerializeEventData(gameEvent);
            
            var evt = new Event
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                EventType = eventType,
                EventClassName = gameEvent.GetType().FullName ?? eventType,
                EventData = eventData,
                IsProcessed = true,
                SessionId = sessionId,
                ClusterId = clusterId
            };

            // Tenta extrair posição se o evento implementar IHasPosition
            if (gameEvent is IHasPosition hasPosition)
            {
                var pos = hasPosition.Position;
                evt.PositionX = pos.X;
                evt.PositionY = pos.Y;
            }

            // Tenta extrair posição via reflexão se não implementar IHasPosition
            var posProp = gameEvent.GetType().GetProperty("Position");
            if (posProp != null && posProp.PropertyType == typeof(Vector2))
            {
                var posValue = posProp.GetValue(gameEvent);
                if (posValue != null)
                {
                    var pos = (Vector2)posValue;
                    evt.PositionX = pos.X;
                    evt.PositionY = pos.Y;
                }
            }

            return evt;
        }

        /// <summary>
        /// Serializa os dados do evento para JSON
        /// </summary>
        private static string SerializeEventData(object gameEvent)
        {
            try
            {
                return System.Text.Json.JsonSerializer.Serialize(gameEvent, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = false,
                    MaxDepth = 10
                });
            }
            catch
            {
                return gameEvent.ToString() ?? "Unknown";
            }
        }

        /// <summary>
        /// Obtém uma representação resumida do evento
        /// </summary>
        public string GetSummary()
        {
            var position = PositionX.HasValue && PositionY.HasValue 
                ? $" @ ({PositionX:F1}, {PositionY:F1})" 
                : "";
            
            return $"[{Timestamp:HH:mm:ss.fff}] {EventType}{position}";
        }

        /// <summary>
        /// Verifica se o evento tem posição
        /// </summary>
        public bool HasPosition => PositionX.HasValue && PositionY.HasValue;

        /// <summary>
        /// Obtém a posição como Vector2 (se disponível)
        /// </summary>
        public Vector2? GetPosition()
        {
            if (HasPosition)
                return new Vector2(PositionX.Value, PositionY.Value);
            return null;
        }
    }

    /// <summary>
    /// Interface para eventos que possuem posição
    /// </summary>
    public interface IHasPosition
    {
        Vector2 Position { get; }
    }
}