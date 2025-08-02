using Albion.Network;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Harvestables;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class NewHarvestableEventHandler : EventPacketHandler<NewHarvestableEvent>
    {
        private readonly HarvestablesHandler harvestableHandler;

        public NewHarvestableEventHandler(HarvestablesHandler harvestableHandler) : base(PacketIndexesLoader.GlobalPacketIndexes?.NewHarvestableObject ?? 0)
        {
            this.harvestableHandler = harvestableHandler;
        }

        protected override Task OnActionAsync(NewHarvestableEvent value)
        {
            harvestableHandler.AddHarvestable(value.Id, value.Type, value.Tier, value.Position, value.Count, value.Charge);

            return Task.CompletedTask;
        }
    }
}
