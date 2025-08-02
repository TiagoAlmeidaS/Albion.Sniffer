using Albion.Network;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Dungeons;
using AlbionOnlineSniffer.Core.Services;

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
            dungeonsHandler.AddDungeon(value.Id, value.Type, value.Position, value.Charges);
            
            // Emitir evento para o EventDispatcher
            await eventDispatcher.DispatchEvent(value);
        }
    }
}
