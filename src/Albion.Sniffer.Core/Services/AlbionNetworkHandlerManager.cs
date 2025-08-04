using System;
using System.Collections.Generic;
using Albion.Network;
using Albion.Sniffer.Core.Services;
using Albion.Sniffer.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Albion.Sniffer.Core.Models.Events;
using Albion.Sniffer.Core.Handlers;
using Albion.Sniffer.Core.Models.GameObjects.Players;
using Albion.Sniffer.Core.Models.GameObjects.Mobs;
using Albion.Sniffer.Core.Models.GameObjects.Dungeons;
using Albion.Sniffer.Core.Models.GameObjects.FishNodes;
using Albion.Sniffer.Core.Models.GameObjects.GatedWisps;
using Albion.Sniffer.Core.Models.GameObjects.Harvestables;
using Albion.Sniffer.Core.Models.GameObjects.Localplayer;
using Albion.Sniffer.Core.Models.GameObjects.LootChests;

namespace Albion.Sniffer.Core.Services
{
    /// <summary>
    /// Gerencia os handlers do Albion.Network
    /// </summary>
    public class AlbionNetworkHandlerManager
    {
        private readonly ILogger<AlbionNetworkHandlerManager> _logger;
        private readonly IServiceProvider _serviceProvider;

        public AlbionNetworkHandlerManager(ILogger<AlbionNetworkHandlerManager> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Configura o ReceiverBuilder com os handlers necessários
        /// </summary>
        /// <returns>ReceiverBuilder configurado</returns>
        public ReceiverBuilder ConfigureReceiverBuilder()
        {
            var builder = new ReceiverBuilder();
            
            // Registrar handlers usando a abordagem correta baseada nos handlers existentes
            builder.AddEventHandler(new LeaveEventHandler(
                _serviceProvider.GetRequiredService<PlayersHandler>(),
                _serviceProvider.GetRequiredService<MobsHandler>(),
                _serviceProvider.GetRequiredService<DungeonsHandler>(),
                _serviceProvider.GetRequiredService<FishNodesHandler>(),
                _serviceProvider.GetRequiredService<GatedWispsHandler>(),
                _serviceProvider.GetRequiredService<LootChestsHandler>(),
                _serviceProvider.GetRequiredService<EventDispatcher>()
            ));
            
            builder.AddResponseHandler(new ChangeClusterEventHandler(
                _serviceProvider.GetRequiredService<LocalPlayerHandler>(),
                _serviceProvider.GetRequiredService<PlayersHandler>(),
                _serviceProvider.GetRequiredService<HarvestablesHandler>(),
                _serviceProvider.GetRequiredService<MobsHandler>(),
                _serviceProvider.GetRequiredService<DungeonsHandler>(),
                _serviceProvider.GetRequiredService<FishNodesHandler>(),
                _serviceProvider.GetRequiredService<GatedWispsHandler>(),
                _serviceProvider.GetRequiredService<LootChestsHandler>(),
                _serviceProvider.GetRequiredService<EventDispatcher>()
            ));
            
            builder.AddResponseHandler(new JoinResponseOperationHandler(
                _serviceProvider.GetRequiredService<LocalPlayerHandler>(),
                _serviceProvider.GetRequiredService<PlayersHandler>(),
                _serviceProvider.GetRequiredService<HarvestablesHandler>(),
                _serviceProvider.GetRequiredService<MobsHandler>(),
                _serviceProvider.GetRequiredService<DungeonsHandler>(),
                _serviceProvider.GetRequiredService<FishNodesHandler>(),
                _serviceProvider.GetRequiredService<GatedWispsHandler>(),
                _serviceProvider.GetRequiredService<LootChestsHandler>(),
                _serviceProvider.GetRequiredService<EventDispatcher>()
            ));
            
            builder.AddRequestHandler(new MoveRequestOperationHandler(
                _serviceProvider.GetRequiredService<LocalPlayerHandler>(),
                _serviceProvider.GetRequiredService<HarvestablesHandler>(),
                _serviceProvider.GetRequiredService<EventDispatcher>()
            ));
            
            builder.AddEventHandler(new MistsPlayerJoinedInfoEventHandler(
                _serviceProvider.GetRequiredService<LocalPlayerHandler>(),
                _serviceProvider.GetRequiredService<EventDispatcher>()
            ));
            
            builder.AddEventHandler(new LoadClusterObjectsEventHandler(
                _serviceProvider.GetRequiredService<LocalPlayerHandler>(),
                _serviceProvider.GetRequiredService<ConfigHandler>(),
                _serviceProvider.GetRequiredService<EventDispatcher>()
            ));
            
            builder.AddEventHandler(new NewCharacterEventHandler(
                _serviceProvider.GetRequiredService<PlayersHandler>(),
                _serviceProvider.GetRequiredService<LocalPlayerHandler>(),
                _serviceProvider.GetRequiredService<ConfigHandler>(),
                _serviceProvider.GetRequiredService<EventDispatcher>()
            ));
            
            builder.AddEventHandler(new MountedEventHandler(
                _serviceProvider.GetRequiredService<PlayersHandler>(),
                _serviceProvider.GetRequiredService<EventDispatcher>()
            ));
            
            builder.AddEventHandler(new ChangeFlaggingFinishedEventHandler(
                _serviceProvider.GetRequiredService<LocalPlayerHandler>(),
                _serviceProvider.GetRequiredService<PlayersHandler>(),
                _serviceProvider.GetRequiredService<EventDispatcher>()
            ));
            
            builder.AddEventHandler(new CharacterEquipmentChangedEventHandler(
                _serviceProvider.GetRequiredService<PlayersHandler>(),
                _serviceProvider.GetRequiredService<EventDispatcher>()
            ));
            
            builder.AddEventHandler(new MoveEventHandler(
                _serviceProvider.GetRequiredService<PlayersHandler>(),
                _serviceProvider.GetRequiredService<MobsHandler>(),
                _serviceProvider.GetRequiredService<EventDispatcher>()
            ));
            
            builder.AddEventHandler(new HealthUpdateEventHandler(
                _serviceProvider.GetRequiredService<PlayersHandler>(),
                _serviceProvider.GetRequiredService<MobsHandler>(),
                _serviceProvider.GetRequiredService<EventDispatcher>()
            ));
            
            builder.AddEventHandler(new RegenerationChangedEventHandler(
                _serviceProvider.GetRequiredService<PlayersHandler>(),
                _serviceProvider.GetRequiredService<EventDispatcher>()
            ));
            
            builder.AddEventHandler(new NewHarvestableEventHandler(
                _serviceProvider.GetRequiredService<HarvestablesHandler>(),
                _serviceProvider.GetRequiredService<EventDispatcher>()
            ));
            
            builder.AddEventHandler(new NewHarvestablesListEventHandler(
                _serviceProvider.GetRequiredService<HarvestablesHandler>(),
                _serviceProvider.GetRequiredService<EventDispatcher>()
            ));
            
            builder.AddEventHandler(new NewMobEventHandler(
                _serviceProvider.GetRequiredService<MobsHandler>(),
                _serviceProvider.GetRequiredService<EventDispatcher>()
            ));
            
            builder.AddEventHandler(new MobChangeStateEventHandler(
                _serviceProvider.GetRequiredService<MobsHandler>(),
                _serviceProvider.GetRequiredService<EventDispatcher>()
            ));
            
            builder.AddEventHandler(new HarvestableChangeStateEventHandler(
                _serviceProvider.GetRequiredService<HarvestablesHandler>(),
                _serviceProvider.GetRequiredService<EventDispatcher>()
            ));
            
            builder.AddEventHandler(new KeySyncEventHandler(
                _serviceProvider.GetRequiredService<PlayersHandler>(),
                _serviceProvider.GetRequiredService<EventDispatcher>()
            ));
            
            builder.AddEventHandler(new NewDungeonEventHandler(
                _serviceProvider.GetRequiredService<DungeonsHandler>(),
                _serviceProvider.GetRequiredService<EventDispatcher>()
            ));
            
            builder.AddEventHandler(new NewFishingZoneEventHandler(
                _serviceProvider.GetRequiredService<FishNodesHandler>(),
                _serviceProvider.GetRequiredService<EventDispatcher>()
            ));
            
            builder.AddEventHandler(new NewGatedWispEventHandler(
                _serviceProvider.GetRequiredService<GatedWispsHandler>(),
                _serviceProvider.GetRequiredService<EventDispatcher>()
            ));
            
            builder.AddEventHandler(new NewLootChestEventHandler(
                _serviceProvider.GetRequiredService<LootChestsHandler>(),
                _serviceProvider.GetRequiredService<EventDispatcher>()
            ));
            
            builder.AddEventHandler(new WispGateOpenedEventHandler(
                _serviceProvider.GetRequiredService<GatedWispsHandler>(),
                _serviceProvider.GetRequiredService<EventDispatcher>()
            ));

            _logger.LogInformation("ReceiverBuilder configurado com {HandlerCount} handlers", 25);
            return builder;
        }
    }
} 