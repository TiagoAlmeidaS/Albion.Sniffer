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
        /// Registra todos os serviços de captura
        /// </summary>
        /// <param name="services">ServiceCollection para registrar os serviços</param>
        /// <param name="configuration">Configuração da aplicação</param>
        public static void AddCaptureServices(this IServiceCollection services, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            // Configurações de captura
            var captureMode = configuration.GetValue<string>("Capture:Mode", "UDP");
            var udpPort = configuration.GetValue<int>("Capture:UdpPort", 5050);
            var tcpEndpoint = configuration.GetValue<string>("Capture:TcpEndpoint", "0.tcp.sa.ngrok.io:16179");
            var enableFeatureFlag = configuration.GetValue<bool>("Capture:EnableFeatureFlag", true);
            var fallbackToUDP = configuration.GetValue<bool>("Capture:FallbackToUDP", true);

            // Log das configurações lidas
            var logger = services.BuildServiceProvider().GetService<Microsoft.Extensions.Logging.ILogger<HybridCaptureService>>();
            logger?.LogInformation("🔧 Configuração de captura: Mode={Mode}, UdpPort={UdpPort}, TcpEndpoint={TcpEndpoint}, EnableFeatureFlag={EnableFeatureFlag}", 
                captureMode, udpPort, tcpEndpoint, enableFeatureFlag);

            // Serviços base de captura
            services.AddSingleton<PacketCaptureMonitor>();
            
            // Serviço UDP (sempre registrado para fallback)
            services.AddSingleton<IPacketCaptureService>(sp =>
            {
                var eventLogger = sp.GetService<IAlbionEventLogger>() ?? new AlbionEventLogger();
                return new PacketCaptureService(udpPort, eventLogger);
            });

            // Serviço TCP (registrado se feature flag habilitada)
            if (enableFeatureFlag)
            {
                logger?.LogInformation("✅ Feature flag habilitada, registrando serviços TCP e híbridos");
                
                services.AddSingleton<ITcpCaptureService>(sp =>
                {
                    var eventLogger = sp.GetService<IAlbionEventLogger>() ?? new AlbionEventLogger();
                    var logger = sp.GetService<ILogger<TcpCaptureService>>();
                    return new TcpCaptureService(tcpEndpoint, eventLogger, logger);
                });

                // Serviço híbrido (orquestra UDP + TCP baseado no modo)
                services.AddSingleton<HybridCaptureService>(sp =>
                {
                    var udpService = sp.GetRequiredService<IPacketCaptureService>();
                    var tcpService = sp.GetRequiredService<ITcpCaptureService>();
                    var logger = sp.GetService<ILogger<HybridCaptureService>>();
                    
                    logger?.LogInformation("🎯 Criando HybridCaptureService com modo: {Mode}", captureMode);
                    return new HybridCaptureService(udpService, tcpService, captureMode, logger);
                });
                
                logger?.LogInformation("✅ Serviços TCP e híbridos registrados com sucesso");
            }
            else
            {
                logger?.LogWarning("⚠️ Feature flag desabilitada, apenas serviços UDP serão registrados");
            }

            // Configuração de captura baseada no modo
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
        public static void AddCaptureServices(this IServiceCollection services, string captureMode = "UDP", int udpPort = 5050, string tcpEndpoint = "0.tcp.sa.ngrok.io:16179")
        {
            // Serviços base
            services.AddSingleton<PacketCaptureMonitor>();
            
            // Serviço UDP
            services.AddSingleton<IPacketCaptureService>(sp =>
            {
                var eventLogger = sp.GetService<IAlbionEventLogger>() ?? new AlbionEventLogger();
                return new PacketCaptureService(udpPort, eventLogger);
            });

            // Serviço TCP (se não for modo UDP apenas)
            if (captureMode != "UDP")
            {
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
                    
                    return new HybridCaptureService(udpService, tcpService, captureMode, logger);
                });
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
        public static void AddUdpCaptureServices(this IServiceCollection services, int udpPort = 5050)
        {
            services.AddSingleton<PacketCaptureMonitor>();
            services.AddSingleton<IPacketCaptureService>(sp =>
            {
                var eventLogger = sp.GetService<IAlbionEventLogger>() ?? new AlbionEventLogger();
                return new PacketCaptureService(udpPort, eventLogger);
            });

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
        public static void AddTcpCaptureServices(this IServiceCollection services, string tcpEndpoint)
        {
            services.AddSingleton<PacketCaptureMonitor>();
            services.AddSingleton<ITcpCaptureService>(sp =>
            {
                var eventLogger = sp.GetService<IAlbionEventLogger>() ?? new AlbionEventLogger();
                var logger = sp.GetService<ILogger<TcpCaptureService>>();
                return new TcpCaptureService(tcpEndpoint, eventLogger, logger);
            });

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
        public static void AddHybridCaptureServices(this IServiceCollection services, int udpPort = 5050, string tcpEndpoint = "0.tcp.sa.ngrok.io:16179")
        {
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
    }

    /// <summary>
    /// Configuração de captura
    /// </summary>
    public class CaptureConfiguration
    {
        public string Mode { get; set; } = "UDP";
        public int UdpPort { get; set; } = 5050;
        public string TcpEndpoint { get; set; } = "0.tcp.sa.ngrok.io:16179";
        public bool EnableFeatureFlag { get; set; } = true;
        public bool FallbackToUDP { get; set; } = true;
    }
} 