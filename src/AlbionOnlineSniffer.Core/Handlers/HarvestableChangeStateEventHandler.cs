using Albion.Network;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Harvestables;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class HarvestableChangeStateEventHandler : EventPacketHandler<HarvestableChangeStateEvent>
    {
        private readonly HarvestablesHandler harvestableHandler;

        public HarvestableChangeStateEventHandler(HarvestablesHandler harvestableHandler) : base(PacketIndexesLoader.GlobalPacketIndexes?.HarvestableChangeState ?? 0)
        {
            this.harvestableHandler = harvestableHandler;
        }

        protected override Task OnActionAsync(HarvestableChangeStateEvent value)
        {
            harvestableHandler.UpdateHarvestable(value.Id, value.Count, value.Charge);

            return Task.CompletedTask;
        }
    }
}
