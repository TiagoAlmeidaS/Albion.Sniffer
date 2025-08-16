using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core;
using AlbionOnlineSniffer.Capture;
using AlbionOnlineSniffer.Queue;
using AlbionOnlineSniffer.App;

namespace AlbionOnlineSniffer.App
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using var loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Warning);
                });
                var logger = loggerFactory.CreateLogger<Program>();

                try
                {
                    // 🔧 CONFIGURAR DEPENDÊNCIAS
                    var host = CreateHostBuilder(args).Build();
                    var serviceProvider = host.Services;

                    // 📡 INICIALIZAR SERVIÇOS DE CAPTURA
                    var captureService = serviceProvider.GetRequiredService<IPacketCaptureService>();
                    var networkHandler = serviceProvider.GetRequiredService<AlbionNetworkHandlerManager>();

                    // 🔗 CONFIGURAR EVENTOS DE CAPTURA
                    captureService.OnUdpPayloadCaptured += async (payload) =>
                    {
                        try
                        {
                            // Processar pacote através do network handler
                            await networkHandler.ProcessPacketAsync(payload);
                        }
                        catch (Exception ex)
                        {
                            // Log silencioso - será exibido na interface web
                        }
                    };

                    // 🚀 INICIAR CAPTURA
                    captureService.StartCapture();

                    // ⏳ AGUARDAR COMANDO DE PARADA
                    var waitHandle = new System.Threading.ManualResetEvent(false);
                    Console.CancelKeyPress += (sender, e) =>
                    {
                        e.Cancel = true;
                        waitHandle.Set();
                    };
                    
                    waitHandle.WaitOne();
                }
                catch (Exception ex)
                {
                    // Log silencioso - será exibido na interface web
                }
            }
            catch (Exception ex)
            {
                // Log silencioso - será exibido na interface web
            }
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // 🔧 REGISTRAR SERVIÇOS CORE
                    services.AddCoreServices(hostContext.Configuration);
                    
                    // 📡 REGISTRAR SERVIÇOS DE CAPTURA
                    services.AddCaptureServices(hostContext.Configuration);
                    
                    // 🚀 REGISTRAR SERVIÇOS DE FILA
                    services.AddQueueServices(hostContext.Configuration);
                });
    }
}
