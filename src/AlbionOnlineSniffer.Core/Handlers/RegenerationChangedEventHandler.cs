using Albion.Network;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Players;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Handlers
{
    class RegenerationChangedEventHandler : EventPacketHandler<RegenerationChangedEvent>
    {
        private readonly PlayersHandler playerHandler;
        private readonly EventDispatcher eventDispatcher;

        public RegenerationChangedEventHandler(PlayersHandler playerHandler, EventDispatcher eventDispatcher) : base(PacketIndexesLoader.GlobalPacketIndexes?.RegenerationHealthChangedEvent ?? 0)
        {
            this.playerHandler = playerHandler;
            this.eventDispatcher = eventDispatcher;
        }

        protected override async Task OnActionAsync(RegenerationChangedEvent value)
        {
            playerHandler.SetRegeneration(value.Id, value.Health);
            
                            // 🚀 CRIAR E DESPACHAR EVENTO V1
                var regenerationChangedV1 = new RegenerationChangedV1
                {
                    EventId = Guid.NewGuid().ToString("n"),
                    ObservedAt = DateTimeOffset.UtcNow,
                    Id = value.Id,
                    CurrentHealth = value.Health.Value,
                    MaxHealth = value.Health.MaxValue,
                    RegenerationRate = value.Health.Regeneration
                };

            // Emitir evento Core para handlers legados - DISABLED
            // await eventDispatcher.DispatchEvent(value);
            
            // Emitir evento V1 para contratos
            await eventDispatcher.DispatchEvent(regenerationChangedV1);
        }
    }
}
