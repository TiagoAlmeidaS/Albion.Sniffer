using Albion.Network;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Services;
using System.Threading.Tasks;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class FishingMiniGameUpdateEventHandler : EventPacketHandler<FishingMiniGameUpdateEvent>
    {
        private readonly EventDispatcher eventDispatcher;

        public FishingMiniGameUpdateEventHandler(EventDispatcher eventDispatcher) : base(PacketIndexesLoader.GlobalPacketIndexes?.FishingMiniGameUpdate ?? 0)
        {
            this.eventDispatcher = eventDispatcher;
        }

        protected override async Task OnActionAsync(FishingMiniGameUpdateEvent value)
        {
            var evt = new FishingMiniGameUpdatedV1
            {
                EventId = System.Guid.NewGuid().ToString("n"),
                ObservedAt = System.DateTimeOffset.UtcNow,
                BobPosition = value.BobPosition,
                BarSpeed = value.BarSpeed,
                Direction = value.Direction
            };

            await eventDispatcher.DispatchEvent(evt);
        }
    }
}


