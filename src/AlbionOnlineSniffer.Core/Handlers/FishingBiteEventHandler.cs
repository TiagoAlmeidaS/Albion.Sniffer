using Albion.Network;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Services;
using System.Threading.Tasks;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class FishingBiteEventHandler : EventPacketHandler<FishingBiteEvent>
    {
        private readonly EventDispatcher eventDispatcher;

        public FishingBiteEventHandler(EventDispatcher eventDispatcher) : base(PacketIndexesLoader.GlobalPacketIndexes?.FishingBiteEvent ?? 0)
        {
            this.eventDispatcher = eventDispatcher;
        }

        protected override async Task OnActionAsync(FishingBiteEvent value)
        {
            var evt = new FishingBiteV1
            {
                EventId = System.Guid.NewGuid().ToString("n"),
                ObservedAt = System.DateTimeOffset.UtcNow,
                FishId = value.FishId,
                Difficulty = value.Difficulty,
                BiteTime = value.BiteTime
            };

            await eventDispatcher.DispatchEvent(evt);
        }
    }
}


