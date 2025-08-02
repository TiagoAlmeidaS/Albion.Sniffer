using Albion.Network;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Players;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Handlers
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
