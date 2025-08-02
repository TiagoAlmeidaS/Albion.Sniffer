using Albion.Network;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.FishNodes;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class NewFishingZoneEventHandler : EventPacketHandler<NewFishingZoneEvent>
    {
        private readonly FishNodesHandler fishZoneHandler;
        public NewFishingZoneEventHandler(FishNodesHandler fishZoneHandler) : base(PacketIndexesLoader.GlobalPacketIndexes?.NewFishingZoneObject ?? 0)
        {
            this.fishZoneHandler = fishZoneHandler;
        }

        protected override Task OnActionAsync(NewFishingZoneEvent value)
        {
            fishZoneHandler.AddFishZone(value.Id, value.Position, value.Size, value.RespawnCount);

            return Task.CompletedTask;
        }
    }
}
