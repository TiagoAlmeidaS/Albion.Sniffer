using System;
using System.Collections.Generic;
using Albion.Network;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Models;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Models.Events;

namespace AlbionOnlineSniffer.Core.Services
{
    /// <summary>
    /// Gerencia os handlers do Albion.Network
    /// </summary>
    public class AlbionNetworkHandlerManager
    {
        private readonly ILogger<AlbionNetworkHandlerManager> _logger;

        public AlbionNetworkHandlerManager(ILogger<AlbionNetworkHandlerManager> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Configura o ReceiverBuilder com os handlers necessários
        /// </summary>
        /// <returns>ReceiverBuilder configurado</returns>
        public ReceiverBuilder ConfigureReceiverBuilder()
        {
            var builder = new ReceiverBuilder();
            
            // Registrar handlers para diferentes tipos de eventos
            builder.AddRequestHandler<NewCharacterEvent>(OnNewCharacter);
            builder.RegisterHandler<MoveEvent>(OnMove);
            builder.RegisterHandler<NewMobEvent>(OnNewMob);
            builder.RegisterHandler<NewHarvestableEvent>(OnNewHarvestable);
            builder.RegisterHandler<NewLootChestEvent>(OnNewLootChest);
            builder.RegisterHandler<NewDungeonEvent>(OnNewDungeon);
            builder.RegisterHandler<NewFishingZoneEvent>(OnNewFishingZone);
            builder.RegisterHandler<NewGatedWispEvent>(OnNewGatedWisp);
            builder.RegisterHandler<LeaveEvent>(OnLeave);
            builder.RegisterHandler<HealthUpdateEvent>(OnHealthUpdate);
            builder.RegisterHandler<MountedEvent>(OnMounted);
            builder.RegisterHandler<MobChangeStateEvent>(OnMobChangeState);
            builder.RegisterHandler<HarvestableChangeStateEvent>(OnHarvestableChangeState);
            builder.RegisterHandler<KeySyncEvent>(OnKeySync);
            builder.RegisterHandler<RegenerationChangedEvent>(OnRegenerationChanged);
            builder.RegisterHandler<MistsPlayerJoinedInfoEvent>(OnMistsPlayerJoinedInfo);
            builder.RegisterHandler<LoadClusterObjectsEvent>(OnLoadClusterObjects);
            builder.RegisterHandler<CharacterEquipmentChanged>(OnCharacterEquipmentChanged);
            builder.RegisterHandler<ChangeFlaggingFinishedEvent>(OnChangeFlaggingFinished);
            builder.RegisterHandler<WispGateOpenedEvent>(OnWispGateOpened);
            builder.RegisterHandler<NewHarvestablesListEvent>(OnNewHarvestablesList);
            builder.RegisterHandler<ChangeClusterEvent>(OnChangeCluster);

            _logger.LogInformation("ReceiverBuilder configurado com {HandlerCount} handlers", 22);
            return builder;
        }

        private void OnNewCharacter(NewCharacterEvent evt)
        {
            _logger.LogDebug("Handler: NewCharacter - ID: {Id}, Name: {Name}", evt.Id, evt.Name);
        }

        private void OnMove(MoveEvent evt)
        {
            _logger.LogDebug("Handler: Move - ID: {Id}", evt.Id);
        }

        private void OnNewMob(NewMobEvent evt)
        {
            _logger.LogDebug("Handler: NewMob - ID: {Id}, TypeId: {TypeId}", evt.Id, evt.TypeId);
        }

        private void OnNewHarvestable(NewHarvestableEvent evt)
        {
            _logger.LogDebug("Handler: NewHarvestable - ID: {Id}, Type: {Type}", evt.Id, evt.Type);
        }

        private void OnNewLootChest(NewLootChestEvent evt)
        {
            _logger.LogDebug("Handler: NewLootChest - ID: {Id}, Name: {Name}", evt.Id, evt.Name);
        }

        private void OnNewDungeon(NewDungeonEvent evt)
        {
            _logger.LogDebug("Handler: NewDungeon - ID: {Id}, Type: {Type}", evt.Id, evt.Type);
        }

        private void OnNewFishingZone(NewFishingZoneEvent evt)
        {
            _logger.LogDebug("Handler: NewFishingZone - ID: {Id}, Size: {Size}", evt.Id, evt.Size);
        }

        private void OnNewGatedWisp(NewGatedWispEvent evt)
        {
            _logger.LogDebug("Handler: NewGatedWisp - ID: {Id}, Collected: {IsCollected}", evt.Id, evt.isCollected);
        }

        private void OnLeave(LeaveEvent evt)
        {
            _logger.LogDebug("Handler: Leave - ID: {Id}", evt.Id);
        }

        private void OnHealthUpdate(HealthUpdateEvent evt)
        {
            _logger.LogDebug("Handler: HealthUpdate - ID: {Id}, Health: {Health}", evt.Id, evt.Health);
        }

        private void OnMounted(MountedEvent evt)
        {
            _logger.LogDebug("Handler: Mounted - ID: {Id}, IsMounted: {IsMounted}", evt.Id, evt.IsMounted);
        }

        private void OnMobChangeState(MobChangeStateEvent evt)
        {
            _logger.LogDebug("Handler: MobChangeState - ID: {Id}, Charge: {Charge}", evt.Id, evt.Charge);
        }

        private void OnHarvestableChangeState(HarvestableChangeStateEvent evt)
        {
            _logger.LogDebug("Handler: HarvestableChangeState - ID: {Id}, Count: {Count}, Charge: {Charge}", evt.Id, evt.Count, evt.Charge);
        }

        private void OnKeySync(KeySyncEvent evt)
        {
            _logger.LogDebug("Handler: KeySync - Code: {CodeLength}", evt.Code?.Length ?? 0);
        }

        private void OnRegenerationChanged(RegenerationChangedEvent evt)
        {
            _logger.LogDebug("Handler: RegenerationChanged - ID: {Id}", evt.Id);
        }

        private void OnMistsPlayerJoinedInfo(MistsPlayerJoinedInfoEvent evt)
        {
            _logger.LogDebug("Handler: MistsPlayerJoinedInfo - TimeCycle: {TimeCycle}", evt.TimeCycle);
        }

        private void OnLoadClusterObjects(LoadClusterObjectsEvent evt)
        {
            _logger.LogDebug("Handler: LoadClusterObjects - Objectives: {Count}", evt.ClusterObjectives?.Count ?? 0);
        }

        private void OnCharacterEquipmentChanged(CharacterEquipmentChanged evt)
        {
            _logger.LogDebug("Handler: CharacterEquipmentChanged - ID: {Id}", evt.Id);
        }

        private void OnChangeFlaggingFinished(ChangeFlaggingFinishedEvent evt)
        {
            _logger.LogDebug("Handler: ChangeFlaggingFinished - ID: {Id}, Faction: {Faction}", evt.Id, evt.Faction);
        }

        private void OnWispGateOpened(WispGateOpenedEvent evt)
        {
            _logger.LogDebug("Handler: WispGateOpened - ID: {Id}, Collected: {IsCollected}", evt.Id, evt.isCollected);
        }

        private void OnNewHarvestablesList(NewHarvestablesListEvent evt)
        {
            _logger.LogDebug("Handler: NewHarvestablesList - Count: {Count}", evt.HarvestableObjects.Count);
        }

        private void OnChangeCluster(ChangeClusterEvent evt)
        {
            _logger.LogDebug("Handler: ChangeCluster - LocationId: {LocationId}, Type: {Type}", evt.LocationId, evt.Type);
        }
    }
} 