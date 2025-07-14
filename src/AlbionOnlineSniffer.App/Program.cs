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

using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Queue;
using AlbionOnlineSniffer.Queue.Interfaces;
using AlbionOnlineSniffer.Core;
using AlbionOnlineSniffer.Core.Handlers;
using AlbionOnlineSniffer.Core.Services;
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
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                // Publishers
                var rabbitHost = config["RabbitMQ:ConnectionString"];
                var rabbitExchange = config["RabbitMQ:Exchange"];
                using IQueuePublisher rabbitPublisher = Queue.DependencyProvider.CreateRabbitMqPublisher(rabbitHost, rabbitExchange);
                // using IQueuePublisher redisPublisher = Queue.DependencyProvider.CreateRedisPublisher(redisConn);
                
                //TODO: Implementar Redis Publisher posteriormente
                // var redisConn = config["Redis:ConnectionString"];
                // using IQueuePublisher redisPublisher = Queue.DependencyProvider.CreateRedisPublisher(redisConn);

                // Core Handlers
                var playersManager = Core.DependencyProvider.CreatePlayersManager();
                var mobsManager = Core.DependencyProvider.CreateMobsManager();
                var harvestablesManager = Core.DependencyProvider.CreateHarvestablesManager();
                var lootChestsManager = Core.DependencyProvider.CreateLootChestsManager();
                var dungeonsManager = Core.DependencyProvider.CreateDungeonsManager();
                var fishNodesManager = Core.DependencyProvider.CreateFishNodesManager();
                var gatedWispsManager = Core.DependencyProvider.CreateGatedWispsManager();
                var configHandler = Core.DependencyProvider.CreateConfigHandler();
                var localPlayerHandler = Core.DependencyProvider.CreateLocalPlayerHandler();

                // Event Handlers
                var characterHandler = new NewCharacterEventHandler(playersManager, localPlayerHandler, configHandler);
                var mobHandler = new NewMobEventHandler(mobsManager);
                var harvestableHandler = new NewHarvestableEventHandler(harvestablesManager);
                var lootChestHandler = new NewLootChestEventHandler(lootChestsManager);
                var dungeonHandler = new NewDungeonEventHandler(dungeonsManager);
                var fishingZoneHandler = new NewFishingZoneEventHandler(fishNodesManager);
                var gatedWispHandler = new NewGatedWispEventHandler(gatedWispsManager);
                var wispGateOpenedHandler = new WispGateOpenedEventHandler(gatedWispsManager);

                // Conectar eventos dos handlers aos publishers
                characterHandler.OnCharacterParsed += async data => {
                    logger.LogInformation("Evento: Novo personagem detectado");
                    await rabbitPublisher.PublishAsync("character.new", data);
                    // await redisPublisher.PublishAsync("character.new", data);
                };
                mobHandler.OnMobParsed += async data => {
                    logger.LogInformation("Evento: Novo mob detectado");
                    await rabbitPublisher.PublishAsync("mob.new", data);
                    // await redisPublisher.PublishAsync("mob.new", data);
                };
                harvestableHandler.OnHarvestableParsed += async data => {
                    logger.LogInformation("Evento: Novo harvestable detectado");
                    await rabbitPublisher.PublishAsync("harvestable.new", data);
                    // await redisPublisher.PublishAsync("harvestable.new", data);
                };
                lootChestHandler.OnLootChestParsed += async data => {
                    logger.LogInformation("Evento: Novo loot chest detectado");
                    await rabbitPublisher.PublishAsync("lootchest.new", data);
                    // await redisPublisher.PublishAsync("lootchest.new", data);
                };
                dungeonHandler.OnDungeonParsed += async data => {
                    logger.LogInformation("Evento: Nova dungeon detectada");
                    await rabbitPublisher.PublishAsync("dungeon.new", data);
                    // await redisPublisher.PublishAsync("dungeon.new", data);
                };
                fishingZoneHandler.OnFishingZoneParsed += async data => {
                    logger.LogInformation("Evento: Nova fishing zone detectada");
                    await rabbitPublisher.PublishAsync("fishingzone.new", data);
                    // await redisPublisher.PublishAsync("fishingzone.new", data);
                };
                gatedWispHandler.OnGatedWispParsed += async data => {
                    logger.LogInformation("Evento: Novo gated wisp detectado");
                    await rabbitPublisher.PublishAsync("gatedwisp.new", data);
                    // await redisPublisher.PublishAsync("gatedwisp.new", data);
                };
                wispGateOpenedHandler.OnWispGateOpenedParsed += async data => {
                    logger.LogInformation("Evento: Wisp gate opened detectado");
                    await rabbitPublisher.PublishAsync("wispopened.new", data);
                    // await redisPublisher.PublishAsync("wispopened.new", data);
                };

                // Instanciar parser com handlers
                var parser = new Protocol16Deserializer(new object[]
                {
                    characterHandler, mobHandler, harvestableHandler, lootChestHandler,
                    dungeonHandler, fishingZoneHandler, gatedWispHandler, wispGateOpenedHandler
                });

                // Capture
                var capture = Capture.DependencyProvider.CreatePacketCaptureService(udpPort: 5056);
                capture.OnUdpPayloadCaptured += parser.ReceivePacket;

                // Iniciar captura
                logger.LogInformation("Iniciando captura de pacotes...");
                capture.Start();

                logger.LogInformation("Sniffer iniciado. Pressione ENTER para sair.");
                Console.ReadLine();

                capture.Dispose();
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
