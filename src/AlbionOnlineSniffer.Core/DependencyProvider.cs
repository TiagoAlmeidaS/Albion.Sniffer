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

namespace AlbionOnlineSniffer.Core
{
    /// <summary>
    /// Configuração de Dependency Injection para o módulo Core
    /// </summary>
    public static class DependencyProvider
    {
        /// <summary>
        /// Registra os serviços de carregamento de dados com possibilidade de sobrescrita
        /// </summary>
        /// <param name="services">ServiceCollection para registrar os serviços</param>
        /// <param name="customPacketOffsets">PacketOffsets customizado (opcional)</param>
        /// <param name="customPacketIndexes">PacketIndexes customizado (opcional)</param>
        public static void RegisterDataLoader(IServiceCollection services, PacketOffsets customPacketOffsets = null, PacketIndexes customPacketIndexes = null)
        {
            // Services
            services.AddSingleton<PacketOffsetsLoader>();
            services.AddSingleton<PacketIndexesLoader>();

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
                    logger2.LogError("Arquivo offsets.json não encontrado. Tentou os seguintes caminhos: {Paths}", paths);
                    throw new FileNotFoundException($"Arquivo offsets.json não encontrado. Tentou os seguintes caminhos: {paths}");
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
                    logger2.LogError("Arquivo indexes.json não encontrado. Tentou os seguintes caminhos: {Paths}", paths);
                    throw new FileNotFoundException($"Arquivo indexes.json não encontrado. Tentou os seguintes caminhos: {paths}");
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

            // Registra o novo PacketOffsets
            services.AddSingleton<PacketOffsets>(packetOffsets);
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

            // Registra o novo PacketIndexes
            services.AddSingleton<PacketIndexes>(packetIndexes);
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
            services.AddSingleton<PositionDecryptor>();
            services.AddSingleton<ClusterService>();
            services.AddSingleton<ItemDataService>();
            services.AddSingleton<DataLoaderService>();
            services.AddSingleton<AlbionNetworkHandlerManager>();

            RegisterDataLoader(services);

            // Event Factory para criação de eventos com injeção de dependência
            services.AddSingleton<IEventFactory, EventFactory>();

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