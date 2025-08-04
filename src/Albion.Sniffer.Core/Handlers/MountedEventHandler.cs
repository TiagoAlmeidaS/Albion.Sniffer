using Albion.Network;
using Albion.Sniffer.Core.Models.Events;
using Albion.Sniffer.Core.Models.GameObjects.Players;
using Albion.Sniffer.Core.Services;

namespace Albion.Sniffer.Core.Handlers
{
    class MountedEventHandler : EventPacketHandler<MountedEvent>
    {
        private readonly PlayersHandler playerHandler;
        private readonly EventDispatcher eventDispatcher;

        public MountedEventHandler(PlayersHandler playerHandler, EventDispatcher eventDispatcher) : base(PacketIndexesLoader.GlobalPacketIndexes?.Mounted ?? 0)
        {
            this.playerHandler = playerHandler;
            this.eventDispatcher = eventDispatcher;
        }

        protected override async Task OnActionAsync(MountedEvent value)
        {
            playerHandler.Mounted(value.Id, value.IsMounted);

            await eventDispatcher.DispatchEvent(value);

            return;
        }
    }
}
