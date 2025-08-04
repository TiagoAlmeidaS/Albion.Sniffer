using Albion.Network;
using Albion.Sniffer.Core.Models.Events;
using Albion.Sniffer.Core.Models.GameObjects.Mobs;
using Albion.Sniffer.Core.Models.GameObjects.Players;
using Albion.Sniffer.Core.Services;

namespace Albion.Sniffer.Core.Handlers
{
    class HealthUpdateEventHandler : EventPacketHandler<HealthUpdateEvent>
    {
        private readonly PlayersHandler playerHandler;
        private readonly MobsHandler mobHandler;
        private readonly EventDispatcher eventDispatcher;

        public HealthUpdateEventHandler(PlayersHandler playerHandler, MobsHandler mobHandler, EventDispatcher eventDispatcher) : base(PacketIndexesLoader.GlobalPacketIndexes?.HealthUpdateEvent ?? 0)
        {
            this.playerHandler = playerHandler;
            this.mobHandler = mobHandler;
            this.eventDispatcher = eventDispatcher;
        }

        protected override async Task OnActionAsync(HealthUpdateEvent value)
        {
            playerHandler.UpdateHealth(value.Id, value.Health);
            mobHandler.UpdateHealth(value.Id, value.Health);
            
            // Emitir evento para o EventDispatcher
            await eventDispatcher.DispatchEvent(value);
        }
    }
}
