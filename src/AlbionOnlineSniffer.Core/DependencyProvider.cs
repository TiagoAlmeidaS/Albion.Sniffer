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
using System.Linq;
using AlbionOnlineSniffer.Core.Interfaces;
using Albion.Network;

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
                    return customPacketOffsets;
                }

                var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")?.ToLowerInvariant();
                var basePath = AppContext.BaseDirectory;
                var jsonPath = Path.Combine(basePath, "Resources", env == "development" ? "offsets-debug.json" : "offsets.json");
                var loader = provider.GetRequiredService<PacketOffsetsLoader>();
                return loader.LoadOffsets(jsonPath);
            });

            services.AddSingleton<PacketIndexes>(provider =>
            {
                if (customPacketIndexes != null)
                {
                    return customPacketIndexes;
                }

                var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")?.ToLowerInvariant();
                var basePath = AppContext.BaseDirectory;
                var jsonPath = Path.Combine(basePath, "Resources", env == "development" ? "indexes-debug.json" : "indexes.json");
                var loader = provider.GetRequiredService<PacketIndexesLoader>();
                return loader.LoadIndexes(jsonPath);
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

            // Só registra se não for null
            if (packetOffsets != null)
            {
                services.AddSingleton<PacketOffsets>(packetOffsets);
            }
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
            if (packetIndexes != null)
            {
                services.AddSingleton<PacketIndexes>(packetIndexes);
            }
        }

        
        /// <summary>
        /// Registra todos os serviços do módulo Core
        /// </summary>
        /// <param name="services">ServiceCollection para registrar os serviços</param>
        public static void RegisterServices(IServiceCollection services)
        {
            // Logging
            services.AddLogging();

            // Services
            services.AddSingleton<EventDispatcher>();
            services.AddSingleton<PhotonDefinitionLoader>();
            services.AddSingleton<Protocol16Deserializer>();
            services.AddSingleton<PositionDecryptor>();
            services.AddSingleton<ClusterService>();
            services.AddSingleton<ItemDataService>();
            services.AddSingleton<DataLoaderService>();
            services.AddSingleton<AlbionNetworkHandlerManager>();

            // Event Factory para criação de eventos com injeção de dependência
            services.AddSingleton<IEventFactory, EventFactory>();

            // Registrar um IPhotonReceiver mínimo para testes
            services.AddSingleton<IPhotonReceiver>(sp =>
            {
                // Usa o manager para montar um receiver básico
                var manager = sp.GetRequiredService<AlbionNetworkHandlerManager>();
                var builder = manager.ConfigureReceiverBuilder();
                return builder.Build();
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