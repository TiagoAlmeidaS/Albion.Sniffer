using Albion.Network;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Mobs;
using AlbionOnlineSniffer.Core.Models.GameObjects.Players;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Handlers
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
            playerHandler.UpdateHealth(value.Id, (int)value.Health);
            mobHandler.UpdateHealth(value.Id, (int)value.Health);
            
            // Emitir evento para o EventDispatcher
            await eventDispatcher.DispatchEvent(value);
        }
    }
}
