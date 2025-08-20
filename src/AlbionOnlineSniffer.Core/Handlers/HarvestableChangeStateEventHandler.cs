using Albion.Network;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Harvestables;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class HarvestableChangeStateEventHandler : EventPacketHandler<HarvestableChangeStateEvent>
    {
        private readonly HarvestablesHandler harvestableHandler;
        private readonly EventDispatcher eventDispatcher;

        public HarvestableChangeStateEventHandler(HarvestablesHandler harvestableHandler, EventDispatcher eventDispatcher) : base(PacketIndexesLoader.GlobalPacketIndexes?.HarvestableChangeState ?? 0)
        {
            this.harvestableHandler = harvestableHandler;
            this.eventDispatcher = eventDispatcher;
        }

        protected override async Task OnActionAsync(HarvestableChangeStateEvent value)
        {
            harvestableHandler.UpdateHarvestable(value.Id, value.Count, value.Charge);
            
                            // 🚀 CRIAR E DESPACHAR EVENTO V1
                var harvestableStateChangedV1 = new HarvestableStateChangedV1
                {
                    EventId = Guid.NewGuid().ToString("n"),
                    ObservedAt = DateTimeOffset.UtcNow,
                    Id = value.Id,
                    Count = value.Count,
                    Charge = value.Charge
                };

            // Emitir evento Core para handlers legados - DISABLED
            // await eventDispatcher.DispatchEvent(value);
            
            // Emitir evento V1 para contratos
            await eventDispatcher.DispatchEvent(harvestableStateChangedV1);
        }
    }
}
