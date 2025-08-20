using Albion.Network;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.GatedWisps;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class WispGateOpenedEventHandler : EventPacketHandler<WispGateOpenedEvent>
    {
        private readonly GatedWispsHandler wispInGateHandler;
        private readonly EventDispatcher eventDispatcher;

        public WispGateOpenedEventHandler(GatedWispsHandler wispInGateHandler, EventDispatcher eventDispatcher) : base(PacketIndexesLoader.GlobalPacketIndexes?.WispGateOpened ?? 0)
        {
            this.wispInGateHandler = wispInGateHandler;
            this.eventDispatcher = eventDispatcher;
        }

        protected override async Task OnActionAsync(WispGateOpenedEvent value)
        {
            if (value.isCollected)
            {
                wispInGateHandler.Remove(value.Id);
            }
            
                            // 🚀 CRIAR E DESPACHAR EVENTO V1
                var wispGateOpenedV1 = new WispGateOpenedV1
                {
                    EventId = Guid.NewGuid().ToString("n"),
                    ObservedAt = DateTimeOffset.UtcNow,
                    Id = value.Id,
                    IsCollected = value.isCollected
                };

            // Emitir evento Core para handlers legados - DISABLED
            // await eventDispatcher.DispatchEvent(value);
            
            // Emitir evento V1 para contratos
            await eventDispatcher.DispatchEvent(wispGateOpenedV1);
        }
    }
}
