using Microsoft.Extensions.DependencyInjection;
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

namespace AlbionOnlineSniffer.Core
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
            services.AddSingleton<AlbionNetworkHandlerManager>();
            
            // Game Object Handlers (substituem os Managers removidos)
            services.AddSingleton<PlayersHandler>();
            services.AddSingleton<MobsHandler>();
            services.AddSingleton<DungeonsHandler>();
            services.AddSingleton<FishNodesHandler>();
            services.AddSingleton<GatedWispsHandler>();
            services.AddSingleton<HarvestablesHandler>();
            services.AddSingleton<LocalPlayerHandler>();
            services.AddSingleton<LootChestsHandler>();
            
            // Configuration
            services.AddSingleton<ConfigHandler>();
        }
    }
} 