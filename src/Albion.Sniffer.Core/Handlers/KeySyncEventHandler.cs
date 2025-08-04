using Albion.Network;
using Albion.Sniffer.Core.Models.Events;
using Albion.Sniffer.Core.Models.GameObjects.Players;
using Albion.Sniffer.Core.Services;

namespace Albion.Sniffer.Core.Handlers
{
    public class KeySyncEventHandler : EventPacketHandler<KeySyncEvent>
    {
        private readonly PlayersHandler playersHandler;
        private readonly EventDispatcher eventDispatcher;

        public KeySyncEventHandler(PlayersHandler playersHandler, EventDispatcher eventDispatcher) : base(PacketIndexesLoader.GlobalPacketIndexes?.KeySync ?? 0)
        {
            this.playersHandler = playersHandler;
            this.eventDispatcher = eventDispatcher;
        }

        protected override async Task OnActionAsync(KeySyncEvent value)
        {
            playersHandler.XorCode = value.Code;
            await eventDispatcher.DispatchEvent(value);
            return;
        }
    }
}
