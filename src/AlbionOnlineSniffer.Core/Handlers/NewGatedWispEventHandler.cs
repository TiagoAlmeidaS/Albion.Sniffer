using Albion.Network;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.GatedWisps;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class NewGatedWispEventHandler : EventPacketHandler<NewGatedWispEvent>
    {
        private readonly GatedWispsHandler wispInGateHandler;
        private readonly EventDispatcher eventDispatcher;

        public NewGatedWispEventHandler(GatedWispsHandler wispInGateHandler, EventDispatcher eventDispatcher) : base(PacketIndexesLoader.GlobalPacketIndexes?.NewWispGate ?? 0)
        {
            this.wispInGateHandler = wispInGateHandler;
            this.eventDispatcher = eventDispatcher;
        }

        protected override async Task OnActionAsync(NewGatedWispEvent value)
        {
            if (!value.isCollected)
                wispInGateHandler.AddWispInGate(value.Id, value.Position);
            
            // Emitir evento para o EventDispatcher
            await eventDispatcher.DispatchEvent(value);
        }
    }
}
