/*
 * AlbionOnlineSniffer - Fluxo Principal
 *
 * 1. Lê configurações de appsettings.json (RabbitMQ, Redis, etc).
 * 2. Instancia publishers de fila (RabbitMQ, Redis) via DependencyProvider do módulo Queue.
 * 3. Instancia todos os handlers e serviços de domínio via DependencyProvider do módulo Core.
 * 4. Conecta eventos dos handlers aos publishers (publicação automática de eventos parseados).
 * 5. Instancia o parser central (Protocol16Deserializer) com todos os handlers.
 * 6. Instancia o capturador de pacotes via DependencyProvider do módulo Capture e conecta ao parser.
 * 7. Inicia a captura de pacotes. Ao receber pacotes, o fluxo é: Captura -> Parsing -> Handler -> Publicação.
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
                var playerManager = serviceProvider.GetRequiredService<Core.Handlers.PlayersManager>();
                var mobManager = serviceProvider.GetRequiredService<Core.Handlers.MobsManager>();
                var harvestableManager = serviceProvider.GetRequiredService<Core.Handlers.HarvestablesManager>();
                var lootChestManager = serviceProvider.GetRequiredService<Core.Handlers.LootChestsManager>();
                var eventDispatcher = serviceProvider.GetRequiredService<Core.Services.EventDispatcher>();
                
                // Configure event handlers
                Core.Services.EventServiceExamples.ConfigureEventHandlers(eventDispatcher, logger);

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

                // Carregar dados de clusters
                var clusterService = serviceProvider.GetRequiredService<Core.Services.ClusterService>();
                var clustersPath = Path.Combine(Directory.GetCurrentDirectory(), "src", "AlbionOnlineSniffer.Core", "Data", "jsons", "clusters.json");
                if (File.Exists(clustersPath))
                {
                    clusterService.LoadClusters(clustersPath);
                    logger.LogInformation("Dados de clusters carregados com sucesso");
                }
                else
                {
                    logger.LogWarning("Arquivo de clusters não encontrado: {Path}", clustersPath);
                }

                // Carregar dados de itens
                var itemDataService = serviceProvider.GetRequiredService<Core.Services.ItemDataService>();
                var itemsPath = Path.Combine(Directory.GetCurrentDirectory(), "ao-bin-dumps", "items.xml");
                if (File.Exists(itemsPath))
                {
                    itemDataService.LoadItems(itemsPath);
                    logger.LogInformation("Dados de itens carregados com sucesso");
                }
                else
                {
                    logger.LogWarning("Arquivo de itens não encontrado: {Path}", itemsPath);
                }

                // Configurar processador de pacotes
                var packetProcessor = serviceProvider.GetRequiredService<Core.Services.PacketProcessor>();
                
                // Configurar parser
                var parser = Core.DependencyProvider.CreateProtocol16Deserializer(
                    packetEnricher, 
                    packetProcessor,
                    loggerFactory.CreateLogger<Core.Services.Protocol16Deserializer>());

                // Conectar evento de pacotes enriquecidos ao publisher
                parser.OnEnrichedPacket += async enrichedPacket =>
                {
                    var topic = $"albion.packet.{enrichedPacket.PacketName.ToLowerInvariant()}";
                    var message = enrichedPacket.ToSerializableObject();
                    await publisher.PublishAsync(topic, message);
                    
                    logger.LogDebug("Pacote enriquecido publicado: {PacketName} -> {Topic}", 
                        enrichedPacket.PacketName, topic);
                };

                logger.LogInformation("Sistema de eventos e parsing configurado com sucesso!");
                logger.LogInformation("Managers disponíveis: Players={PlayerCount}, Mobs={MobCount}, Harvestables={HarvestableCount}, LootChests={LootChestCount}", 
                    playerManager.PlayerCount, mobManager.MobCount, harvestableManager.HarvestableCount, lootChestManager.LootChestCount);

                // Capture
                var capture = Capture.DependencyProvider.CreatePacketCaptureService(udpPort: 5050);
                capture.OnUdpPayloadCaptured += parser.ReceivePacket;

                // Iniciar captura
                logger.LogInformation("Iniciando captura de pacotes na porta UDP {Port}...", 5050);
                capture.Start();

                logger.LogInformation("Sniffer iniciado. Sistema de eventos funcionando. Pressione ENTER para sair.");
                Console.ReadLine();

                // capture.Dispose(); // Temporariamente comentado
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
