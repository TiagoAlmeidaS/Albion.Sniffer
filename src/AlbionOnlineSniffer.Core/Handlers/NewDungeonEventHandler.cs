using Albion.Network;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Dungeons;
using AlbionOnlineSniffer.Core.Services;
using System.Numerics;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class NewDungeonEventHandler : EventPacketHandler<NewDungeonEvent>
    {
        private readonly DungeonsHandler dungeonsHandler;
        private readonly EventDispatcher eventDispatcher;

        public NewDungeonEventHandler(DungeonsHandler dungeonsHandler, EventDispatcher eventDispatcher) : base(PacketIndexesLoader.GlobalPacketIndexes?.NewDungeonExit ?? 0)
        {
            this.dungeonsHandler = dungeonsHandler;
            this.eventDispatcher = eventDispatcher;
        }

        protected override async Task OnActionAsync(NewDungeonEvent value)
        {
            Vector2 position = Vector2.Zero;
            if (value.PositionBytes != null && value.PositionBytes.Length >= 8)
            {
                position = new Vector2(BitConverter.ToSingle(value.PositionBytes, 4), BitConverter.ToSingle(value.PositionBytes, 0));
            }

            dungeonsHandler.AddDungeon(value.Id, value.Type ?? "NULL", position, value.Charges);
            
            // Emitir evento para o EventDispatcher
            await eventDispatcher.DispatchEvent(value);
        }
    }
}
