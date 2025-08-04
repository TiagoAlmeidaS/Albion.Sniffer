using Albion.Network;
using Albion.Sniffer.Core.Models.Events;
using Albion.Sniffer.Core.Models.GameObjects.Dungeons;
using Albion.Sniffer.Core.Services;

namespace Albion.Sniffer.Core.Handlers
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
