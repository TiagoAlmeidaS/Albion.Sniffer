using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using AlbionOnlineSniffer.Options.Extensions;
using AlbionOnlineSniffer.Capture;
using AlbionOnlineSniffer.Core;
using AlbionOnlineSniffer.Queue;

namespace AlbionOnlineSniffer.App
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("🚀 Iniciando AlbionOnlineSniffer...");
                Console.WriteLine($"📁 Diretório atual: {Directory.GetCurrentDirectory()}");
                Console.WriteLine($"🔧 Versão do .NET: {Environment.Version}");
                Console.WriteLine($"💻 Arquitetura: {(Environment.Is64BitProcess ? "x64" : "x86")}");

                // Verificar assemblies carregados
                Console.WriteLine("📦 Assemblies carregados:");
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Console.WriteLine($"   - {assembly.GetName().Name} ({assembly.GetName().Version})");
                }

                using var loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Information);
                });
                var logger = loggerFactory.CreateLogger<Program>();

                try
                {
                    logger.LogInformation("Iniciando AlbionOnlineSniffer...");

                    // 🎨 VALIDAR LOGO DO APLICATIVO
                    logger.LogInformation("🎨 Verificando logo do aplicativo...");

                    // Configuração
                    logger.LogInformation("📋 Carregando configurações...");
                    var configuration = new ConfigurationBuilder()
                        .SetBasePath(AppContext.BaseDirectory)
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables()
                        .Build();


                    // Publishers
                    logger.LogInformation("📤 Configurando publishers...");
                    // Publisher via DI (configurado no módulo de Queue)

                    // Configure dependency injection
                    logger.LogInformation("🔧 Configurando injeção de dependências...");
                    var services = new ServiceCollection();

                    // Add logging
                    services.AddLogging(builder => builder.AddConsole());

                    // Options + Profiles
                    services.AddSnifferOptions(configuration);
                    services.ValidateOptionsOnStart<AlbionOnlineSniffer.Options.SnifferOptions>();
                    services.AddProfileManagement();

                    // Core services (registra EventDispatcher, PacketOffsets, etc.)
                    logger.LogInformation("🔧 Registrando serviços do Core...");
                    Core.DependencyProvider.RegisterServices(services);

                    // Queue services + Event->Queue Bridge
                    logger.LogInformation("🔧 Registrando serviços do Queue...");
                    Queue.DependencyProvider.AddQueueServices(services, configuration);

                    // Serviços de captura usando DependencyProvider do módulo Capture
                    services.AddCaptureServices(configuration);
                    
                    // Pipeline App usando serviço híbrido quando disponível
                    services.AddSingleton<App.Services.CapturePipeline>();

                    // Build service provider
                    logger.LogInformation("🔧 Construindo service provider...");
                    var serviceProvider = services.BuildServiceProvider();

                    // Force validation and log active profile
                    var snifferOptions = serviceProvider
                        .GetRequiredService<IOptions<AlbionOnlineSniffer.Options.SnifferOptions>>().Value;
                    logger.LogInformation("Profile ativo: {Profile}", snifferOptions.GetActiveProfile().Name);

                    // Get services from DI container
                    logger.LogInformation("🔧 Obtendo serviços do container...");
                    var eventDispatcher = serviceProvider.GetRequiredService<Core.Services.EventDispatcher>();
                    var packetOffsets = serviceProvider.GetRequiredService<Core.Models.ResponseObj.PacketOffsets>();
                    var packetIndexes = serviceProvider.GetRequiredService<Core.Models.ResponseObj.PacketIndexes>();

                    // Get pipeline service
                    var pipeline = serviceProvider.GetRequiredService<Core.Pipeline.IEventPipeline>();
                    logger.LogInformation("🚀 Pipeline obtido: {PipelineType}", pipeline.GetType().Name);

                    // Start the pipeline
                    await pipeline.StartAsync();
                    logger.LogInformation("✅ Pipeline iniciado com sucesso!");

                    // Register pipeline handler with EventDispatcher (global handler)
                    eventDispatcher.RegisterGlobalHandler(async (eventData) =>
                    {
                        try
                        {
                            var eventTypeName = eventData.GetType().Name;
                            await pipeline.EnqueueAsync(eventTypeName, eventData);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Erro ao enfileirar evento no pipeline");
                        }
                    });
                    logger.LogInformation("🔗 Pipeline conectado ao EventDispatcher");

                    // 🔧 VERIFICAR SE OS OFFSETS FORAM CARREGADOS CORRETAMENTE (via Core)
                    logger.LogInformation("🔍 VERIFICANDO OFFSETS CARREGADOS (via Core):");
                    logger.LogInformation("  - Leave: [{Offsets}]", string.Join(", ", packetOffsets.Leave));
                    logger.LogInformation("  - HealthUpdateEvent: [{Offsets}]",
                        string.Join(", ", packetOffsets.HealthUpdateEvent));
                    logger.LogInformation("  - NewCharacter: [{Offsets}]",
                        string.Join(", ", packetOffsets.NewCharacter));
                    logger.LogInformation("  - Move: [{Offsets}]", string.Join(", ", packetOffsets.Move));

                    // 🔧 INTEGRAÇÃO COM MENSAGERIA - Bridge via DI
                    logger.LogInformation("🔧 Conectando EventDispatcher ao Publisher via Bridge...");
                    serviceProvider.GetRequiredService<AlbionOnlineSniffer.Queue.Publishers.EventToQueueBridge>();
                    logger.LogInformation("✅ Bridge Event->Queue registrada!");
                    logger.LogInformation("🔧 Configuração de handlers: {HandlerCount} handlers registrados",
                        eventDispatcher.GetHandlerCount("*"));

                    // Configurar serviços de parsing usando DI
                    logger.LogInformation("🔧 Configurando serviços de parsing...");
                    var protocol16Deserializer =
                        serviceProvider.GetRequiredService<Core.Services.Protocol16Deserializer>();


                    // Configurar captura de pacotes
                    logger.LogInformation("🔧 Configurando captura de pacotes...");
                    var capturePipeline = serviceProvider.GetRequiredService<App.Services.CapturePipeline>();
                    logger.LogInformation("✅ Captura de pacotes configurada (pipeline)!");

                    // Iniciar captura
                    logger.LogInformation("🚀 Iniciando captura de pacotes...");
                    capturePipeline.Start();

                    logger.LogInformation("✅ AlbionOnlineSniffer iniciado com sucesso!");
                    logger.LogInformation("📡 Aguardando pacotes do Albion Online...");
                    logger.LogInformation("🛑 Pressione Ctrl+C para parar");

                    // Manter a aplicação rodando
                    Console.CancelKeyPress += (sender, e) =>
                    {
                        logger.LogInformation("🛑 Parando captura...");
                        capturePipeline.Stop();
                        logger.LogInformation("✅ Captura parada. Saindo...");
                        Environment.Exit(0);
                    };

                    // Aguardar indefinidamente
                    while (true)
                    {
                        Thread.Sleep(1000);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "❌ Erro crítico na aplicação: {Message}", ex.Message);
                    Console.WriteLine($"❌ ERRO CRÍTICO: {ex.Message}");
                    Console.WriteLine($"📋 Stack Trace: {ex.StackTrace}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERRO FATAL: {ex.Message}");
                Console.WriteLine($"📋 Tipo: {ex.GetType().Name}");
                Console.WriteLine($"📋 Stack Trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"📋 Inner Exception: {ex.InnerException.Message}");
                    Console.WriteLine($"📋 Inner Stack Trace: {ex.InnerException.StackTrace}");
                }

                Environment.Exit(1);
            }
        }
    }
}
