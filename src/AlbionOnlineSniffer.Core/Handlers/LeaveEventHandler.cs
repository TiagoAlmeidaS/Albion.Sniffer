using Albion.Network;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Dungeons;
using AlbionOnlineSniffer.Core.Models.GameObjects.FishNodes;
using AlbionOnlineSniffer.Core.Models.GameObjects.GatedWisps;
using AlbionOnlineSniffer.Core.Models.GameObjects.LootChests;
using AlbionOnlineSniffer.Core.Models.GameObjects.Mobs;
using AlbionOnlineSniffer.Core.Models.GameObjects.Players;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class LeaveEventHandler : EventPacketHandler<LeaveEvent>
    {
        private readonly PlayersHandler playersHandler;
        private readonly MobsHandler mobsHandler;
        private readonly DungeonsHandler dungeonsHandler;
        private readonly FishNodesHandler fishNodesHandler;
        private readonly GatedWispsHandler gatedWispsHandler;
        private readonly LootChestsHandler lootChestsHandler;

        public LeaveEventHandler(PlayersHandler playersHandler, MobsHandler mobsHandler, DungeonsHandler dungeonsHandler, FishNodesHandler fishNodesHandler, GatedWispsHandler gatedWispsHandler, LootChestsHandler lootChestsHandler) : base(PacketIndexesLoader.GlobalPacketIndexes?.Leave ?? 0)
        {
            this.playersHandler = playersHandler;
            this.mobsHandler = mobsHandler;
            this.dungeonsHandler = dungeonsHandler;
            this.fishNodesHandler = fishNodesHandler;
            this.gatedWispsHandler = gatedWispsHandler;
            this.lootChestsHandler = lootChestsHandler;
        }

        protected override Task OnActionAsync(LeaveEvent value)
        {
            playersHandler.Remove(value.Id);
            mobsHandler.Remove(value.Id);
            dungeonsHandler.Remove(value.Id);
            fishNodesHandler.Remove(value.Id);
            gatedWispsHandler.Remove(value.Id);
            lootChestsHandler.Remove(value.Id);

            return Task.CompletedTask;
        }
    }
}
