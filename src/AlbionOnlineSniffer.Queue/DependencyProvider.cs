using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using AlbionOnlineSniffer.Core.Interfaces;
using AlbionOnlineSniffer.Queue.Interfaces;
using AlbionOnlineSniffer.Queue.Publishers;
using AlbionOnlineSniffer.Queue.Adapters;
using AlbionOnlineSniffer.Core.Services;
using Microsoft.Extensions.Logging;

namespace AlbionOnlineSniffer.Queue
{
    public static class DependencyProvider
    {


        /// <summary>
        /// Registra os serviços de fila lendo configurações
        /// RabbitMQ:ConnectionString, RabbitMQ:Exchange ou ConnectionStrings:RabbitMQ
        /// </summary>
        public static void AddQueueServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IQueuePublisher>(provider =>
            {
                var connectionString = configuration.GetConnectionString("RabbitMQ")
                    ?? configuration.GetValue<string>("RabbitMQ:ConnectionString")
                    ?? configuration.GetValue<string>("Queue:ConnectionString")
                    ?? "amqp://localhost";
                var exchange = configuration.GetValue<string>("RabbitMQ:Exchange")
                    ?? configuration.GetValue<string>("Queue:ExchangeName")
                    ?? "albion.sniffer";
                return new RabbitMqPublisher(connectionString, exchange);
            });

            // ✅ REGISTRAR ADAPTADOR PARA IEventPublisher DO CORE (SOBRESCREVE O STUB)
            services.AddSingleton<IEventPublisher>(provider =>
            {
                var queuePublisher = provider.GetRequiredService<IQueuePublisher>();
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                return new EventPublisherAdapter(queuePublisher, loggerFactory.CreateLogger<EventPublisherAdapter>());
            });

            services.AddSingleton<EventToQueueBridge>(sp =>
            {
                var dispatcher = sp.GetRequiredService<AlbionOnlineSniffer.Core.Services.EventDispatcher>();
                var publisher = sp.GetRequiredService<IQueuePublisher>();
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                return new EventToQueueBridge(dispatcher, publisher, loggerFactory.CreateLogger<EventToQueueBridge>());
            });

            // V1 Contracts pipeline: router + transformers + bridge
            services.AddSingleton<AlbionOnlineSniffer.Core.Contracts.EventContractRouter>();
            services.AddSingleton<AlbionOnlineSniffer.Core.Contracts.IEventContractTransformer, AlbionOnlineSniffer.Core.Contracts.Transformers.NewCharacterToPlayerSpottedV1>();
            services.AddSingleton<AlbionOnlineSniffer.Core.Contracts.IEventContractTransformer, AlbionOnlineSniffer.Core.Contracts.Transformers.MoveEventToPlayerSpottedV1>();
            services.AddSingleton<AlbionOnlineSniffer.Core.Contracts.IEventContractTransformer, AlbionOnlineSniffer.Core.Contracts.Transformers.NewMobToMobSpawnedV1>();
            services.AddSingleton<AlbionOnlineSniffer.Core.Contracts.IEventContractTransformer, AlbionOnlineSniffer.Core.Contracts.Transformers.ChangeClusterToClusterChangedV1>();
            services.AddSingleton<AlbionOnlineSniffer.Core.Contracts.IEventContractTransformer, AlbionOnlineSniffer.Core.Contracts.Transformers.ChangeFlaggingFinishedToFlaggingFinishedV1>();
            services.AddSingleton<AlbionOnlineSniffer.Core.Contracts.IEventContractTransformer, AlbionOnlineSniffer.Core.Contracts.Transformers.CharacterEquipmentChangedToEquipmentChangedV1>();
            services.AddSingleton<AlbionOnlineSniffer.Core.Contracts.IEventContractTransformer, AlbionOnlineSniffer.Core.Contracts.Transformers.HarvestableChangeStateToHarvestableStateChangedV1>();
            services.AddSingleton<AlbionOnlineSniffer.Core.Contracts.IEventContractTransformer, AlbionOnlineSniffer.Core.Contracts.Transformers.HealthUpdateToHealthUpdatedV1>();
            services.AddSingleton<AlbionOnlineSniffer.Core.Contracts.IEventContractTransformer, AlbionOnlineSniffer.Core.Contracts.Transformers.KeySyncToKeySyncV1>();
            services.AddSingleton<AlbionOnlineSniffer.Core.Contracts.IEventContractTransformer, AlbionOnlineSniffer.Core.Contracts.Transformers.LeaveToEntityLeftV1>();
            services.AddSingleton<AlbionOnlineSniffer.Core.Contracts.IEventContractTransformer, AlbionOnlineSniffer.Core.Contracts.Transformers.LoadClusterObjectsToClusterObjectsLoadedV1>();
            services.AddSingleton<AlbionOnlineSniffer.Core.Contracts.IEventContractTransformer, AlbionOnlineSniffer.Core.Contracts.Transformers.MistsPlayerJoinedInfoToMistsPlayerJoinedV1>();
            services.AddSingleton<AlbionOnlineSniffer.Core.Contracts.IEventContractTransformer, AlbionOnlineSniffer.Core.Contracts.Transformers.MobChangeStateToMobStateChangedV1>();
            services.AddSingleton<AlbionOnlineSniffer.Core.Contracts.IEventContractTransformer, AlbionOnlineSniffer.Core.Contracts.Transformers.MountedToMountedStateChangedV1>();
            services.AddSingleton<AlbionOnlineSniffer.Core.Contracts.IEventContractTransformer, AlbionOnlineSniffer.Core.Contracts.Transformers.NewDungeonToDungeonFoundV1>();
            services.AddSingleton<AlbionOnlineSniffer.Core.Contracts.IEventContractTransformer, AlbionOnlineSniffer.Core.Contracts.Transformers.NewFishingZoneToFishingZoneFoundV1>();
            services.AddSingleton<AlbionOnlineSniffer.Core.Contracts.IEventContractTransformer, AlbionOnlineSniffer.Core.Contracts.Transformers.NewGatedWispToGatedWispFoundV1>();
            services.AddSingleton<AlbionOnlineSniffer.Core.Contracts.IEventContractTransformer, AlbionOnlineSniffer.Core.Contracts.Transformers.NewHarvestableToHarvestableFoundV1>();
            services.AddSingleton<AlbionOnlineSniffer.Core.Contracts.IEventContractTransformer, AlbionOnlineSniffer.Core.Contracts.Transformers.NewHarvestablesListToHarvestablesListFoundV1>();
            services.AddSingleton<AlbionOnlineSniffer.Core.Contracts.IEventContractTransformer, AlbionOnlineSniffer.Core.Contracts.Transformers.NewLootChestToLootChestFoundV1>();
            services.AddSingleton<AlbionOnlineSniffer.Core.Contracts.IEventContractTransformer, AlbionOnlineSniffer.Core.Contracts.Transformers.RegenerationChangedToRegenerationChangedV1>();
            services.AddSingleton<AlbionOnlineSniffer.Core.Contracts.IEventContractTransformer, AlbionOnlineSniffer.Core.Contracts.Transformers.WispGateOpenedToWispGateOpenedV1>();
            services.AddSingleton<V1ContractPublisherBridge>(sp =>
            {
                var dispatcher = sp.GetRequiredService<AlbionOnlineSniffer.Core.Services.EventDispatcher>();
                var router = sp.GetRequiredService<AlbionOnlineSniffer.Core.Contracts.EventContractRouter>();
                var publisher = sp.GetRequiredService<IQueuePublisher>();
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                return new V1ContractPublisherBridge(dispatcher, router, publisher, loggerFactory.CreateLogger<V1ContractPublisherBridge>());
            });
        }

        /// <summary>
        /// Factory para criar RabbitMqPublisher (método legado)
        /// </summary>
        public static IQueuePublisher CreateRabbitMqPublisher(string connectionString, string exchange)
        {
            return new RabbitMqPublisher(connectionString, exchange);
        }

        /// <summary>
        /// Factory para criar RedisPublisher (método legado)
        /// </summary>
        public static IQueuePublisher CreateRedisPublisher(string connectionString)
            => new RedisPublisher(connectionString);
    }
} 