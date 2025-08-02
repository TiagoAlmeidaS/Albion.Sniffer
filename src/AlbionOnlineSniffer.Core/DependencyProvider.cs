using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Managers;

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

            // Managers
            services.AddSingleton<PlayersManager>();
            services.AddSingleton<MobsManager>();
            services.AddSingleton<HarvestablesManager>();
            services.AddSingleton<LootChestsManager>();
            services.AddSingleton<DungeonsManager>();
            services.AddSingleton<FishNodesManager>();
            services.AddSingleton<GatedWispsManager>();
        }
    }
} 