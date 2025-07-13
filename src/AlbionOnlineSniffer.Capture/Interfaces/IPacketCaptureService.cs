using System;

namespace AlbionOnlineSniffer.Capture.Interfaces
{
    public interface IPacketCaptureService : IDisposable
    {
        event Action<byte[]>? OnUdpPayloadCaptured;
        void Start();
        void Stop();
    }
} 