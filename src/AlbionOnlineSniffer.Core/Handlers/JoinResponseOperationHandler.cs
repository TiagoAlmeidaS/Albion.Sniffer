using Albion.Network;
using Albion.Events.V1;
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
        private readonly EventDispatcher eventDispatcher;

        public JoinResponseOperationHandler(LocalPlayerHandler localPlayerHandler, PlayersHandler playersHandler, HarvestablesHandler harvestablesHandler, MobsHandler mobsHandler, DungeonsHandler dungeonsHandler, FishNodesHandler fishNodesHandler, GatedWispsHandler gatedWispsHandler, LootChestsHandler lootChestsHandler, EventDispatcher eventDispatcher) : base(PacketIndexesLoader.GlobalPacketIndexes?.JoinResponse ?? 0)
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

        protected override async Task OnActionAsync(JoinResponseOperation value)
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

            // 🚀 CRIAR E DESPACHAR EVENTO V1
            var playerJoinedV1 = new PlayerJoinedV1
            {
                EventId = Guid.NewGuid().ToString("n"),
                ObservedAt = DateTimeOffset.UtcNow,
                Cluster = localPlayerHandler.localPlayer?.CurrentCluster?.DisplayName ?? "Unknown",
                Region = localPlayerHandler.localPlayer?.CurrentCluster?.ClusterColor.ToString() ?? "Unknown",
                PlayerId = value.Id,
                PlayerName = value.Nick,
                GuildName = string.IsNullOrWhiteSpace(value.Guild) ? null : value.Guild,
                AllianceName = string.IsNullOrWhiteSpace(value.Alliance) ? null : value.Alliance,
                Faction = value.Faction.ToString(),
                Location = value.Location,
                X = value.Position.X,
                Y = value.Position.Y
            };

            // Emitir evento Core para handlers legados - DISABLED
            // await eventDispatcher.DispatchEvent(value);
            
            // Emitir evento V1 para contratos
            await eventDispatcher.DispatchEvent(playerJoinedV1);
        }
    }
}
