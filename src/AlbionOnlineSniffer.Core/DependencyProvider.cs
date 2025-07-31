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
            services.AddSingleton<Protocol16Deserializer>();

            // Serviços de decriptação e offsets
            services.AddSingleton<PositionDecryptor>();
            services.AddSingleton<PacketOffsetsLoader>();
            services.AddSingleton<PacketOffsets>();

            // Sistema de eventos
            services.AddSingleton<EventDispatcher>();

            // Handlers específicos
            services.AddSingleton<NewCharacterEventHandler>();
            services.AddSingleton<MoveEventHandler>();
            services.AddSingleton<NewMobEventHandler>();
            services.AddSingleton<NewHarvestableEventHandler>();
            services.AddSingleton<NewLootChestEventHandler>();
            
            // Novos handlers para parsing real
            services.AddSingleton<NewDungeonExitEventHandler>();
            services.AddSingleton<NewFishingZoneObjectEventHandler>();
            services.AddSingleton<NewWispGateEventHandler>();
            services.AddSingleton<WispGateOpenedEventHandler>();

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

        /// <summary>
        /// Cria uma instância do NewCharacterEventHandler
        /// </summary>
        public static NewCharacterEventHandler CreateNewCharacterEventHandler()
        {
            return ServiceFactory.CreateHandler<NewCharacterEventHandler>((logger, positionDecryptor, packetOffsets) =>
                new NewCharacterEventHandler(logger, positionDecryptor, packetOffsets));
        }

        /// <summary>
        /// Cria uma instância do MoveEventHandler
        /// </summary>
        public static MoveEventHandler CreateMoveEventHandler()
        {
            return ServiceFactory.CreateHandler<MoveEventHandler>((logger, positionDecryptor, packetOffsets) =>
                new MoveEventHandler(logger, positionDecryptor, packetOffsets));
        }

        /// <summary>
        /// Cria uma instância do NewMobEventHandler
        /// </summary>
        public static NewMobEventHandler CreateNewMobEventHandler()
        {
            return ServiceFactory.CreateHandler<NewMobEventHandler>((logger, positionDecryptor, packetOffsets) =>
                new NewMobEventHandler(logger, positionDecryptor, packetOffsets));
        }

        /// <summary>
        /// Cria uma instância do NewHarvestableEventHandler
        /// </summary>
        public static NewHarvestableEventHandler CreateNewHarvestableEventHandler()
        {
            return ServiceFactory.CreateHandler<NewHarvestableEventHandler>((logger, positionDecryptor, packetOffsets) =>
                new NewHarvestableEventHandler(logger, positionDecryptor, packetOffsets));
        }

        /// <summary>
        /// Cria uma instância do NewLootChestEventHandler
        /// </summary>
        public static NewLootChestEventHandler CreateNewLootChestEventHandler()
        {
            return ServiceFactory.CreateHandler<NewLootChestEventHandler>((logger, positionDecryptor, packetOffsets) =>
                new NewLootChestEventHandler(logger, positionDecryptor, packetOffsets));
        }

        /// <summary>
        /// Cria uma instância do NewDungeonExitEventHandler
        /// </summary>
        public static NewDungeonExitEventHandler CreateNewDungeonExitEventHandler()
        {
            return ServiceFactory.CreateHandler<NewDungeonExitEventHandler>((logger, positionDecryptor, packetOffsets) =>
                new NewDungeonExitEventHandler(logger, positionDecryptor, packetOffsets));
        }

        /// <summary>
        /// Cria uma instância do NewFishingZoneObjectEventHandler
        /// </summary>
        public static NewFishingZoneObjectEventHandler CreateNewFishingZoneObjectEventHandler()
        {
            return ServiceFactory.CreateHandler<NewFishingZoneObjectEventHandler>((logger, positionDecryptor, packetOffsets) =>
                new NewFishingZoneObjectEventHandler(logger, positionDecryptor, packetOffsets));
        }

        /// <summary>
        /// Cria uma instância do NewWispGateEventHandler
        /// </summary>
        public static NewWispGateEventHandler CreateNewWispGateEventHandler()
        {
            return ServiceFactory.CreateHandler<NewWispGateEventHandler>((logger, positionDecryptor, packetOffsets) =>
                new NewWispGateEventHandler(logger, positionDecryptor, packetOffsets));
        }

        /// <summary>
        /// Cria uma instância do WispGateOpenedEventHandler
        /// </summary>
        public static WispGateOpenedEventHandler CreateWispGateOpenedEventHandler()
        {
            return new WispGateOpenedEventHandler(ServiceFactory.CreateLogger<WispGateOpenedEventHandler>(), ServiceFactory.CreatePacketOffsets());
        }

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
                new DungeonsManager(logger, eventDispatcher, CreateNewDungeonExitEventHandler()));
        }

        /// <summary>
        /// Cria uma instância do FishNodesManager
        /// </summary>
        public static FishNodesManager CreateFishNodesManager()
        {
            return ServiceFactory.CreateManager<FishNodesManager>((logger, eventDispatcher) =>
                new FishNodesManager(logger, eventDispatcher, CreateNewFishingZoneObjectEventHandler()));
        }

        /// <summary>
        /// Cria uma instância do GatedWispsManager
        /// </summary>
        public static GatedWispsManager CreateGatedWispsManager()
        {
            return ServiceFactory.CreateManager<GatedWispsManager>((logger, eventDispatcher) =>
                new GatedWispsManager(logger, eventDispatcher, CreateNewWispGateEventHandler(), CreateWispGateOpenedEventHandler()));
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