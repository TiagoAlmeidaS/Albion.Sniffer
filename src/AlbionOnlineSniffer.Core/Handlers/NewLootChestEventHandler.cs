using Albion.Network;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.LootChests;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Handlers
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
