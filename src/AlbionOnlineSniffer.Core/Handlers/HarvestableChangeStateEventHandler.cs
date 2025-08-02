using Albion.Network;
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
            
            // Emitir evento para o EventDispatcher
            await eventDispatcher.DispatchEvent(value);
        }
    }
}
