using Microsoft.Extensions.DependencyInjection;
using AlbionOnlineSniffer.Capture.Interfaces;
using AlbionOnlineSniffer.Core.Interfaces;

namespace AlbionOnlineSniffer.Capture
{
    public static class DependencyProvider
    {
        /// <summary>
        /// Registra os serviços de captura
        /// </summary>
        public static void AddCaptureServices(this IServiceCollection services)
        {
            services.AddSingleton<IPacketCaptureService>(provider =>
            {
                var eventLogger = provider.GetService<IAlbionEventLogger>();
                return new PacketCaptureService(5050, eventLogger);
            });
        }

        /// <summary>
        /// Factory para criar PacketCaptureService (método legado)
        /// </summary>
        public static PacketCaptureService CreatePacketCaptureService(int udpPort = 5050)
        {
            var eventLogger = new AlbionOnlineSniffer.Core.Services.AlbionEventLogger();
            return new PacketCaptureService(udpPort, eventLogger);
        }
    }
} 