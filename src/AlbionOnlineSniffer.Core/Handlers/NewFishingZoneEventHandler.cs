using Albion.Network;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.FishNodes;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class NewFishingZoneEventHandler : EventPacketHandler<NewFishingZoneEvent>
    {
        private readonly FishNodesHandler fishZoneHandler;
        private readonly EventDispatcher eventDispatcher;
        
        public NewFishingZoneEventHandler(FishNodesHandler fishZoneHandler, EventDispatcher eventDispatcher) : base(PacketIndexesLoader.GlobalPacketIndexes?.NewFishingZoneObject ?? 0)
        {
            this.fishZoneHandler = fishZoneHandler;
            this.eventDispatcher = eventDispatcher;
        }

        protected override async Task OnActionAsync(NewFishingZoneEvent value)
        {
            fishZoneHandler.AddFishZone(value.Id, value.Position, value.Size, value.RespawnCount);
            
            // Emitir evento para o EventDispatcher
            await eventDispatcher.DispatchEvent(value);
        }
    }
}
