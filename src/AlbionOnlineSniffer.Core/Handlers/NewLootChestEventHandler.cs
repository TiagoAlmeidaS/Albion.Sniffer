using Albion.Network;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.LootChests;
using AlbionOnlineSniffer.Core.Services;
using System.Numerics;

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
            Vector2 position = Vector2.Zero;
            if (value.PositionBytes != null && value.PositionBytes.Length >= 8)
            {
                position = new Vector2(BitConverter.ToSingle(value.PositionBytes, 4), BitConverter.ToSingle(value.PositionBytes, 0));
            }

            // NewLootChestEvent só possui Id e PositionBytes atualmente; usar defaults
            worldChestHandler.AddWorldChest(value.Id, position, name: "NULL", enchLvl: 0);
            
                            // 🚀 CRIAR E DESPACHAR EVENTO V1 COM POSIÇÃO
                var lootChestFoundV1 = new LootChestFoundV1
                {
                    EventId = Guid.NewGuid().ToString("n"),
                    ObservedAt = DateTimeOffset.UtcNow,
                    Id = value.Id,
                    X = position.X,
                    Y = position.Y
                };

            // Emitir evento Core para handlers legados - DISABLED
            // await eventDispatcher.DispatchEvent(value);
            
            // Emitir evento V1 para contratos
            await eventDispatcher.DispatchEvent(lootChestFoundV1);
        }
    }
}
