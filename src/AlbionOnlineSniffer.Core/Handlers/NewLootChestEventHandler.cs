using Albion.Network;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.LootChests;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class NewLootChestEventHandler : EventPacketHandler<NewLootChestEvent>
    {
        private readonly LootChestsHandler worldChestHandler;

        public NewLootChestEventHandler(LootChestsHandler worldChestHandler) : base(PacketIndexesLoader.GlobalPacketIndexes?.NewLootChest ?? 0)
        {
            this.worldChestHandler = worldChestHandler;
        }

        protected override Task OnActionAsync(NewLootChestEvent value)
        {
            worldChestHandler.AddWorldChest(value.Id, value.Position, value.Name, value.EnchLvl);

            return Task.CompletedTask;
        }
    }
}
