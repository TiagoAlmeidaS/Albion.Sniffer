using Albion.Network;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Harvestables;
using AlbionOnlineSniffer.Core.Services;
using System.Numerics;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class NewHarvestableEventHandler : EventPacketHandler<NewHarvestableEvent>
    {
        private readonly HarvestablesHandler harvestableHandler;
        private readonly EventDispatcher eventDispatcher;
        private readonly LocationService locationService;

        public NewHarvestableEventHandler(HarvestablesHandler harvestableHandler, EventDispatcher eventDispatcher, LocationService locationService) : base(PacketIndexesLoader.GlobalPacketIndexes?.NewHarvestableObject ?? 0)
        {
            this.harvestableHandler = harvestableHandler;
            this.eventDispatcher = eventDispatcher;
            this.locationService = locationService;
        }

        protected override async Task OnActionAsync(NewHarvestableEvent value)
        {
            // 🔐 DESCRIPTOGRAFAR POSIÇÃO USANDO LOCATIONSERVICE
            Vector2 position = locationService.ProcessPosition(value.PositionBytes);

            harvestableHandler.AddHarvestable(value.Id, value.TypeId, value.Tier, position, count: 0, charge: value.Charges);
            
            // 🚀 CRIAR E DESPACHAR EVENTO V1 COM POSIÇÃO DESCRIPTOGRAFADA
            var harvestableFoundV1 = new HarvestableFoundV1
            {
                EventId = Guid.NewGuid().ToString("n"),
                ObservedAt = DateTimeOffset.UtcNow,
                Id = value.Id,
                TypeId = value.TypeId,
                Tier = value.Tier,
                X = position.X,
                Y = position.Y,
                Charges = value.Charges
            };

            // Emitir evento Core para handlers legados - DISABLED
            // await eventDispatcher.DispatchEvent(value);
            
            // Emitir evento V1 para contratos
            await eventDispatcher.DispatchEvent(harvestableFoundV1);
        }
    }
}
