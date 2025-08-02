using Albion.Network;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Dungeons;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class NewDungeonEventHandler : EventPacketHandler<NewDungeonEvent>
    {
        private readonly DungeonsHandler dungeonsHandler;

        public NewDungeonEventHandler(DungeonsHandler dungeonsHandler) : base(PacketIndexesLoader.GlobalPacketIndexes?.NewDungeonExit ?? 0)
        {
            this.dungeonsHandler = dungeonsHandler;
        }

        protected override Task OnActionAsync(NewDungeonEvent value)
        {
            dungeonsHandler.AddDungeon(value.Id, value.Type, value.Position, value.Charges);

            return Task.CompletedTask;
        }
    }
}
