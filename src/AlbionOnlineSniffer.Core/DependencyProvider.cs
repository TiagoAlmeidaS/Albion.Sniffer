using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Handlers;
using AlbionOnlineSniffer.Core.Models;

namespace AlbionOnlineSniffer.Core
{
    /// <summary>
    /// Factory para criação de dependências comuns
    /// </summary>
    public static class ServiceFactory
    {
        private static readonly ILoggerFactory _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        private static readonly string _offsetsPath = "src/AlbionOnlineSniffer.Core/Data/jsons/offsets.json";
        private static readonly string _indexesPath = "src/AlbionOnlineSniffer.Core/Data/jsons/indexes.json";

        /// <summary>
        /// Cria um logger para o tipo especificado
        /// </summary>
        public static ILogger<T> CreateLogger<T>()
        {
            return _loggerFactory.CreateLogger<T>();
        }

        /// <summary>
        /// Cria uma instância do PositionDecryptor
        /// </summary>
        public static PositionDecryptor CreatePositionDecryptor()
        {
            return new PositionDecryptor(CreateLogger<PositionDecryptor>());
        }

        /// <summary>
        /// Cria uma instância do EventDispatcher
        /// </summary>
        public static EventDispatcher CreateEventDispatcher()
        {
            return new EventDispatcher(CreateLogger<EventDispatcher>());
        }

        /// <summary>
        /// Cria uma instância do PacketOffsets carregado do JSON
        /// </summary>
        public static PacketOffsets CreatePacketOffsets()
        {
            var loader = new PacketOffsetsLoader(CreateLogger<PacketOffsetsLoader>());
            return loader.LoadOffsets(_offsetsPath);
        }

        /// <summary>
        /// Cria uma instância do PacketIndexes carregado do JSON
        /// </summary>
        public static PacketIndexes CreatePacketIndexes()
        {
            var loader = new PacketIndexesLoader(CreateLogger<PacketIndexesLoader>());
            return loader.LoadIndexes(_indexesPath);
        }

        /// <summary>
        /// Cria um handler com dependências comuns
        /// </summary>
        public static T CreateHandler<T>(Func<ILogger<T>, PositionDecryptor, PacketOffsets, T> factory)
            where T : class
        {
            return factory(CreateLogger<T>(), CreatePositionDecryptor(), CreatePacketOffsets());
        }

        /// <summary>
        /// Cria um manager com dependências comuns
        /// </summary>
        public static T CreateManager<T>(Func<ILogger<T>, EventDispatcher, T> factory)
            where T : class
        {
            return factory(CreateLogger<T>(), CreateEventDispatcher());
        }
    }

    /// <summary>
    /// Provedor de dependências para o módulo Core
    /// </summary>
    public static class DependencyProvider
    {
        /// <summary>
        /// Registra os serviços do módulo Core
        /// </summary>
        /// <param name="services">Coleção de serviços</param>
        public static void RegisterServices(IServiceCollection services)
        {
            // Serviços de parsing e enriquecimento
            services.AddSingleton<PhotonDefinitionLoader>();
            services.AddSingleton<PhotonPacketEnricher>();
            
            // Sistema de eventos
            services.AddSingleton<EventDispatcher>();
            
            // Gerenciador de handlers do Albion.Network
            services.AddSingleton<AlbionNetworkHandlerManager>(provider =>
            {
                var eventDispatcher = provider.GetRequiredService<EventDispatcher>();
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                var packetIndexes = provider.GetRequiredService<PacketIndexes>();
                return new AlbionNetworkHandlerManager(eventDispatcher, loggerFactory, packetIndexes);
            });
            
            // Protocol16Deserializer (configurado externamente)
            services.AddSingleton<Protocol16Deserializer>();

            // Serviços de decriptação e offsets
            services.AddSingleton<PositionDecryptor>();
            services.AddSingleton<PacketOffsetsLoader>();
            services.AddSingleton<PacketOffsets>();
            services.AddSingleton<PacketIndexesLoader>();
            services.AddSingleton<PacketIndexes>();

            // Sistema de eventos
            services.AddSingleton<EventDispatcher>();

            // Handlers agora gerenciados pelo AlbionNetworkHandlerManager

            // Managers
            services.AddSingleton<PlayersManager>();
            services.AddSingleton<MobsManager>();
            services.AddSingleton<HarvestablesManager>();
            services.AddSingleton<LootChestsManager>();
            services.AddSingleton<DungeonsManager>();
            services.AddSingleton<FishNodesManager>();
            services.AddSingleton<GatedWispsManager>();

            // Processador de pacotes
            services.AddSingleton<PacketProcessor>();
        }

        // Handlers antigos removidos - agora gerenciados pelo AlbionNetworkHandlerManager

        /// <summary>
        /// Cria uma instância do PlayersManager
        /// </summary>
        public static PlayersManager CreatePlayersManager()
        {
            return ServiceFactory.CreateManager<PlayersManager>((logger, eventDispatcher) =>
                new PlayersManager(logger, ServiceFactory.CreatePositionDecryptor(), eventDispatcher));
        }

        /// <summary>
        /// Cria uma instância do MobsManager
        /// </summary>
        public static MobsManager CreateMobsManager()
        {
            return ServiceFactory.CreateManager<MobsManager>((logger, eventDispatcher) =>
                new MobsManager(logger, ServiceFactory.CreatePositionDecryptor(), eventDispatcher));
        }

        /// <summary>
        /// Cria uma instância do HarvestablesManager
        /// </summary>
        public static HarvestablesManager CreateHarvestablesManager()
        {
            return ServiceFactory.CreateManager<HarvestablesManager>((logger, eventDispatcher) =>
                new HarvestablesManager(logger, ServiceFactory.CreatePositionDecryptor(), eventDispatcher));
        }

        /// <summary>
        /// Cria uma instância do LootChestsManager
        /// </summary>
        public static LootChestsManager CreateLootChestsManager()
        {
            return ServiceFactory.CreateManager<LootChestsManager>((logger, eventDispatcher) =>
                new LootChestsManager(logger, ServiceFactory.CreatePositionDecryptor(), eventDispatcher));
        }

        /// <summary>
        /// Cria uma instância do DungeonsManager
        /// </summary>
        public static DungeonsManager CreateDungeonsManager()
        {
            return ServiceFactory.CreateManager<DungeonsManager>((logger, eventDispatcher) =>
                new DungeonsManager(logger, eventDispatcher));
        }

        /// <summary>
        /// Cria uma instância do FishNodesManager
        /// </summary>
        public static FishNodesManager CreateFishNodesManager()
        {
            return ServiceFactory.CreateManager<FishNodesManager>((logger, eventDispatcher) =>
                new FishNodesManager(logger, eventDispatcher));
        }

        /// <summary>
        /// Cria uma instância do GatedWispsManager
        /// </summary>
        public static GatedWispsManager CreateGatedWispsManager()
        {
            return ServiceFactory.CreateManager<GatedWispsManager>((logger, eventDispatcher) =>
                new GatedWispsManager(logger, eventDispatcher));
        }

        /// <summary>
        /// Cria uma instância do PacketProcessor
        /// </summary>
        public static PacketProcessor CreatePacketProcessor()
        {
            return new PacketProcessor(
                ServiceFactory.CreateLogger<PacketProcessor>(),
                ServiceFactory.CreatePacketOffsets(),
                CreatePlayersManager(),
                CreateMobsManager(),
                CreateHarvestablesManager(),
                CreateLootChestsManager(),
                CreateDungeonsManager(),
                CreateFishNodesManager(),
                CreateGatedWispsManager(),
                ServiceFactory.CreatePositionDecryptor(),
                ServiceFactory.CreateEventDispatcher()
            );
        }
    }
} 