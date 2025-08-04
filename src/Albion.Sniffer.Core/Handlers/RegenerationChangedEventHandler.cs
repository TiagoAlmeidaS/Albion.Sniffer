using Albion.Network;
using Albion.Sniffer.Core.Models.Events;
using Albion.Sniffer.Core.Models.GameObjects.Players;
using Albion.Sniffer.Core.Services;

namespace Albion.Sniffer.Core.Handlers
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
            
            // Emitir evento para o EventDispatcher
            await eventDispatcher.DispatchEvent(value);
        }
    }
}
