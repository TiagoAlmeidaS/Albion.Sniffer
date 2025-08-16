using System.Collections.Generic;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Interfaces
{
    /// <summary>
    /// Interface para o sistema de logs personalizado do Albion
    /// </summary>
    public interface IAlbionEventLogger
    {
        #region Logs Gerais

        /// <summary>
        /// Adiciona um log geral
        /// </summary>
        void AddLog(LogLevel level, string message, string? category = null, object? data = null);

        /// <summary>
        /// Obtém logs gerais
        /// </summary>
        IEnumerable<LogEntry> GetLogs(int count = 100, LogLevel? minLevel = null);

        #endregion

        #region Logs de Captura de Rede

        /// <summary>
        /// Adiciona log de captura de pacote UDP
        /// </summary>
        void LogUdpPacketCapture(byte[] payload, string sourceIp, int sourcePort, string destIp, int destPort);

        /// <summary>
        /// Adiciona log de erro na captura
        /// </summary>
        void LogCaptureError(string error, string context, Exception? exception = null);

        /// <summary>
        /// Adiciona log de dispositivo de rede
        /// </summary>
        void LogNetworkDevice(string deviceName, string deviceType, bool isValid);

        /// <summary>
        /// Obtém logs de captura de rede
        /// </summary>
        IEnumerable<NetworkCaptureLog> GetNetworkLogs(int count = 100, NetworkCaptureType? type = null);

        #endregion

        #region Logs de Eventos

        /// <summary>
        /// Adiciona log de evento processado
        /// </summary>
        void LogEventProcessed(string eventType, object eventData, bool success, string? error = null);

        /// <summary>
        /// Adiciona log de evento enviado para fila
        /// </summary>
        void LogEventQueued(string eventType, string queueName, bool success, string? error = null);

        /// <summary>
        /// Obtém logs de eventos
        /// </summary>
        IEnumerable<EventLog> GetEventLogs(int count = 100, string? eventType = null);

        #endregion

        #region Métricas e Estatísticas

        /// <summary>
        /// Obtém estatísticas dos logs
        /// </summary>
        LogStatistics GetStatistics();

        /// <summary>
        /// Limpa logs antigos (mais de 24 horas)
        /// </summary>
        void CleanupOldLogs();

        #endregion
    }
}
