using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Handlers;
using AlbionOnlineSniffer.Core.Models;

namespace AlbionOnlineSniffer.Core
{
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
            services.AddSingleton<PhotonPacketParser>();
            services.AddSingleton<Protocol16Deserializer>();

            // Serviços de decriptação e offsets
            services.AddSingleton<PositionDecryptor>();
            services.AddSingleton<PacketOffsets>();

            // Sistema de eventos
            services.AddSingleton<EventDispatcher>();

            // Handlers específicos
            services.AddSingleton<NewCharacterEventHandler>();
            services.AddSingleton<MoveEventHandler>();
            services.AddSingleton<NewMobEventHandler>();
            services.AddSingleton<NewHarvestableEventHandler>();
            services.AddSingleton<NewLootChestEventHandler>();

            // Managers
            services.AddSingleton<PlayersManager>();
            services.AddSingleton<MobsManager>();
            services.AddSingleton<HarvestablesManager>();
            services.AddSingleton<LootChestsManager>();

            // Processador de pacotes
            services.AddSingleton<PacketProcessor>();

            // Managers (comentados até serem implementados)
            // services.AddSingleton<DungeonsManager>();
            // services.AddSingleton<FishNodesManager>();
            // services.AddSingleton<GatedWispsManager>();
            // services.AddSingleton<LocalPlayerHandler>();

            // New services
            services.AddSingleton<Services.XorDecryptor>();
            services.AddSingleton<Services.ClusterService>();
            services.AddSingleton<Services.ItemDataService>();

            // New handlers
            services.AddSingleton<KeySyncEventHandler>();
            services.AddSingleton<RegenerationChangedEventHandler>();
            services.AddSingleton<MistsPlayerJoinedInfoEventHandler>();
            services.AddSingleton<LoadClusterObjectsEventHandler>();
            services.AddSingleton<MountedEventHandler>();
            services.AddSingleton<CharacterEquipmentChangedEventHandler>();
            
            // Additional handlers for 100% compatibility
            services.AddSingleton<ChangeFlaggingFinishedEventHandler>();
            services.AddSingleton<WispGateOpenedEventHandler>();
            services.AddSingleton<NewFishingZoneEventHandler>();
            services.AddSingleton<NewDungeonExitEventHandler>();
            services.AddSingleton<HarvestableChangeStateEventHandler>();
            services.AddSingleton<MobChangeStateEventHandler>();
            services.AddSingleton<HealthUpdateEventHandler>();
            
            // Final handlers for 100% compatibility
            services.AddSingleton<NewHarvestablesListEventHandler>();
            services.AddSingleton<MoveRequestEventHandler>();
            services.AddSingleton<JoinResponseEventHandler>();
        }

        /// <summary>
        /// Factory method para criar PhotonPacketParser
        /// </summary>
        public static PhotonPacketParser CreatePhotonPacketParser(
            PhotonPacketEnricher packetEnricher, 
            ILogger<PhotonPacketParser> logger)
        {
            return new PhotonPacketParser(packetEnricher, logger);
        }

        /// <summary>
        /// Factory method para criar PositionDecryptor
        /// </summary>
        public static PositionDecryptor CreatePositionDecryptor(ILogger<PositionDecryptor> logger)
        {
            return new PositionDecryptor(logger);
        }

        /// <summary>
        /// Factory method para criar EventDispatcher
        /// </summary>
        public static EventDispatcher CreateEventDispatcher(ILogger<EventDispatcher> logger)
        {
            return new EventDispatcher(logger);
        }

        /// <summary>
        /// Factory method para criar PlayersManager
        /// </summary>
        public static PlayersManager CreatePlayersManager(
            ILogger<PlayersManager> logger, 
            PositionDecryptor positionDecryptor,
            EventDispatcher eventDispatcher)
        {
            return new PlayersManager(logger, positionDecryptor, eventDispatcher);
        }

        /// <summary>
        /// Factory method para criar MobsManager
        /// </summary>
        public static MobsManager CreateMobsManager(
            ILogger<MobsManager> logger, 
            PositionDecryptor positionDecryptor,
            EventDispatcher eventDispatcher)
        {
            return new MobsManager(logger, positionDecryptor, eventDispatcher);
        }

        /// <summary>
        /// Factory method para criar HarvestablesManager
        /// </summary>
        public static HarvestablesManager CreateHarvestablesManager(
            ILogger<HarvestablesManager> logger, 
            PositionDecryptor positionDecryptor,
            EventDispatcher eventDispatcher)
        {
            return new HarvestablesManager(logger, positionDecryptor, eventDispatcher);
        }

        /// <summary>
        /// Factory method para criar LootChestsManager
        /// </summary>
        public static LootChestsManager CreateLootChestsManager(
            ILogger<LootChestsManager> logger, 
            PositionDecryptor positionDecryptor,
            EventDispatcher eventDispatcher)
        {
            return new LootChestsManager(logger, positionDecryptor, eventDispatcher);
        }

                /// <summary>
        /// Factory method para criar PacketProcessor
        /// </summary>
        public static PacketProcessor CreatePacketProcessor(
            ILogger<PacketProcessor> logger,
            PacketOffsets packetOffsets,
            PlayersManager playersManager,
            MobsManager mobsManager,
            HarvestablesManager harvestablesManager,
            LootChestsManager lootChestsManager,
            PositionDecryptor positionDecryptor,
            EventDispatcher eventDispatcher)
        {
            return new PacketProcessor(logger, packetOffsets, playersManager, mobsManager, 
                harvestablesManager, lootChestsManager, positionDecryptor, eventDispatcher);
        }

        /// <summary>
        /// Factory method para criar PhotonDefinitionLoader
        /// </summary>
        public static PhotonDefinitionLoader CreatePhotonDefinitionLoader(ILogger<PhotonDefinitionLoader> logger)
        {
            return new PhotonDefinitionLoader(logger);
        }

        /// <summary>
        /// Factory method para criar PhotonPacketEnricher
        /// </summary>
        public static PhotonPacketEnricher CreatePhotonPacketEnricher(
            PhotonDefinitionLoader definitionLoader, 
            ILogger<PhotonPacketEnricher> logger)
        {
            return new PhotonPacketEnricher(definitionLoader, logger);
        }

        /// <summary>
        /// Factory method para criar Protocol16Deserializer
        /// </summary>
        public static Protocol16Deserializer CreateProtocol16Deserializer(
            PhotonPacketEnricher packetEnricher,
            PacketProcessor packetProcessor,
            ILogger<Protocol16Deserializer> logger,
            IEnumerable<object>? handlers = null)
        {
            return new Protocol16Deserializer(packetEnricher, packetProcessor, logger, handlers);
        }

        /// <summary>
        /// Factory method para criar XorDecryptor
        /// </summary>
        public static XorDecryptor CreateXorDecryptor(ILogger<XorDecryptor> logger)
        {
            return new XorDecryptor(logger);
        }

        /// <summary>
        /// Factory method para criar ClusterService
        /// </summary>
        public static ClusterService CreateClusterService(ILogger<ClusterService> logger)
        {
            return new ClusterService(logger);
        }

        /// <summary>
        /// Factory method para criar ItemDataService
        /// </summary>
        public static ItemDataService CreateItemDataService(ILogger<ItemDataService> logger)
        {
            return new ItemDataService(logger);
        }
    }
} 