using Albion.Network;
using Albion.Events.V1;
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
            
                            // 🚀 CRIAR E DESPACHAR EVENTO V1
                var healthUpdatedV1 = new HealthUpdatedV1
                {
                    EventId = Guid.NewGuid().ToString("n"),
                    ObservedAt = DateTimeOffset.UtcNow,
                    Id = value.Id,
                    Health = value.Health,
                    MaxHealth = value.MaxHealth,
                    Energy = value.Energy
                };

            // Emitir evento Core para handlers legados - DISABLED
            // await eventDispatcher.DispatchEvent(value);
            
            // Emitir evento V1 para contratos
            await eventDispatcher.DispatchEvent(healthUpdatedV1);
        }
    }
}
