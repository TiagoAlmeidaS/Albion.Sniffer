using Albion.Network;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Harvestables;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class NewHarvestablesListEventHandler : EventPacketHandler<NewHarvestablesListEvent>
    {
        private readonly HarvestablesHandler harvestableHandler;
        public NewHarvestablesListEventHandler(HarvestablesHandler harvestableHandler) : base(PacketIndexesLoader.GlobalPacketIndexes?.NewHarvestableList ?? 0)
        {
            this.harvestableHandler = harvestableHandler;
        }

        protected override Task OnActionAsync(NewHarvestablesListEvent value)
        {
            foreach (var harvestableObject in value.HarvestableObjects)
            {
                harvestableHandler.AddHarvestable(harvestableObject.Id, harvestableObject.Type, harvestableObject.Tier, harvestableObject.Position, harvestableObject.Count, harvestableObject.Charge);
            }

            return Task.CompletedTask;
        }
    }
}
