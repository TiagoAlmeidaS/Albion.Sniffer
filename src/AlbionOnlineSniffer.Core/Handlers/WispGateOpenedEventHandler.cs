using Albion.Network;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.GatedWisps;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class WispGateOpenedEventHandler : EventPacketHandler<WispGateOpenedEvent>
    {
        private readonly GatedWispsHandler wispInGateHandler;

        public WispGateOpenedEventHandler(GatedWispsHandler wispInGateHandler) : base(PacketIndexesLoader.GlobalPacketIndexes?.WispGateOpened ?? 0)
        {
            this.wispInGateHandler = wispInGateHandler;
        }

        protected override Task OnActionAsync(WispGateOpenedEvent value)
        {
            if (value.isCollected)
            {
                wispInGateHandler.Remove(value.Id);
            }

            return Task.CompletedTask;
        }
    }
}
