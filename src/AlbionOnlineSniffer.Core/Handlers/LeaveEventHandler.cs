using Albion.Network;
using Albion.Events.V1;
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
            
                            // 🚀 CRIAR E DESPACHAR EVENTO V1
                var entityLeftV1 = new EntityLeftV1
                {
                    EventId = Guid.NewGuid().ToString("n"),
                    ObservedAt = DateTimeOffset.UtcNow,
                    Id = value.Id
                };

            // Emitir evento Core para handlers legados - DISABLED
            // await eventDispatcher.DispatchEvent(value);
            
            // Emitir evento V1 para contratos
            await eventDispatcher.DispatchEvent(entityLeftV1);
        }
    }
}
