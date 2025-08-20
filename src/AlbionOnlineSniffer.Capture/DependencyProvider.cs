using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using AlbionOnlineSniffer.Capture.Interfaces;
using AlbionOnlineSniffer.Capture.Services;
using AlbionOnlineSniffer.Core.Interfaces;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Capture
{
    /// <summary>
    /// Configuração de Dependency Injection para o módulo Capture
    /// </summary>
    public static class DependencyProvider
    {
        /// <summary>
        /// Registra todos os serviços de captura usando configuração padrão
        /// </summary>
        /// <param name="services">ServiceCollection para registrar os serviços</param>
        public static void RegisterServices(IServiceCollection services)
        {
            // Configuração padrão: modo UDP na porta 5050
            RegisterServices(services, "UDP", 5050);
        }

        /// <summary>
        /// Registra todos os serviços de captura com configuração customizada
        /// </summary>
        /// <param name="services">ServiceCollection para registrar os serviços</param>
        /// <param name="configuration">Configuração da aplicação</param>
        public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            var captureMode = configuration.GetValue<string>("Capture:Mode", "UDP");
            var udpPort = configuration.GetValue<int>("Capture:UdpPort", 5050);
            var tcpEndpoint = configuration.GetValue<string>("Capture:TcpEndpoint", "0.tcp.sa.ngrok.io:16179");
            var enableFeatureFlag = configuration.GetValue<bool>("Capture:EnableFeatureFlag", false);
            var fallbackToUDP = configuration.GetValue<bool>("Capture:FallbackToUDP", true);

            // Log das configurações lidas
            var logger = services.BuildServiceProvider().GetService<ILogger<HybridCaptureService>>();
            logger?.LogInformation("🔧 Configuração de captura: Mode={Mode}, UdpPort={UdpPort}, TcpEndpoint={TcpEndpoint}, EnableFeatureFlag={EnableFeatureFlag}", 
                captureMode, udpPort, tcpEndpoint, enableFeatureFlag);

            // Baseado no modo, registra os serviços apropriados
            switch (captureMode.ToUpperInvariant())
            {
                case "UDP":
                    RegisterUdpServices(services, udpPort);
                    break;
                case "TCP":
                    if (enableFeatureFlag)
                        RegisterTcpServices(services, tcpEndpoint);
                    else
                        RegisterUdpServices(services, udpPort); // Fallback para UDP
                    break;
                case "HYBRID":
                    if (enableFeatureFlag)
                        RegisterHybridServices(services, udpPort, tcpEndpoint);
                    else
                        RegisterUdpServices(services, udpPort); // Fallback para UDP
                    break;
                default:
                    logger?.LogWarning("⚠️ Modo de captura '{Mode}' não reconhecido, usando UDP como fallback", captureMode);
                    RegisterUdpServices(services, udpPort);
                    break;
            }

            // Configuração de captura
            services.Configure<CaptureConfiguration>(options =>
            {
                options.Mode = captureMode;
                options.UdpPort = udpPort;
                options.TcpEndpoint = tcpEndpoint;
                options.EnableFeatureFlag = enableFeatureFlag;
                options.FallbackToUDP = fallbackToUDP;
            });
        }

        /// <summary>
        /// Registra serviços de captura com configuração customizada
        /// </summary>
        /// <param name="services">ServiceCollection</param>
        /// <param name="captureMode">Modo de captura (UDP, TCP, HYBRID)</param>
        /// <param name="udpPort">Porta UDP</param>
        /// <param name="tcpEndpoint">Endpoint TCP</param>
        public static void RegisterServices(IServiceCollection services, string captureMode = "UDP", int udpPort = 5050, string tcpEndpoint = "0.tcp.sa.ngrok.io:16179")
        {
            switch (captureMode.ToUpperInvariant())
            {
                case "UDP":
                    RegisterUdpServices(services, udpPort);
                    break;
                case "TCP":
                    RegisterTcpServices(services, tcpEndpoint);
                    break;
                case "HYBRID":
                    RegisterHybridServices(services, udpPort, tcpEndpoint);
                    break;
                default:
                    RegisterUdpServices(services, udpPort); // Fallback para UDP
                    break;
            }

            // Configuração
            services.Configure<CaptureConfiguration>(options =>
            {
                options.Mode = captureMode;
                options.UdpPort = udpPort;
                options.TcpEndpoint = tcpEndpoint;
                options.EnableFeatureFlag = captureMode != "UDP";
                options.FallbackToUDP = true;
            });
        }

        /// <summary>
        /// Registra apenas serviços UDP (modo tradicional)
        /// </summary>
        /// <param name="services">ServiceCollection</param>
        /// <param name="udpPort">Porta UDP</param>
        public static void RegisterUdpServices(IServiceCollection services, int udpPort = 5050)
        {
            // Serviços base de captura
            services.AddSingleton<PacketCaptureMonitor>();
            
            // Serviço UDP principal
            services.AddSingleton<IPacketCaptureService>(sp =>
            {
                var eventLogger = sp.GetService<IAlbionEventLogger>() ?? new AlbionEventLogger();
                return new PacketCaptureService(udpPort, eventLogger);
            });

            // Configuração
            services.Configure<CaptureConfiguration>(options =>
            {
                options.Mode = "UDP";
                options.UdpPort = udpPort;
                options.EnableFeatureFlag = false;
                options.FallbackToUDP = true;
            });
        }

        /// <summary>
        /// Registra apenas serviços TCP
        /// </summary>
        /// <param name="services">ServiceCollection</param>
        /// <param name="tcpEndpoint">Endpoint TCP</param>
        public static void RegisterTcpServices(IServiceCollection services, string tcpEndpoint)
        {
            // Serviços base de captura
            services.AddSingleton<PacketCaptureMonitor>();
            
            // Serviço TCP
            services.AddSingleton<ITcpCaptureService>(sp =>
            {
                var eventLogger = sp.GetService<IAlbionEventLogger>() ?? new AlbionEventLogger();
                var logger = sp.GetService<ILogger<TcpCaptureService>>();
                return new TcpCaptureService(tcpEndpoint, eventLogger, logger);
            });

            // Configuração
            services.Configure<CaptureConfiguration>(options =>
            {
                options.Mode = "TCP";
                options.TcpEndpoint = tcpEndpoint;
                options.EnableFeatureFlag = true;
                options.FallbackToUDP = false;
            });
        }

        /// <summary>
        /// Registra serviços híbridos (UDP + TCP)
        /// </summary>
        /// <param name="services">ServiceCollection</param>
        /// <param name="udpPort">Porta UDP</param>
        /// <param name="tcpEndpoint">Endpoint TCP</param>
        public static void RegisterHybridServices(IServiceCollection services, int udpPort = 5050, string tcpEndpoint = "0.tcp.sa.ngrok.io:16179")
        {
            // Serviços base de captura
            services.AddSingleton<PacketCaptureMonitor>();
            
            // Serviço UDP
            services.AddSingleton<IPacketCaptureService>(sp =>
            {
                var eventLogger = sp.GetService<IAlbionEventLogger>() ?? new AlbionEventLogger();
                return new PacketCaptureService(udpPort, eventLogger);
            });

            // Serviço TCP
            services.AddSingleton<ITcpCaptureService>(sp =>
            {
                var eventLogger = sp.GetService<IAlbionEventLogger>() ?? new AlbionEventLogger();
                var logger = sp.GetService<ILogger<TcpCaptureService>>();
                return new TcpCaptureService(tcpEndpoint, eventLogger, logger);
            });

            // Serviço híbrido
            services.AddSingleton<HybridCaptureService>(sp =>
            {
                var udpService = sp.GetRequiredService<IPacketCaptureService>();
                var tcpService = sp.GetRequiredService<ITcpCaptureService>();
                var logger = sp.GetService<ILogger<HybridCaptureService>>();
                
                return new HybridCaptureService(udpService, tcpService, "HYBRID", logger);
            });

            services.Configure<CaptureConfiguration>(options =>
            {
                options.Mode = "HYBRID";
                options.UdpPort = udpPort;
                options.TcpEndpoint = tcpEndpoint;
                options.EnableFeatureFlag = true;
                options.FallbackToUDP = true;
            });
        }

        // Métodos de compatibilidade (mantidos para não quebrar código existente)
        
        /// <summary>
        /// Método de compatibilidade - registra serviços de captura
        /// </summary>
        [Obsolete("Use RegisterServices(IServiceCollection, IConfiguration) instead")]
        public static void AddCaptureServices(this IServiceCollection services, IConfiguration configuration)
        {
            RegisterServices(services, configuration);
        }

        /// <summary>
        /// Método de compatibilidade - registra serviços de captura
        /// </summary>
        [Obsolete("Use RegisterServices(IServiceCollection, string, int, string) instead")]
        public static void AddCaptureServices(this IServiceCollection services, string captureMode = "UDP", int udpPort = 5050, string tcpEndpoint = "0.tcp.sa.ngrok.io:16179")
        {
            RegisterServices(services, captureMode, udpPort, tcpEndpoint);
        }

        /// <summary>
        /// Método de compatibilidade - registra serviços UDP
        /// </summary>
        [Obsolete("Use RegisterUdpServices instead")]
        public static void AddUdpCaptureServices(this IServiceCollection services, int udpPort = 5050)
        {
            RegisterUdpServices(services, udpPort);
        }

        /// <summary>
        /// Método de compatibilidade - registra serviços TCP
        /// </summary>
        [Obsolete("Use RegisterTcpServices instead")]
        public static void AddTcpCaptureServices(this IServiceCollection services, string tcpEndpoint)
        {
            RegisterTcpServices(services, tcpEndpoint);
        }

        /// <summary>
        /// Método de compatibilidade - registra serviços híbridos
        /// </summary>
        [Obsolete("Use RegisterHybridServices instead")]
        public static void AddHybridCaptureServices(this IServiceCollection services, int udpPort = 5050, string tcpEndpoint = "0.tcp.sa.ngrok.io:16179")
        {
            RegisterHybridServices(services, udpPort, tcpEndpoint);
        }
    }

    /// <summary>
    /// Configuração de captura
    /// </summary>
    public class CaptureConfiguration
    {
        public string Mode { get; set; } = "UDP";
        public int UdpPort { get; set; } = 5050;
        public string TcpEndpoint { get; set; } = "0.tcp.sa.ngrok.io:16179";
        public bool EnableFeatureFlag { get; set; } = false;
        public bool FallbackToUDP { get; set; } = true;
    }
} 