using AlbionOnlineSniffer.Capture;

namespace AlbionOnlineSniffer.Capture
{
    public static class DependencyProvider
    {
        public static PacketCaptureService CreatePacketCaptureService(int udpPort = 5050)
            => new PacketCaptureService(udpPort);
        // Adicione outros factories/configurações conforme necessário
    }
} 