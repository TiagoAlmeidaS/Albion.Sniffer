using Albion.Network;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Dungeons;
using AlbionOnlineSniffer.Core.Models.GameObjects.FishNodes;
using AlbionOnlineSniffer.Core.Models.GameObjects.GatedWisps;
using AlbionOnlineSniffer.Core.Models.GameObjects.Harvestables;
using AlbionOnlineSniffer.Core.Models.GameObjects.Localplayer;
using AlbionOnlineSniffer.Core.Models.GameObjects.LootChests;
using AlbionOnlineSniffer.Core.Models.GameObjects.Mobs;
using AlbionOnlineSniffer.Core.Models.GameObjects.Players;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Handlers
{
    class ChangeClusterEventHandler : ResponsePacketHandler<ChangeClusterEvent>
    {
        private readonly LocalPlayerHandler localPlayerHandler;
        private readonly PlayersHandler playersHandler;
        private readonly HarvestablesHandler harvestablesHandler;
        private readonly MobsHandler mobsHandler;
        private readonly DungeonsHandler dungeonsHandler;
        private readonly FishNodesHandler fishNodesHandler;
        private readonly GatedWispsHandler gatedWispsHandler;
        private readonly LootChestsHandler lootChestsHandler;
        private readonly EventDispatcher eventDispatcher;

        public ChangeClusterEventHandler(LocalPlayerHandler localPlayerHandler, PlayersHandler playersHandler, HarvestablesHandler harvestablesHandler, MobsHandler mobsHandler, DungeonsHandler dungeonsHandler, FishNodesHandler fishNodesHandler, GatedWispsHandler gatedWispsHandler, LootChestsHandler lootChestsHandler, EventDispatcher eventDispatcher) : base(PacketIndexesLoader.GlobalPacketIndexes?.ChangeCluster ?? 0)
        {
            this.localPlayerHandler = localPlayerHandler;
            this.playersHandler = playersHandler;
            this.harvestablesHandler = harvestablesHandler;
            this.mobsHandler = mobsHandler;
            this.dungeonsHandler = dungeonsHandler;
            this.fishNodesHandler = fishNodesHandler;
            this.gatedWispsHandler = gatedWispsHandler;
            this.lootChestsHandler = lootChestsHandler;
            this.eventDispatcher = eventDispatcher;
        }

        protected override async Task OnActionAsync(ChangeClusterEvent value)
        {
            localPlayerHandler.ChangeCluster(value.LocationId, value.DynamicClusterData);

            playersHandler.Clear();
            harvestablesHandler.Clear();
            mobsHandler.Clear();
            dungeonsHandler.Clear();
            fishNodesHandler.Clear();
            gatedWispsHandler.Clear();
            lootChestsHandler.Clear();

            // Emitir evento para o EventDispatcher
            await eventDispatcher.DispatchEvent(value);
        }
    }
}
