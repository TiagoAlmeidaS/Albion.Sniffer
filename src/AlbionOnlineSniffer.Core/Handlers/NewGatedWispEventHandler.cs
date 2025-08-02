using Albion.Network;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.GatedWisps;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class NewGatedWispEventHandler : EventPacketHandler<NewGatedWispEvent>
    {
        private readonly GatedWispsHandler wispInGateHandler;

        public NewGatedWispEventHandler(GatedWispsHandler wispInGateHandler) : base(PacketIndexesLoader.GlobalPacketIndexes?.NewWispGate ?? 0)
        {
            this.wispInGateHandler = wispInGateHandler;
        }

        protected override Task OnActionAsync(NewGatedWispEvent value)
        {
            if (!value.isCollected)
                wispInGateHandler.AddWispInGate(value.Id, value.Position);

            return Task.CompletedTask;
        }
    }
}
