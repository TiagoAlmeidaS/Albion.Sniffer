namespace AlbionOnlineSniffer.Web.Models
{
    /// <summary>
    /// Modelo de sessão para agrupar eventos relacionados
    /// </summary>
    public class Session
    {
        /// <summary>
        /// ID único da sessão
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Tipo da sessão (ex: cluster, dungeon, fishing, etc.)
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Nome da sessão
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp de início da sessão
        /// </summary>
        public DateTime StartedAt { get; set; }

        /// <summary>
        /// Timestamp de fim da sessão (se encerrada)
        /// </summary>
        public DateTime? EndedAt { get; set; }

        /// <summary>
        /// Status da sessão
        /// </summary>
        public SessionStatus Status { get; set; }

        /// <summary>
        /// ID do cluster associado
        /// </summary>
        public string? ClusterId { get; set; }

        /// <summary>
        /// Nome do cluster
        /// </summary>
        public string? ClusterName { get; set; }

        /// <summary>
        /// Quantidade de eventos na sessão
        /// </summary>
        public int EventCount { get; set; }

        /// <summary>
        /// Quantidade de pacotes na sessão
        /// </summary>
        public int PacketCount { get; set; }

        /// <summary>
        /// Quantidade de logs na sessão
        /// </summary>
        public int LogCount { get; set; }

        /// <summary>
        /// Última atividade na sessão
        /// </summary>
        public DateTime LastActivity { get; set; }

        /// <summary>
        /// Metadados adicionais da sessão
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();

        /// <summary>
        /// Cria uma nova sessão
        /// </summary>
        public static Session Create(string id, string type, string name, string? clusterId = null, string? clusterName = null)
        {
            var now = DateTime.UtcNow;
            return new Session
            {
                Id = id,
                Type = type,
                Name = name,
                StartedAt = now,
                LastActivity = now,
                Status = SessionStatus.Active,
                ClusterId = clusterId,
                ClusterName = clusterName,
                EventCount = 0,
                PacketCount = 0,
                LogCount = 0
            };
        }

        /// <summary>
        /// Atualiza a atividade da sessão
        /// </summary>
        public void UpdateActivity()
        {
            LastActivity = DateTime.UtcNow;
        }

        /// <summary>
        /// Incrementa o contador de eventos
        /// </summary>
        public void IncrementEventCount()
        {
            EventCount++;
            UpdateActivity();
        }

        /// <summary>
        /// Incrementa o contador de pacotes
        /// </summary>
        public void IncrementPacketCount()
        {
            PacketCount++;
            UpdateActivity();
        }

        /// <summary>
        /// Incrementa o contador de logs
        /// </summary>
        public void IncrementLogCount()
        {
            LogCount++;
            UpdateActivity();
        }

        /// <summary>
        /// Encerra a sessão
        /// </summary>
        public void End()
        {
            EndedAt = DateTime.UtcNow;
            Status = SessionStatus.Ended;
            UpdateActivity();
        }

        /// <summary>
        /// Obtém a duração da sessão
        /// </summary>
        public TimeSpan GetDuration()
        {
            var endTime = EndedAt ?? DateTime.UtcNow;
            return endTime - StartedAt;
        }

        /// <summary>
        /// Obtém uma representação resumida da sessão
        /// </summary>
        public string GetSummary()
        {
            var duration = GetDuration();
            var status = Status == SessionStatus.Active ? "🟢" : "🔴";
            return $"{status} {Type}: {Name} ({duration:mm\\:ss}) - {EventCount} eventos, {PacketCount} pacotes";
        }

        /// <summary>
        /// Verifica se a sessão está ativa
        /// </summary>
        public bool IsActive => Status == SessionStatus.Active;

        /// <summary>
        /// Verifica se a sessão está inativa há muito tempo
        /// </summary>
        public bool IsStale(TimeSpan threshold)
        {
            return DateTime.UtcNow - LastActivity > threshold;
        }
    }

    /// <summary>
    /// Status da sessão
    /// </summary>
    public enum SessionStatus
    {
        Active,
        Ended,
        Suspended
    }
}