using Microsoft.Extensions.DependencyInjection;
using Albion.Sniffer.Core.Services;
using Albion.Sniffer.Core.Models;
using Albion.Sniffer.Core.Models.GameObjects.Players;
using Albion.Sniffer.Core.Models.GameObjects.Mobs;
using Albion.Sniffer.Core.Models.GameObjects.Dungeons;
using Albion.Sniffer.Core.Models.GameObjects.FishNodes;
using Albion.Sniffer.Core.Models.GameObjects.GatedWisps;
using Albion.Sniffer.Core.Models.GameObjects.Harvestables;
using Albion.Sniffer.Core.Models.GameObjects.Localplayer;
using Albion.Sniffer.Core.Models.GameObjects.LootChests;
using Albion.Sniffer.Core.Models.ResponseObj;

namespace Albion.Sniffer.Core
{
    /// <summary>
    /// Configuração de Dependency Injection para o módulo Core
    /// </summary>
    public static class DependencyProvider
    {
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

            services.AddSingleton<MobsHandler>(provider =>
            {
                var dataLoader = provider.GetRequiredService<DataLoaderService>();
                var mobs = dataLoader.LoadMobs();
                return new MobsHandler(mobs);
            });

            services.AddSingleton<HarvestablesHandler>(provider =>
            {
                var dataLoader = provider.GetRequiredService<DataLoaderService>();
                var harvestables = dataLoader.LoadHarvestables();
                var localPlayerHandler = provider.GetRequiredService<LocalPlayerHandler>();
                return new HarvestablesHandler(harvestables, localPlayerHandler);
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