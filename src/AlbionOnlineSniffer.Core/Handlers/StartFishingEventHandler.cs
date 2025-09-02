using Albion.Network;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Services;
using System.Threading.Tasks;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class StartFishingEventHandler : EventPacketHandler<StartFishingEvent>
    {
        private readonly EventDispatcher eventDispatcher;

        public StartFishingEventHandler(EventDispatcher eventDispatcher) : base(PacketIndexesLoader.GlobalPacketIndexes?.StartFishing ?? 0)
        {
            this.eventDispatcher = eventDispatcher;
        }

        protected override async Task OnActionAsync(StartFishingEvent value)
        {
            var evt = new FishingStartedV1
            {
                EventId = System.Guid.NewGuid().ToString("n"),
                ObservedAt = System.DateTimeOffset.UtcNow,
                RodId = value.RodId,
                BaitId = value.BaitId,
                TargetX = value.TargetX,
                TargetY = value.TargetY
            };

            await eventDispatcher.DispatchEvent(evt);
        }
    }
}


