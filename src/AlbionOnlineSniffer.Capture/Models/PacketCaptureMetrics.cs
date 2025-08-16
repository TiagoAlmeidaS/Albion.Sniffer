using System;

namespace AlbionOnlineSniffer.Capture.Models
{
    /// <summary>
    /// Métricas de captura de pacotes para monitoramento
    /// </summary>
    public class PacketCaptureMetrics
    {
        /// <summary>
        /// Total de pacotes capturados desde o início
        /// </summary>
        public long TotalPacketsCaptured { get; set; }
        
        /// <summary>
        /// Total de pacotes válidos (UDP na porta correta)
        /// </summary>
        public long ValidPacketsCaptured { get; set; }
        
        /// <summary>
        /// Total de pacotes descartados (inválidos ou com erro)
        /// </summary>
        public long PacketsDropped { get; set; }
        
        /// <summary>
        /// Total de bytes capturados
        /// </summary>
        public long TotalBytesCapturated { get; set; }
        
        /// <summary>
        /// Taxa de pacotes por segundo (calculada)
        /// </summary>
        public double PacketsPerSecond { get; set; }
        
        /// <summary>
        /// Taxa de bytes por segundo (calculada)
        /// </summary>
        public double BytesPerSecond { get; set; }
        
        /// <summary>
        /// Timestamp da última captura
        /// </summary>
        public DateTime LastCaptureTime { get; set; }
        
        /// <summary>
        /// Timestamp do início da captura
        /// </summary>
        public DateTime StartTime { get; set; }
        
        /// <summary>
        /// Tempo total de captura ativa
        /// </summary>
        public TimeSpan TotalCaptureTime => DateTime.UtcNow - StartTime;
        
        /// <summary>
        /// Status atual da captura
        /// </summary>
        public string Status { get; set; } = "Stopped";
        
        /// <summary>
        /// Última interface de rede utilizada
        /// </summary>
        public string? LastInterface { get; set; }
        
        /// <summary>
        /// Último filtro aplicado
        /// </summary>
        public string? LastFilter { get; set; }
        
        /// <summary>
        /// Número de erros de captura
        /// </summary>
        public long CaptureErrors { get; set; }
        
        /// <summary>
        /// Última mensagem de erro
        /// </summary>
        public string? LastError { get; set; }
        
        /// <summary>
        /// Timestamp do último erro
        /// </summary>
        public DateTime? LastErrorTime { get; set; }

        /// <summary>
        /// Número de dispositivos de rede encontrados
        /// </summary>
        public int NetworkDevicesFound { get; set; }

        /// <summary>
        /// Calcula as taxas por segundo baseado no tempo decorrido
        /// </summary>
        public void CalculateRates()
        {
            var elapsed = TotalCaptureTime.TotalSeconds;
            if (elapsed > 0)
            {
                PacketsPerSecond = ValidPacketsCaptured / elapsed;
                BytesPerSecond = TotalBytesCapturated / elapsed;
            }
        }
        
        /// <summary>
        /// Reseta as métricas para um novo ciclo de captura
        /// </summary>
        public void Reset()
        {
            TotalPacketsCaptured = 0;
            ValidPacketsCaptured = 0;
            PacketsDropped = 0;
            TotalBytesCapturated = 0;
            PacketsPerSecond = 0;
            BytesPerSecond = 0;
            CaptureErrors = 0;
            LastError = null;
            LastErrorTime = null;
            NetworkDevicesFound = 0;
            StartTime = DateTime.UtcNow;
            Status = "Starting";
        }
        
        /// <summary>
        /// Cria uma cópia das métricas atuais
        /// </summary>
        public PacketCaptureMetrics Clone()
        {
            return new PacketCaptureMetrics
            {
                TotalPacketsCaptured = this.TotalPacketsCaptured,
                ValidPacketsCaptured = this.ValidPacketsCaptured,
                PacketsDropped = this.PacketsDropped,
                TotalBytesCapturated = this.TotalBytesCapturated,
                PacketsPerSecond = this.PacketsPerSecond,
                BytesPerSecond = this.BytesPerSecond,
                LastCaptureTime = this.LastCaptureTime,
                StartTime = this.StartTime,
                Status = this.Status,
                LastInterface = this.LastInterface,
                LastFilter = this.LastFilter,
                CaptureErrors = this.CaptureErrors,
                LastError = this.LastError,
                LastErrorTime = this.LastErrorTime,
                NetworkDevicesFound = this.NetworkDevicesFound
            };
        }

        /// <summary>
        /// Converte as métricas para uma representação string legível
        /// </summary>
        public override string ToString()
        {
            return $"Status: {Status} | " +
                   $"Packets: {ValidPacketsCaptured}/{TotalPacketsCaptured} | " +
                   $"Rate: {PacketsPerSecond:F2} pkt/s, {BytesPerSecond:F2} B/s | " +
                   $"Dropped: {PacketsDropped} | " +
                   $"Errors: {CaptureErrors} | " +
                   $"Uptime: {TotalCaptureTime:hh\\:mm\\:ss}";
        }
    }
}