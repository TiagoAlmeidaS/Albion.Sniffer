using Albion.Network;
using Albion.Sniffer.Core.Models.Events;
using Albion.Sniffer.Core.Models.GameObjects.Dungeons;
using Albion.Sniffer.Core.Models.GameObjects.FishNodes;
using Albion.Sniffer.Core.Models.GameObjects.GatedWisps;
using Albion.Sniffer.Core.Models.GameObjects.LootChests;
using Albion.Sniffer.Core.Models.GameObjects.Mobs;
using Albion.Sniffer.Core.Models.GameObjects.Players;
using Albion.Sniffer.Core.Services;

namespace Albion.Sniffer.Core.Handlers
{
    public class LeaveEventHandler : EventPacketHandler<LeaveEvent>
    {
        private readonly PlayersHandler playersHandler;
        private readonly MobsHandler mobsHandler;
        private readonly DungeonsHandler dungeonsHandler;
        private readonly FishNodesHandler fishNodesHandler;
        private readonly GatedWispsHandler gatedWispsHandler;
        private readonly LootChestsHandler lootChestsHandler;
        private readonly EventDispatcher eventDispatcher;

        public LeaveEventHandler(PlayersHandler playersHandler, MobsHandler mobsHandler, DungeonsHandler dungeonsHandler, FishNodesHandler fishNodesHandler, GatedWispsHandler gatedWispsHandler, LootChestsHandler lootChestsHandler, EventDispatcher eventDispatcher) : base(PacketIndexesLoader.GlobalPacketIndexes?.Leave ?? 0)
        {
            this.playersHandler = playersHandler;
            this.mobsHandler = mobsHandler;
            this.dungeonsHandler = dungeonsHandler;
            this.fishNodesHandler = fishNodesHandler;
            this.gatedWispsHandler = gatedWispsHandler;
            this.lootChestsHandler = lootChestsHandler;
            this.eventDispatcher = eventDispatcher;
        }

        protected override async Task OnActionAsync(LeaveEvent value)
        {
            playersHandler.Remove(value.Id);
            mobsHandler.Remove(value.Id);
            dungeonsHandler.Remove(value.Id);
            fishNodesHandler.Remove(value.Id);
            gatedWispsHandler.Remove(value.Id);
            lootChestsHandler.Remove(value.Id);
            
            // Emitir evento para o EventDispatcher
            await eventDispatcher.DispatchEvent(value);
        }
    }
}
