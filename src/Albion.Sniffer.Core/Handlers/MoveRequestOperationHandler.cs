using Albion.Network;
using Albion.Sniffer.Core.Models.GameObjects.Harvestables;
using Albion.Sniffer.Core.Models.GameObjects.Localplayer;
using Albion.Sniffer.Core.Services;

namespace Albion.Sniffer.Core.Handlers
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
