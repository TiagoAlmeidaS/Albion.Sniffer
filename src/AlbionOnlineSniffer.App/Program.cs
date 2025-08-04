/*
 * AlbionOnlineSniffer - Fluxo Principal
 *
 * 1. Lê configurações de appsettings.json (RabbitMQ, Redis, etc).
 * 2. Instancia publishers de fila (RabbitMQ, Redis) via DependencyProvider do módulo Queue.
 * 3. Instancia todos os handlers e serviços de domínio via DependencyProvider do módulo Core.
 * 4. Conecta EventDispatcher ao Publisher para publicação automática de eventos.
 * 5. Instancia o parser central (Protocol16Deserializer) com PacketProcessor via DI.
 * 6. Instancia o capturador de pacotes via DependencyProvider do módulo Capture e conecta ao parser.
 * 7. Inicia a captura de pacotes. Ao receber pacotes, o fluxo é: Captura -> Parsing -> PacketProcessor -> EventDispatcher -> Publisher.
 * 8. Logging estruturado e tratamento de erros garantem rastreabilidade e robustez.
 *
 * Para adicionar novos eventos, handlers ou publishers, basta expandir os DependencyProviders e conectar os eventos desejados.
 */

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using AlbionOnlineSniffer.Queue;
using AlbionOnlineSniffer.Core;
using AlbionOnlineSniffer.Capture;

namespace AlbionOnlineSniffer.App
{
    class Program
    {
        static void Main(string[] args)
        {

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
                
                if (LogoLoader.IsLogoAvailable())
                {
                    logger.LogInformation("✅ Logo encontrada no executável!");
                    
                    var logoBytes = LogoLoader.LoadLogoAsBytes();
                    if (logoBytes != null)
                    {
                        logger.LogInformation("📊 Tamanho da logo: {Size} bytes", logoBytes.Length);
                    }
                    
                    var embeddedResources = LogoLoader.GetEmbeddedResources();
                    logger.LogInformation("📦 Recursos embutidos: {Count} recursos", embeddedResources.Length);
                    foreach (var resource in embeddedResources)
                    {
                        logger.LogInformation("   - {Resource}", resource);
                    }
                }
                else
                {
                    logger.LogWarning("⚠️ Logo não encontrada no executável!");
                }

                // Configuração
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var binDumpsEnabled = configuration.GetValue<bool>("BinDumps:Enabled", true);
                var binDumpsPath = configuration.GetValue<string>("BinDumps:BasePath", "ao-bin-dumps");
                
                logger.LogInformation("Configuração de bin-dumps: Habilitado={Enabled}, Caminho={Path}", 
                    binDumpsEnabled, binDumpsPath);

                // Publishers
                var rabbitMqConnectionString = configuration.GetConnectionString("RabbitMQ") ?? 
                    configuration.GetValue<string>("RabbitMQ:ConnectionString") ?? 
                    "amqp://localhost";
                var exchange = configuration.GetValue<string>("RabbitMQ:Exchange", "albion.sniffer");
                
                var publisher = Queue.DependencyProvider.CreateRabbitMqPublisher(rabbitMqConnectionString, exchange);

                // Configure dependency injection
                var services = new ServiceCollection();
                
                // Add logging
                services.AddLogging(builder => builder.AddConsole());
                
                // 🔧 CARREGAR OFFSETS E INDEXES PRIMEIRO
                var packetOffsetsLoader = new Core.Services.PacketOffsetsLoader(loggerFactory.CreateLogger<Core.Services.PacketOffsetsLoader>());
                var packetIndexesLoader = new Core.Services.PacketIndexesLoader(loggerFactory.CreateLogger<Core.Services.PacketIndexesLoader>());
                
                // Carregar offsets e indexes
                var offsetsPath = Path.Combine(Directory.GetCurrentDirectory(), "src/AlbionOnlineSniffer.Core/Data/jsons/offsets.json");
                var indexesPath = Path.Combine(Directory.GetCurrentDirectory(), "src/AlbionOnlineSniffer.Core/Data/jsons/indexes.json");
                
                logger.LogInformation("📂 Carregando offsets de: {Path}", offsetsPath);
                var packetOffsets = packetOffsetsLoader.LoadOffsets(offsetsPath);
                
                logger.LogInformation("📂 Carregando indexes de: {Path}", indexesPath);
                var packetIndexes = packetIndexesLoader.LoadIndexes(indexesPath);
                
                logger.LogInformation("✅ Offsets e Indexes carregados com sucesso");
                
                // 🔧 VERIFICAR SE OS OFFSETS FORAM CARREGADOS CORRETAMENTE
                logger.LogInformation("🔍 VERIFICANDO OFFSETS CARREGADOS:");
                logger.LogInformation("  - Leave: [{Offsets}]", string.Join(", ", packetOffsets.Leave));
                logger.LogInformation("  - HealthUpdateEvent: [{Offsets}]", string.Join(", ", packetOffsets.HealthUpdateEvent));
                logger.LogInformation("  - NewCharacter: [{Offsets}]", string.Join(", ", packetOffsets.NewCharacter));
                logger.LogInformation("  - Move: [{Offsets}]", string.Join(", ", packetOffsets.Move));
                
                // 🔧 REGISTRAR OS OFFSETS CARREGADOS NO CONTAINER DI
                services.AddSingleton(packetOffsets);
                services.AddSingleton(packetIndexes);
                
                // Register Core services (agora com os offsets já carregados)
                Core.DependencyProvider.RegisterServices(services);
                
                // Build service provider
                var serviceProvider = services.BuildServiceProvider();
                
                // Get services from DI container
                var eventDispatcher = serviceProvider.GetRequiredService<Core.Services.EventDispatcher>();
                
                // 🔧 INTEGRAÇÃO COM MENSAGERIA - Conectar EventDispatcher ao Publisher
                eventDispatcher.RegisterGlobalHandler(async gameEvent =>
                {
                    try
                    {
                        var eventType = gameEvent.GetType().Name;
                        var timestamp = DateTime.UtcNow;
                        
                        logger.LogInformation("🎯 EVENTO RECEBIDO: {EventType} em {Timestamp}", 
                            eventType, timestamp);

                        var eventTypeFormatted = eventType.Replace("Event", "");
                        
                        var topic = $"albion.event.{eventTypeFormatted.ToLowerInvariant()}";
                        var message = new
                        {
                            EventType = eventType,
                            Timestamp = timestamp,
                            Data = gameEvent
                        };
                        
                        logger.LogInformation("📤 PUBLICANDO: {EventType} -> {Topic}", eventType, topic);
                        await publisher.PublishAsync(topic, message);
                        logger.LogInformation("✅ Evento publicado na fila: {EventType} -> {Topic}", 
                            eventType, topic);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "❌ Erro ao publicar evento na fila: {EventType} - {Message}", 
                            gameEvent.GetType().Name, ex.Message);
                    }
                });

                logger.LogInformation("✅ EventDispatcher conectado ao sistema de mensageria!");
                logger.LogInformation("🔧 Configuração de handlers: {HandlerCount} handlers registrados", 
                    eventDispatcher.GetHandlerCount("*"));

                // Configurar serviços de parsing usando DI
                var definitionLoader = serviceProvider.GetRequiredService<Core.Services.PhotonDefinitionLoader>();
                
                // Configurar Albion.Network com handlers
                var albionNetworkHandlerManager = serviceProvider.GetRequiredService<Core.Services.AlbionNetworkHandlerManager>();
                var receiverBuilder = albionNetworkHandlerManager.ConfigureReceiverBuilder();
                var photonReceiver = receiverBuilder.Build();
                
                // Criar Protocol16Deserializer com o receiver configurado
                var protocol16Deserializer = new Core.Services.Protocol16Deserializer(
                    photonReceiver, 
                    loggerFactory.CreateLogger<Core.Services.Protocol16Deserializer>()
                );

                // Carregar definições dos bin-dumps se habilitado
                if (binDumpsEnabled)
                {
                    try
                    {
                        var fullBinDumpsPath = Path.Combine(Directory.GetCurrentDirectory(), binDumpsPath);
                        if (Directory.Exists(fullBinDumpsPath))
                        {
                            definitionLoader.Load(fullBinDumpsPath);
                            logger.LogInformation("Definições dos bin-dumps carregadas com sucesso");
                        }
                        else
                        {
                            logger.LogWarning("Diretório de bin-dumps não encontrado: {Path}", fullBinDumpsPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Erro ao carregar definições dos bin-dumps: {Message}", ex.Message);
                    }
                }

                logger.LogInformation("Sistema de eventos e parsing configurado com sucesso!");

                // Capture
                var capture = Capture.DependencyProvider.CreatePacketCaptureService(udpPort: 5050);
                capture.OnUdpPayloadCaptured += protocol16Deserializer.ReceivePacket;

                // Iniciar captura
                logger.LogInformation("Iniciando captura de pacotes na porta UDP {Port}...", 5050);
                capture.Start();

                logger.LogInformation("Sniffer iniciado. Sistema de eventos funcionando. Pressione ENTER para sair.");
                Console.ReadLine();

                logger.LogInformation("Sniffer finalizado.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro fatal no sniffer: {Message}", ex.Message);
                Console.WriteLine($"Erro fatal: {ex.Message}");
            }
        }
    }
}
