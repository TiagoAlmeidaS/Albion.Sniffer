using Albion.Network;
using Albion.Sniffer.Core.Models.Events;
using Albion.Sniffer.Core.Models.GameObjects.LootChests;
using Albion.Sniffer.Core.Services;

namespace Albion.Sniffer.Core.Handlers
{
    public class NewLootChestEventHandler : EventPacketHandler<NewLootChestEvent>
    {
        private readonly LootChestsHandler worldChestHandler;
        private readonly EventDispatcher eventDispatcher;

        public NewLootChestEventHandler(LootChestsHandler worldChestHandler, EventDispatcher eventDispatcher) : base(PacketIndexesLoader.GlobalPacketIndexes?.NewLootChest ?? 0)
        {
            this.worldChestHandler = worldChestHandler;
            this.eventDispatcher = eventDispatcher;
        }

        protected override async Task OnActionAsync(NewLootChestEvent value)
        {
            worldChestHandler.AddWorldChest(value.Id, value.Position, value.Name, value.EnchLvl);
            
            // Emitir evento para o EventDispatcher
            await eventDispatcher.DispatchEvent(value);
        }
    }
}
