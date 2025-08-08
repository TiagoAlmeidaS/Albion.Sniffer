using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using AlbionOnlineSniffer.Core.Services;
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

namespace AlbionOnlineSniffer.Core
{
    /// <summary>
    /// Configuração de Dependency Injection para o módulo Core
    /// </summary>
    public static class DependencyProvider
    {
        public static void RegisterDataLoader(IServiceCollection services)
        {
                        // Carregamento e injeção de PacketOffsets e PacketIndexes (a partir do Core)
            services.AddSingleton<AlbionOnlineSniffer.Core.Models.ResponseObj.PacketOffsets>(provider =>
            {
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("Core.DependencyProvider.PacketOffsets");
                var offsetsLoader = provider.GetRequiredService<PacketOffsetsLoader>();

                var possibleOffsetsPaths = new[]
                {
                        Path.Combine(AppContext.BaseDirectory, "src/AlbionOnlineSniffer.Core/Data/jsons/offsets.json"),
                        Path.Combine(Directory.GetCurrentDirectory(), "src/AlbionOnlineSniffer.Core/Data/jsons/offsets.json"),
                        Path.Combine(AppContext.BaseDirectory, "offsets.json"),
                        Path.Combine(Directory.GetCurrentDirectory(), "offsets.json")
                };

                string? offsetsPath = null;
                foreach (var path in possibleOffsetsPaths)
                {
                    if (File.Exists(path))
                    {
                        offsetsPath = path;
                        break;
                    }
                }

                if (offsetsPath == null)
                {
                    var paths = string.Join(", ", possibleOffsetsPaths);
                    logger.LogError("Arquivo offsets.json não encontrado. Tentou os seguintes caminhos: {Paths}", paths);
                    throw new FileNotFoundException($"Arquivo offsets.json não encontrado. Tentou os seguintes caminhos: {paths}");
                }

                logger.LogInformation("📂 Carregando offsets de: {Path}", offsetsPath);
                var packetOffsets = offsetsLoader.LoadOffsets(offsetsPath);
                logger.LogInformation("✅ Offsets carregados e registrados no container");
                return packetOffsets;
            });

            services.AddSingleton<AlbionOnlineSniffer.Core.Models.ResponseObj.PacketIndexes>(provider =>
            {
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("Core.DependencyProvider.PacketIndexes");
                var indexesLoader = provider.GetRequiredService<PacketIndexesLoader>();

                var possibleIndexesPaths = new[]
                {
                        Path.Combine(AppContext.BaseDirectory, "src/AlbionOnlineSniffer.Core/Data/jsons/indexes.json"),
                        Path.Combine(Directory.GetCurrentDirectory(), "src/AlbionOnlineSniffer.Core/Data/jsons/indexes.json"),
                        Path.Combine(AppContext.BaseDirectory, "indexes.json"),
                        Path.Combine(Directory.GetCurrentDirectory(), "indexes.json")
                };

                string? indexesPath = null;
                foreach (var path in possibleIndexesPaths)
                {
                    if (File.Exists(path))
                    {
                        indexesPath = path;
                        break;
                    }
                }

                if (indexesPath == null)
                {
                    var paths = string.Join(", ", possibleIndexesPaths);
                    logger.LogError("Arquivo indexes.json não encontrado. Tentou os seguintes caminhos: {Paths}", paths);
                    throw new FileNotFoundException($"Arquivo indexes.json não encontrado. Tentou os seguintes caminhos: {paths}");
                }

                logger.LogInformation("📂 Carregando indexes de: {Path}", indexesPath);
                var packetIndexes = indexesLoader.LoadIndexes(indexesPath);
                logger.LogInformation("✅ Indexes carregados e registrados no container");
                return packetIndexes;
            });
        }

        
        /// <summary>
        /// Registra todos os serviços do módulo Core
        /// </summary>
        /// <param name="services">ServiceCollection para registrar os serviços</param>
        public static void RegisterServices(IServiceCollection services)
        {
            // Services
            services.AddSingleton<EventDispatcher>();
            services.AddSingleton<PhotonDefinitionLoader>();
            services.AddSingleton<Protocol16Deserializer>();
            services.AddSingleton<PacketOffsetsLoader>();
            services.AddSingleton<PacketIndexesLoader>();
            services.AddSingleton<PositionDecryptor>();
            services.AddSingleton<ClusterService>();
            services.AddSingleton<ItemDataService>();
            services.AddSingleton<DataLoaderService>();
            services.AddSingleton<AlbionNetworkHandlerManager>();

            RegisterDataLoader(services);

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
    }
} 