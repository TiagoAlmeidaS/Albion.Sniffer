using Albion.Network;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Players;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Handlers
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

                            // 🚀 CRIAR E DESPACHAR EVENTO V1
                var mountedStateChangedV1 = new MountedStateChangedV1
                {
                    EventId = Guid.NewGuid().ToString("n"),
                    ObservedAt = DateTimeOffset.UtcNow,
                    Id = value.Id,
                    IsMounted = value.IsMounted
                };

            // Emitir evento Core para handlers legados - DISABLED
            // await eventDispatcher.DispatchEvent(value);
            
            // Emitir evento V1 para contratos
            await eventDispatcher.DispatchEvent(mountedStateChangedV1);
        }
    }
}
