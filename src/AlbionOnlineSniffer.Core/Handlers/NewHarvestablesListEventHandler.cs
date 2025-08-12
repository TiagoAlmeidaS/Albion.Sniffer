using Albion.Network;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Harvestables;
using AlbionOnlineSniffer.Core.Services;
using System.Numerics;

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
                Vector2 position = Vector2.Zero;
                if (harvestableObject.PositionBytes != null && harvestableObject.PositionBytes.Length >= 8)
                {
                    position = new Vector2(BitConverter.ToSingle(harvestableObject.PositionBytes, 4), BitConverter.ToSingle(harvestableObject.PositionBytes, 0));
                }

                harvestableHandler.AddHarvestable(harvestableObject.Id, harvestableObject.TypeId, harvestableObject.Tier, position, count: 0, charge: harvestableObject.Charges);
            }
            // Emitir evento para o EventDispatcher
            await eventDispatcher.DispatchEvent(value);
        }
    }
}
