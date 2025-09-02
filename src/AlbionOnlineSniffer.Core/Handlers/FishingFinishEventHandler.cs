using Albion.Network;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Services;
using System.Threading.Tasks;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class FishingFinishEventHandler : EventPacketHandler<FishingFinishEvent>
    {
        private readonly EventDispatcher eventDispatcher;

        public FishingFinishEventHandler(EventDispatcher eventDispatcher) : base(PacketIndexesLoader.GlobalPacketIndexes?.FishingFinish ?? 0)
        {
            this.eventDispatcher = eventDispatcher;
        }

        protected override async Task OnActionAsync(FishingFinishEvent value)
        {
            var evt = new FishingFinishedV1
            {
                EventId = System.Guid.NewGuid().ToString("n"),
                ObservedAt = System.DateTimeOffset.UtcNow,
                Result = value.Result,
                ItemId = value.ItemId,
                Quantity = value.Quantity,
                Rarity = value.Rarity
            };

            await eventDispatcher.DispatchEvent(evt);
        }
    }
}


