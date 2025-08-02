using Albion.Network;
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
    public class JoinResponseOperationHandler : ResponsePacketHandler<JoinResponseOperation>
    {
        private readonly LocalPlayerHandler localPlayerHandler;
        private readonly PlayersHandler playersHandler;
        private readonly HarvestablesHandler harvestablesHandler;
        private readonly MobsHandler mobsHandler;
        private readonly DungeonsHandler dungeonsHandler;
        private readonly FishNodesHandler fishNodesHandler;
        private readonly GatedWispsHandler gatedWispsHandler;
        private readonly LootChestsHandler lootChestsHandler;

        public JoinResponseOperationHandler(LocalPlayerHandler localPlayerHandler, PlayersHandler playersHandler, HarvestablesHandler harvestablesHandler, MobsHandler mobsHandler, DungeonsHandler dungeonsHandler, FishNodesHandler fishNodesHandler, GatedWispsHandler gatedWispsHandler, LootChestsHandler lootChestsHandler) : base(PacketIndexesLoader.GlobalPacketIndexes?.JoinResponse ?? 0)
        {
            this.localPlayerHandler = localPlayerHandler;
            this.playersHandler = playersHandler;
            this.harvestablesHandler = harvestablesHandler;
            this.mobsHandler = mobsHandler;
            this.dungeonsHandler = dungeonsHandler;
            this.fishNodesHandler = fishNodesHandler;
            this.gatedWispsHandler = gatedWispsHandler;
            this.lootChestsHandler = lootChestsHandler;
        }

        protected override Task OnActionAsync(JoinResponseOperation value)
        {
            localPlayerHandler.UpdateInfo(value.Id, value.Nick, value.Guild, value.Alliance, value.Faction, value.Position);

            if (localPlayerHandler.ChangeCluster(value.Location) && localPlayerHandler.localPlayer.CurrentCluster.ClusterColor != ClusterColor.Unknown)
            {
                playersHandler.Clear();
                harvestablesHandler.Clear();
                mobsHandler.Clear();
                dungeonsHandler.Clear();
                fishNodesHandler.Clear();
                gatedWispsHandler.Clear();
                lootChestsHandler.Clear();
            }

            return Task.CompletedTask;
        }
    }
}
