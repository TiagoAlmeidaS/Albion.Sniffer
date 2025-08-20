using System;

namespace AlbionOnlineSniffer.Capture.Interfaces
{
    /// <summary>
    /// Interface para captura de dados via TCP (conexões remotas)
    /// </summary>
    public interface ITcpCaptureService
    {
        /// <summary>
        /// Evento disparado quando dados TCP são capturados
        /// </summary>
        event Action<byte[]>? OnTcpDataCaptured;

        /// <summary>
        /// Inicia a captura TCP
        /// </summary>
        void Start();

        /// <summary>
        /// Para a captura TCP
        /// </summary>
        void Stop();

        /// <summary>
        /// Verifica se está capturando
        /// </summary>
        bool IsCapturing { get; }

        /// <summary>
        /// Endpoint TCP configurado
        /// </summary>
        string Endpoint { get; }
    }
}
