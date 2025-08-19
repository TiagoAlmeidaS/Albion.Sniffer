namespace AlbionOnlineSniffer.Core.Observability.Metrics
{
    /// <summary>
    /// Coletor de métricas para o sistema de observabilidade
    /// </summary>
    public interface IMetricsCollector
    {
        /// <summary>
        /// Registra uma métrica de contador
        /// </summary>
        void IncrementCounter(string name, params KeyValuePair<string, object?>[] tags);

        /// <summary>
        /// Registra uma métrica de valor
        /// </summary>
        void RecordValue(string name, double value, params KeyValuePair<string, object?>[] tags);

        /// <summary>
        /// Registra uma métrica de histograma
        /// </summary>
        void RecordHistogram(string name, double value, params KeyValuePair<string, object?>[] tags);

        /// <summary>
        /// Registra uma métrica de gauge
        /// </summary>
        void SetGauge(string name, double value, params KeyValuePair<string, object?>[] tags);

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

        /// <summary>
        /// Obtém estatísticas das métricas
        /// </summary>
        MetricsStatistics GetStatistics();
    }

    /// <summary>
    /// Estatísticas das métricas coletadas
    /// </summary>
    public class MetricsStatistics
    {
        public int TotalCounters { get; set; }
        public int TotalValues { get; set; }
        public int TotalHistograms { get; set; }
        public int TotalGauges { get; set; }
        public DateTime LastUpdate { get; set; }
        public Dictionary<string, long> CounterTotals { get; set; } = new();
        public Dictionary<string, double> ValueAverages { get; set; } = new();
        public Dictionary<string, double> HistogramPercentiles { get; set; } = new();
    }
}
