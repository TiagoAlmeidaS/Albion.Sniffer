using Albion.Network;
using AlbionOnlineSniffer.Core.Models.GameObjects.Harvestables;
using AlbionOnlineSniffer.Core.Models.GameObjects.Localplayer;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class MoveRequestOperationHandler : RequestPacketHandler<MoveRequestOperation>
    {
        private readonly LocalPlayerHandler localPlayerHandler;
        private readonly HarvestablesHandler harvestablesHandler;
        private readonly EventDispatcher eventDispatcher;

        public MoveRequestOperationHandler(LocalPlayerHandler localPlayerHandler, HarvestablesHandler harvestablesHandler, EventDispatcher eventDispatcher) : base(PacketIndexesLoader.GlobalPacketIndexes?.MoveRequest ?? 0)
        {
            this.localPlayerHandler = localPlayerHandler;
            this.harvestablesHandler = harvestablesHandler;
            this.eventDispatcher = eventDispatcher;
        }

        protected override async Task OnActionAsync(MoveRequestOperation value)
        {
            localPlayerHandler.Move(value.Position, value.NewPosition, value.Speed, value.Time);
            
            if(!localPlayerHandler.localPlayer.IsStanding)
                harvestablesHandler.RemoveHarvestables();

            await eventDispatcher.DispatchEvent(value);

            return;
        }
    }
}
