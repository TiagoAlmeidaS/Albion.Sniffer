using Albion.Network;
using Albion.Sniffer.Core.Models.Events;
using Albion.Sniffer.Core.Models.GameObjects.Harvestables;
using Albion.Sniffer.Core.Services;

namespace Albion.Sniffer.Core.Handlers
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
            
            // Emitir evento para o EventDispatcher
            await eventDispatcher.DispatchEvent(value);
        }
    }
}
