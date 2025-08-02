using Albion.Network;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Harvestables;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class NewHarvestablesListEventHandler : EventPacketHandler<NewHarvestablesListEvent>
    {
        private readonly HarvestablesHandler harvestableHandler;
        private readonly EventDispatcher eventDispatcher;
        public NewHarvestablesListEventHandler(HarvestablesHandler harvestableHandler, EventDispatcher eventDispatcher) : base(PacketIndexesLoader.GlobalPacketIndexes?.NewHarvestableList ?? 0)
        {
            this.harvestableHandler = harvestableHandler;
            this.eventDispatcher = eventDispatcher;
        }

        protected override async Task OnActionAsync(NewHarvestablesListEvent value)
        {
            foreach (var harvestableObject in value.HarvestableObjects)
            {
                harvestableHandler.AddHarvestable(harvestableObject.Id, harvestableObject.Type, harvestableObject.Tier, harvestableObject.Position, harvestableObject.Count, harvestableObject.Charge);
            }
            // Emitir evento para o EventDispatcher
            await eventDispatcher.DispatchEvent(value);
        }
    }
}
