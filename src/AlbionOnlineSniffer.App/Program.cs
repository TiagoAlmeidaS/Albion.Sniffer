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

                // Configuração
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
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
                
                // Register Core services
                Core.DependencyProvider.RegisterServices(services);
                
                // Build service provider
                var serviceProvider = services.BuildServiceProvider();
                
                // Get services from DI container
                var eventDispatcher = serviceProvider.GetRequiredService<Core.Services.EventDispatcher>();
                var packetProcessor = serviceProvider.GetRequiredService<Core.Services.PacketProcessor>();
                
                // Configure event handlers (exemplos de uso)
                Core.Services.EventServiceExamples.ConfigureEventHandlers(eventDispatcher, logger);

                // 🔧 INTEGRAÇÃO COM MENSAGERIA - Conectar EventDispatcher ao Publisher
                eventDispatcher.RegisterGlobalHandler(async gameEvent =>
                {
                    try
                    {
                        var topic = $"albion.event.{gameEvent.EventType.ToLowerInvariant()}";
                        var message = new
                        {
                            EventType = gameEvent.EventType,
                            Timestamp = gameEvent.Timestamp,
                            Data = gameEvent
                        };
                        
                        await publisher.PublishAsync(topic, message);
                        logger.LogDebug("Evento publicado na fila: {EventType} -> {Topic}", 
                            gameEvent.EventType, topic);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Erro ao publicar evento na fila: {EventType}", gameEvent.EventType);
                    }
                });

                logger.LogInformation("✅ EventDispatcher conectado ao sistema de mensageria!");

                // Configurar serviços de parsing
                var definitionLoader = Core.DependencyProvider.CreatePhotonDefinitionLoader(
                    loggerFactory.CreateLogger<Core.Services.PhotonDefinitionLoader>());
                
                var packetEnricher = Core.DependencyProvider.CreatePhotonPacketEnricher(
                    definitionLoader, 
                    loggerFactory.CreateLogger<Core.Services.PhotonPacketEnricher>());

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

                // Configurar parser
                var parser = Core.DependencyProvider.CreateProtocol16Deserializer(
                    packetEnricher, 
                    packetProcessor,
                    loggerFactory.CreateLogger<Core.Services.Protocol16Deserializer>());

                logger.LogInformation("Sistema de eventos e parsing configurado com sucesso!");

                // Capture
                var capture = Capture.DependencyProvider.CreatePacketCaptureService(udpPort: 5050);
                capture.OnUdpPayloadCaptured += parser.ReceivePacket;

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
