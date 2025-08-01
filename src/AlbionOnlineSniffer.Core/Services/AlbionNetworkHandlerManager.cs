using System;
using System.Collections.Generic;
using Albion.Network;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Handlers.AlbionNetworkHandlers;
using AlbionOnlineSniffer.Core.Models;
using Microsoft.Extensions.Logging;

namespace AlbionOnlineSniffer.Core.Services
{
    /// <summary>
    /// Gerenciador de handlers do Albion.Network
    /// Responsável por criar e configurar todos os handlers do ReceiverBuilder
    /// Baseado no padrão do albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkHandlerManager
    {
        private readonly EventDispatcher _eventDispatcher;
        private readonly ILogger<AlbionNetworkHandlerManager> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly PacketIndexes _packetIndexes;
        private readonly PacketOffsets _packetOffsets;

        public AlbionNetworkHandlerManager(EventDispatcher eventDispatcher, ILoggerFactory loggerFactory, PacketIndexes packetIndexes, PacketOffsets packetOffsets)
        {
            _eventDispatcher = eventDispatcher;
            _loggerFactory = loggerFactory;
            _packetIndexes = packetIndexes;
            _packetOffsets = packetOffsets;
            _logger = loggerFactory.CreateLogger<AlbionNetworkHandlerManager>();
        }

        /// <summary>
        /// Configura e retorna o ReceiverBuilder com todos os handlers registrados
        /// </summary>
        /// <returns>ReceiverBuilder configurado com todos os handlers</returns>
        public ReceiverBuilder ConfigureReceiverBuilder()
        {
            try
            {
                _logger.LogInformation("🔧 Configurando ReceiverBuilder com handlers do Albion.Network...");

                var builder = ReceiverBuilder.Create();

                // Registrar todos os handlers do Albion.Network
                RegisterEventHandlers(builder);
                RegisterRequestHandlers(builder);
                RegisterResponseHandlers(builder);

                _logger.LogInformation("✅ ReceiverBuilder configurado com sucesso");
                return builder;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao configurar ReceiverBuilder: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Registra handlers de eventos (EventPacketHandler)
        /// </summary>
        private void RegisterEventHandlers(ReceiverBuilder builder)
        {
            _logger.LogDebug("📝 Registrando EventHandlers...");

            // NewCharacter
            var newCharacterHandler = new NewCharacterEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<NewCharacterEventHandler>(), _packetIndexes);
            builder.AddEventHandler(newCharacterHandler);

            // Move
            var moveHandler = new MoveEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<MoveEventHandler>(), _packetIndexes);
            builder.AddEventHandler(moveHandler);

            // NewMob
            var newMobHandler = new NewMobEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<NewMobEventHandler>(), _packetIndexes);
            builder.AddEventHandler(newMobHandler);

            // NewHarvestable
            var newHarvestableHandler = new NewHarvestableEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<NewHarvestableEventHandler>(), _packetIndexes);
            builder.AddEventHandler(newHarvestableHandler);

            // HealthUpdate
            var healthUpdateHandler = new HealthUpdateEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<HealthUpdateEventHandler>(), _packetIndexes);
            builder.AddEventHandler(healthUpdateHandler);

            // NewLootChest
            var newLootChestHandler = new NewLootChestEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<NewLootChestEventHandler>(), _packetIndexes);
            builder.AddEventHandler(newLootChestHandler);

            // Mounted
            var mountedHandler = new MountedEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<MountedEventHandler>(), _packetIndexes);
            builder.AddEventHandler(mountedHandler);

            // CharacterEquipmentChanged
            var characterEquipmentChangedHandler = new CharacterEquipmentChangedEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<CharacterEquipmentChangedEventHandler>(), _packetIndexes);
            builder.AddEventHandler(characterEquipmentChangedHandler);

            // RegenerationChanged
            var regenerationChangedHandler = new RegenerationChangedEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<RegenerationChangedEventHandler>(), _packetIndexes);
            builder.AddEventHandler(regenerationChangedHandler);

            // NewHarvestablesList
            var newHarvestablesListHandler = new NewHarvestablesListEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<NewHarvestablesListEventHandler>(), _packetIndexes);
            builder.AddEventHandler(newHarvestablesListHandler);

            // HarvestableChangeState
            var harvestableChangeStateHandler = new HarvestableChangeStateEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<HarvestableChangeStateEventHandler>(), _packetIndexes);
            builder.AddEventHandler(harvestableChangeStateHandler);

            // MobChangeState
            var mobChangeStateHandler = new MobChangeStateEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<MobChangeStateEventHandler>(), _packetIndexes);
            builder.AddEventHandler(mobChangeStateHandler);

            // NewFishingZone
            var newFishingZoneHandler = new NewFishingZoneEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<NewFishingZoneEventHandler>(), _packetIndexes);
            builder.AddEventHandler(newFishingZoneHandler);

            // NewDungeon
            var newDungeonHandler = new NewDungeonEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<NewDungeonEventHandler>(), _packetIndexes);
            builder.AddEventHandler(newDungeonHandler);

            // NewGatedWisp
            var newGatedWispHandler = new NewGatedWispEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<NewGatedWispEventHandler>(), _packetIndexes);
            builder.AddEventHandler(newGatedWispHandler);

            // WispGateOpened
            var wispGateOpenedHandler = new WispGateOpenedEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<WispGateOpenedEventHandler>(), _packetIndexes);
            builder.AddEventHandler(wispGateOpenedHandler);

            // KeySync
            var keySyncHandler = new KeySyncEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<KeySyncEventHandler>(), _packetIndexes);
            builder.AddEventHandler(keySyncHandler);

            // MistsPlayerJoinedInfo
            var mistsPlayerJoinedInfoHandler = new MistsPlayerJoinedInfoEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<MistsPlayerJoinedInfoEventHandler>(), _packetIndexes);
            builder.AddEventHandler(mistsPlayerJoinedInfoHandler);

            // LoadClusterObjects
            var loadClusterObjectsHandler = new LoadClusterObjectsEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<LoadClusterObjectsEventHandler>(), _packetIndexes);
            builder.AddEventHandler(loadClusterObjectsHandler);

            // ChangeFlaggingFinished
            var changeFlaggingFinishedHandler = new ChangeFlaggingFinishedEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<ChangeFlaggingFinishedEventHandler>(), _packetIndexes);
            builder.AddEventHandler(changeFlaggingFinishedHandler);

            // NewDungeonExit
            var newDungeonExitHandler = new NewDungeonExitEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<NewDungeonExitEventHandler>(), _packetIndexes);
            builder.AddEventHandler(newDungeonExitHandler);

            // NewFishingZoneObject
            var newFishingZoneObjectHandler = new NewFishingZoneObjectEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<NewFishingZoneObjectEventHandler>(), _packetIndexes);
            builder.AddEventHandler(newFishingZoneObjectHandler);

            // Leave
            var leaveHandler = new LeaveEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<LeaveEventHandler>(), _packetIndexes);
            builder.AddEventHandler(leaveHandler);

            // TODO: Adicionar mais handlers conforme implementados
            // builder.AddEventHandler(new WispGateOpenedEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<WispGateOpenedEventHandler>()));
            // builder.AddEventHandler(new KeySyncEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<KeySyncEventHandler>()));
            // builder.AddEventHandler(new MistsPlayerJoinedInfoEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<MistsPlayerJoinedInfoEventHandler>()));
            // builder.AddEventHandler(new LoadClusterObjectsEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<LoadClusterObjectsEventHandler>()));
            // builder.AddEventHandler(new ChangeFlaggingFinishedEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<ChangeFlaggingFinishedEventHandler>()));
            // builder.AddEventHandler(new NewHarvestablesListEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<NewHarvestablesListEventHandler>()));
            // builder.AddEventHandler(new HarvestableChangeStateEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<HarvestableChangeStateEventHandler>()));
            // builder.AddEventHandler(new MobChangeStateEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<MobChangeStateEventHandler>()));
            // builder.AddEventHandler(new NewFishingZoneEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<NewFishingZoneEventHandler>()));
            // builder.AddEventHandler(new NewDungeonEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<NewDungeonEventHandler>()));
            // builder.AddEventHandler(new NewGatedWispEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<NewGatedWispEventHandler>()));
            // builder.AddEventHandler(new WispGateOpenedEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<WispGateOpenedEventHandler>()));
            // builder.AddEventHandler(new KeySyncEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<KeySyncEventHandler>()));
            // builder.AddEventHandler(new MistsPlayerJoinedInfoEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<MistsPlayerJoinedInfoEventHandler>()));
            // builder.AddEventHandler(new LoadClusterObjectsEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<LoadClusterObjectsEventHandler>()));
            // builder.AddEventHandler(new ChangeFlaggingFinishedEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<ChangeFlaggingFinishedEventHandler>()));

            _logger.LogDebug("✅ EventHandlers registrados");
        }

        /// <summary>
        /// Registra handlers de requisições (RequestPacketHandler)
        /// </summary>
        private void RegisterRequestHandlers(ReceiverBuilder builder)
        {
            _logger.LogDebug("📝 Registrando RequestHandlers...");

            // MoveRequest
            var moveRequestHandler = new MoveRequestOperationHandler(_eventDispatcher, _loggerFactory.CreateLogger<MoveRequestOperationHandler>(), _packetIndexes);
            builder.AddRequestHandler(moveRequestHandler);

            // JoinRequest
            var joinRequestHandler = new JoinRequestOperationHandler(_eventDispatcher, _loggerFactory.CreateLogger<JoinRequestOperationHandler>(), _packetIndexes);
            builder.AddRequestHandler(joinRequestHandler);

            // TODO: Adicionar mais handlers de requisição conforme implementados
            // builder.AddRequestHandler(new LeaveRequestOperationHandler(_eventDispatcher, _loggerFactory.CreateLogger<LeaveRequestOperationHandler>()));

            _logger.LogDebug("✅ RequestHandlers registrados");
        }

        /// <summary>
        /// Registra handlers de respostas (ResponsePacketHandler)
        /// </summary>
        private void RegisterResponseHandlers(ReceiverBuilder builder)
        {
            _logger.LogDebug("📝 Registrando ResponseHandlers...");

            // ChangeCluster
            var changeClusterHandler = new ChangeClusterEventHandler(_eventDispatcher, _loggerFactory.CreateLogger<ChangeClusterEventHandler>(), _packetIndexes);
            builder.AddResponseHandler(changeClusterHandler);

            // JoinResponse
            var joinResponseHandler = new JoinResponseOperationHandler(_eventDispatcher, _loggerFactory.CreateLogger<JoinResponseOperationHandler>(), _packetIndexes);
            builder.AddResponseHandler(joinResponseHandler);

            // LeaveResponse
            var leaveResponseHandler = new LeaveResponseOperationHandler(_eventDispatcher, _loggerFactory.CreateLogger<LeaveResponseOperationHandler>(), _packetIndexes);
            builder.AddResponseHandler(leaveResponseHandler);

            // TODO: Adicionar mais handlers de resposta conforme implementados
            // builder.AddResponseHandler(new LeaveRequestOperationHandler(_eventDispatcher, _loggerFactory.CreateLogger<LeaveRequestOperationHandler>()));

            _logger.LogDebug("✅ ResponseHandlers registrados");
        }
    }
} 