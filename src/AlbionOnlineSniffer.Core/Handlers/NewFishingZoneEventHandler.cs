using Albion.Network;
using Albion.Events.V1;
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
        private readonly LocationService locationService;
        
        public NewFishingZoneEventHandler(FishNodesHandler fishZoneHandler, EventDispatcher eventDispatcher, LocationService locationService) : base(PacketIndexesLoader.GlobalPacketIndexes?.NewFishingZoneObject ?? 0)
        {
            this.fishZoneHandler = fishZoneHandler;
            this.eventDispatcher = eventDispatcher;
            this.locationService = locationService;
        }

        protected override async Task OnActionAsync(NewFishingZoneEvent value)
        {
            // 🔐 DESCRIPTOGRAFAR POSIÇÃO USANDO LOCATIONSERVICE
            Vector2 position = locationService.ProcessPosition(value.PositionBytes);

            // NewFishingZoneEvent só possui Id e PositionBytes no momento
            fishZoneHandler.AddFishZone(value.Id, position, size: 0, respawnCount: 0);
            
            // 🚀 CRIAR E DESPACHAR EVENTO V1 COM POSIÇÃO DESCRIPTOGRAFADA
            var fishingZoneFoundV1 = new FishingZoneFoundV1
            {
                EventId = Guid.NewGuid().ToString("n"),
                ObservedAt = DateTimeOffset.UtcNow,
                Id = value.Id,
                X = position.X,
                Y = position.Y
            };

            // Emitir evento Core para handlers legados - DISABLED
            // await eventDispatcher.DispatchEvent(value);
            
            // Emitir evento V1 para contratos
            await eventDispatcher.DispatchEvent(fishingZoneFoundV1);
        }
    }
}
