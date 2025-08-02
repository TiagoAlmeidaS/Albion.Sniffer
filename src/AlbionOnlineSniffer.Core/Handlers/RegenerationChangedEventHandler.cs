using Albion.Network;
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
            
            // Emitir evento para o EventDispatcher
            await eventDispatcher.DispatchEvent(value);
        }
    }
}
