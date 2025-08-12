using Albion.Network;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.FishNodes;
using AlbionOnlineSniffer.Core.Services;
using System.Numerics;

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
            Vector2 position = Vector2.Zero;
            if (value.PositionBytes != null && value.PositionBytes.Length >= 8)
            {
                position = new Vector2(BitConverter.ToSingle(value.PositionBytes, 4), BitConverter.ToSingle(value.PositionBytes, 0));
            }

            // NewFishingZoneEvent só possui Id e PositionBytes no momento
            fishZoneHandler.AddFishZone(value.Id, position, size: 0, respawnCount: 0);
            
            // Emitir evento para o EventDispatcher
            await eventDispatcher.DispatchEvent(value);
        }
    }
}
