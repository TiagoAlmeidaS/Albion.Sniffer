using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Interfaces;
using AlbionOnlineSniffer.Core.Models;
using AlbionOnlineSniffer.Core.Models.GameObjects.Players;
using AlbionOnlineSniffer.Core.Models.GameObjects.Mobs;
using AlbionOnlineSniffer.Core.Models.GameObjects.Dungeons;
using AlbionOnlineSniffer.Core.Models.GameObjects.FishNodes;
using AlbionOnlineSniffer.Core.Models.GameObjects.GatedWisps;
using AlbionOnlineSniffer.Core.Models.GameObjects.Harvestables;
using AlbionOnlineSniffer.Core.Models.GameObjects.Localplayer;
using AlbionOnlineSniffer.Core.Models.GameObjects.LootChests;
using AlbionOnlineSniffer.Core.Models.ResponseObj;
using System.Linq;
using Albion.Network;

namespace AlbionOnlineSniffer.Core
{
    /// <summary>
    /// Configuração de Dependency Injection para o módulo Core
    /// </summary>
    public static class DependencyProvider
    {
        private sealed class StubPhotonReceiver : IPhotonReceiver
        {
            public void ReceivePacket(byte[] payload) { }
        }

        /// <summary>
        /// Registra os serviços de carregamento de dados com possibilidade de sobrescrita
        /// </summary>
        /// <param name="services">ServiceCollection para registrar os serviços</param>
        /// <param name="customPacketOffsets">PacketOffsets customizado (opcional)</param>
        /// <param name="customPacketIndexes">PacketIndexes customizado (opcional)</param>
        public static void RegisterDataLoader(IServiceCollection services, PacketOffsets customPacketOffsets = null, PacketIndexes customPacketIndexes = null)
        {
            // Services
            services.AddSingleton<PacketOffsetsLoader>(sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                return new PacketOffsetsLoader(loggerFactory.CreateLogger<PacketOffsetsLoader>());
            });
            services.AddSingleton<PacketIndexesLoader>(sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                return new PacketIndexesLoader(loggerFactory.CreateLogger<PacketIndexesLoader>());
            });

            // Carregamento e injeção de PacketOffsets e PacketIndexes (a partir do Core)
            services.AddSingleton<PacketOffsets>(provider =>
            {
                // Se um PacketOffsets customizado foi fornecido, usa ele
                if (customPacketOffsets != null)
                {
                    var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger("Core.DependencyProvider.PacketOffsets");
                    logger.LogInformation("✅ Usando PacketOffsets customizado fornecido");
                    return customPacketOffsets;
                }

                var loggerFactory2 = provider.GetRequiredService<ILoggerFactory>();
                var logger2 = loggerFactory2.CreateLogger("Core.DependencyProvider.PacketOffsets");
                var offsetsLoader = provider.GetRequiredService<PacketOffsetsLoader>();

                // Tente localizar um arquivo de offsets em locais conhecidos ou por padrão de nome
                string? offsetsPath = null;

                string[] probePaths = new[]
                {
                    Path.Combine(AppContext.BaseDirectory, "src/AlbionOnlineSniffer.Core/Data/jsons/offsets.json"),
                    Path.Combine(Directory.GetCurrentDirectory(), "src/AlbionOnlineSniffer.Core/Data/jsons/offsets.json"),
                    Path.Combine(AppContext.BaseDirectory, "offsets.json"),
                    Path.Combine(Directory.GetCurrentDirectory(), "offsets.json")
                };

                offsetsPath = probePaths.FirstOrDefault(File.Exists);

                if (offsetsPath == null)
                {
                    // Busca por qualquer arquivo que contenha "offsets" no nome
                    var currentDir = Directory.GetCurrentDirectory();
                    var baseDir = AppContext.BaseDirectory;
                    var candidates = Directory.Exists(currentDir) ? Directory.GetFiles(currentDir, "*offsets*.json").ToList() : new List<string>();
                    if (Directory.Exists(baseDir))
                    {
                        candidates.AddRange(Directory.GetFiles(baseDir, "*offsets*.json"));
                    }
                    offsetsPath = candidates.FirstOrDefault();
                }

                if (offsetsPath == null)
                {
                    logger2.LogWarning("Arquivo offsets.json não encontrado. Usando PacketOffsets vazio (fallback).");
                    return new PacketOffsets();
                }

                logger2.LogInformation("📂 Carregando offsets de: {Path}", offsetsPath);
                var packetOffsets = offsetsLoader.LoadOffsets(offsetsPath);
                logger2.LogInformation("✅ Offsets carregados e registrados no container");
                return packetOffsets;
            });

            services.AddSingleton<PacketIndexes>(provider =>
            {
                // Se um PacketIndexes customizado foi fornecido, usa ele
                if (customPacketIndexes != null)
                {
                    var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger("Core.DependencyProvider.PacketIndexes");
                    logger.LogInformation("✅ Usando PacketIndexes customizado fornecido");
                    return customPacketIndexes;
                }

                var loggerFactory2 = provider.GetRequiredService<ILoggerFactory>();
                var logger2 = loggerFactory2.CreateLogger("Core.DependencyProvider.PacketIndexes");
                var indexesLoader = provider.GetRequiredService<PacketIndexesLoader>();

                // Tente localizar um arquivo de indexes em locais conhecidos ou por padrão de nome
                string? indexesPath = null;

                string[] probePaths = new[]
                {
                    Path.Combine(AppContext.BaseDirectory, "src/AlbionOnlineSniffer.Core/Data/jsons/indexes.json"),
                    Path.Combine(Directory.GetCurrentDirectory(), "src/AlbionOnlineSniffer.Core/Data/jsons/indexes.json"),
                    Path.Combine(AppContext.BaseDirectory, "indexes.json"),
                    Path.Combine(Directory.GetCurrentDirectory(), "indexes.json")
                };

                indexesPath = probePaths.FirstOrDefault(File.Exists);

                if (indexesPath == null)
                {
                    // Busca por qualquer arquivo que contenha "indexes" no nome
                    var currentDir = Directory.GetCurrentDirectory();
                    var baseDir = AppContext.BaseDirectory;
                    var candidates = Directory.Exists(currentDir) ? Directory.GetFiles(currentDir, "*indexes*.json").ToList() : new List<string>();
                    if (Directory.Exists(baseDir))
                    {
                        candidates.AddRange(Directory.GetFiles(baseDir, "*indexes*.json"));
                    }
                    indexesPath = candidates.FirstOrDefault();
                }

                if (indexesPath == null)
                {
                    logger2.LogWarning("Arquivo indexes.json não encontrado. Usando PacketIndexes vazio (fallback).");
                    return new PacketIndexes();
                }

                logger2.LogInformation("📂 Carregando indexes de: {Path}", indexesPath);
                var packetIndexes = indexesLoader.LoadIndexes(indexesPath);
                logger2.LogInformation("✅ Indexes carregados e registrados no container");
                return packetIndexes;
            });
        }

        /// <summary>
        /// Método para permitir sobrescrita completa dos PacketOffsets
        /// Útil para testes ou configurações específicas
        /// </summary>
        /// <param name="services">ServiceCollection</param>
        /// <param name="packetOffsets">PacketOffsets para sobrescrever o existente</param>
        public static void OverridePacketOffsets(IServiceCollection services, PacketOffsets packetOffsets)
        {
            // Remove o registro existente se houver
            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(PacketOffsets));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Registra o novo PacketOffsets (permitir null conforme testes)
            services.AddSingleton<PacketOffsets>(sp => packetOffsets);
        }

        /// <summary>
        /// Método para permitir sobrescrita completa dos PacketIndexes
        /// Útil para testes ou configurações específicas
        /// </summary>
        /// <param name="services">ServiceCollection</param>
        /// <param name="packetIndexes">PacketIndexes para sobrescrever o existente</param>
        public static void OverridePacketIndexes(IServiceCollection services, PacketIndexes packetIndexes)
        {
            // Remove o registro existente se houver
            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(PacketIndexes));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Registra o novo PacketIndexes (permitir null conforme testes)
            services.AddSingleton<PacketIndexes>(sp => packetIndexes);
        }

        
        /// <summary>
        /// Registra todos os serviços do Core
        /// </summary>
        /// <param name="services">ServiceCollection para registrar os serviços</param>
        public static void RegisterServices(IServiceCollection services)
        {
            // AlbionEventLogger - Sistema de logs personalizado para web
            services.AddSingleton<IAlbionEventLogger, AlbionEventLogger>();

            // AlbionLogsApiService - Serviço para expor logs via API
            services.AddSingleton<AlbionLogsApiService>();

            // Services
            services.AddSingleton<PacketOffsetsLoader>(sp =>
            {
                var factory = sp.GetRequiredService<ILoggerFactory>();
                return new PacketOffsetsLoader(factory.CreateLogger<PacketOffsetsLoader>());
            });
            services.AddSingleton<PacketIndexesLoader>(sp =>
            {
                var factory = sp.GetRequiredService<ILoggerFactory>();
                return new PacketIndexesLoader(factory.CreateLogger<PacketIndexesLoader>());
            });
            services.AddSingleton<PhotonDefinitionLoader>(sp =>
            {
                var factory = sp.GetRequiredService<ILoggerFactory>();
                return new PhotonDefinitionLoader(factory.CreateLogger<PhotonDefinitionLoader>());
            });
            services.AddSingleton<Protocol16Deserializer>(sp =>
            {
                var factory = sp.GetRequiredService<ILoggerFactory>();
                var logger = factory.CreateLogger<Protocol16Deserializer>();
                // Minimal stub receiver that no-ops
                var stubReceiver = new StubPhotonReceiver();
                return new Protocol16Deserializer(stubReceiver, logger);
            });
            services.AddSingleton<PositionDecryptor>(sp =>
            {
                var factory = sp.GetRequiredService<ILoggerFactory>();
                return new PositionDecryptor(factory.CreateLogger<PositionDecryptor>());
            });
            services.AddSingleton<ClusterService>(sp =>
            {
                var factory = sp.GetRequiredService<ILoggerFactory>();
                return new ClusterService(factory.CreateLogger<ClusterService>());
            });
            services.AddSingleton<ItemDataService>(sp =>
            {
                var factory = sp.GetRequiredService<ILoggerFactory>();
                return new ItemDataService(factory.CreateLogger<ItemDataService>());
            });
            services.AddSingleton<DataLoaderService>(sp =>
            {
                var factory = sp.GetRequiredService<ILoggerFactory>();
                return new DataLoaderService(factory.CreateLogger<DataLoaderService>());
            });
            services.AddSingleton<AlbionNetworkHandlerManager>(sp =>
            {
                var factory = sp.GetRequiredService<ILoggerFactory>();
                return new AlbionNetworkHandlerManager(factory.CreateLogger<AlbionNetworkHandlerManager>(), sp);
            });

            // Only register data loader if not already registered externally
            if (!services.Any(d => d.ServiceType == typeof(PacketOffsets)))
            {
                RegisterDataLoader(services);
            }

            // Event Factory para criação de eventos com injeção de dependência
            services.AddSingleton<IEventFactory, EventFactory>();

            // EventDispatcher com AlbionEventLogger
            services.AddSingleton<EventDispatcher>(sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var eventLogger = sp.GetRequiredService<IAlbionEventLogger>();
                return new EventDispatcher(loggerFactory.CreateLogger<EventDispatcher>(), eventLogger);
            });

            // Game Object Handlers com dados carregados
            services.AddSingleton<LocalPlayerHandler>(provider =>
            {
                var dataLoader = provider.GetRequiredService<DataLoaderService>();
                var clusters = dataLoader.LoadClusters();
                return new LocalPlayerHandler(clusters);
            });

            services.AddSingleton<PlayersHandler>(provider =>
            {
                var dataLoader = provider.GetRequiredService<DataLoaderService>();
                var items = dataLoader.LoadItems();
                return new PlayersHandler(items);
            });

            services.AddSingleton<HarvestablesHandler>(provider =>
            {
                var dataLoader = provider.GetRequiredService<DataLoaderService>();
                var harvestables = dataLoader.LoadHarvestables();
                var localPlayerHandler = provider.GetRequiredService<LocalPlayerHandler>();
                return new HarvestablesHandler(harvestables, localPlayerHandler);
            });

            services.AddSingleton<MobsHandler>(provider =>
            {
                var dataLoader = provider.GetRequiredService<DataLoaderService>();
                var mobs = dataLoader.LoadMobs();
                return new MobsHandler(mobs);
            });

            // Handlers simples
            services.AddSingleton<DungeonsHandler>();
            services.AddSingleton<FishNodesHandler>();
            services.AddSingleton<GatedWispsHandler>();
            services.AddSingleton<LootChestsHandler>();

            // Configuration
            services.AddSingleton<ConfigHandler>();
        }

        /// <summary>
        /// Configura o PacketOffsetsProvider após a construção do ServiceProvider
        /// Este método deve ser chamado após a criação do ServiceProvider
        /// </summary>
        /// <param name="serviceProvider">ServiceProvider configurado</param>
        public static void ConfigurePacketOffsetsProvider(IServiceProvider serviceProvider)
        {
            PacketOffsetsProvider.Configure(serviceProvider);
        }
    }
} 