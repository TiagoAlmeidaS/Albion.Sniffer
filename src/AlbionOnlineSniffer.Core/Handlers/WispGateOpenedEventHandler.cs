using Albion.Network;
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
            
            // Emitir evento para o EventDispatcher
            await eventDispatcher.DispatchEvent(value);
        }
    }
}
