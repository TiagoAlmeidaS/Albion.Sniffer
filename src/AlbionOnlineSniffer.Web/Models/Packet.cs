namespace AlbionOnlineSniffer.Web.Models
{
    /// <summary>
    /// Modelo de pacote para armazenamento em memória
    /// </summary>
    public class Packet
    {
        /// <summary>
        /// ID único do pacote
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Timestamp de recebimento
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Chave de roteamento da fila
        /// </summary>
        public string RoutingKey { get; set; } = string.Empty;

        /// <summary>
        /// Tamanho do payload em bytes
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Payload binário do pacote
        /// </summary>
        public byte[] Payload { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Preview hexadecimal do payload (primeiros 256 bytes)
        /// </summary>
        public string HexPreview { get; set; } = string.Empty;

        /// <summary>
        /// IP de origem (se disponível)
        /// </summary>
        public string? SourceIp { get; set; }

        /// <summary>
        /// Porta de origem (se disponível)
        /// </summary>
        public int? SourcePort { get; set; }

        /// <summary>
        /// IP de destino (se disponível)
        /// </summary>
        public string? DestinationIp { get; set; }

        /// <summary>
        /// Porta de destino (se disponível)
        /// </summary>
        public int? DestinationPort { get; set; }

        /// <summary>
        /// Indica se o pacote foi processado com sucesso
        /// </summary>
        public bool IsProcessed { get; set; }

        /// <summary>
        /// Erro de processamento (se houver)
        /// </summary>
        public string? ProcessingError { get; set; }

        /// <summary>
        /// Metadados adicionais do pacote
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();

        /// <summary>
        /// Cria um novo pacote a partir de dados binários
        /// </summary>
        public static Packet Create(byte[] payload, string routingKey = "", string? sourceIp = null, int? sourcePort = null)
        {
            var hexPreview = payload.Length > 0 ? Convert.ToHexString(payload.Take(256).ToArray()) : "";
            
            return new Packet
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                RoutingKey = routingKey,
                Size = payload.Length,
                Payload = payload,
                HexPreview = hexPreview,
                SourceIp = sourceIp,
                SourcePort = sourcePort,
                IsProcessed = false
            };
        }

        /// <summary>
        /// Obtém uma representação resumida do pacote
        /// </summary>
        public string GetSummary()
        {
            return $"[{Timestamp:HH:mm:ss.fff}] {Size} bytes via {RoutingKey}";
        }
    }
}