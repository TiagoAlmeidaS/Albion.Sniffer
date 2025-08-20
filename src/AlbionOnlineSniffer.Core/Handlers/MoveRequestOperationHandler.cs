using Albion.Network;
using Albion.Events.V1;
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

            // 🚀 CRIAR E DESPACHAR EVENTO V1
            var playerMoveRequestV1 = new PlayerMoveRequestV1
            {
                EventId = Guid.NewGuid().ToString("n"),
                ObservedAt = DateTimeOffset.UtcNow,
                Cluster = localPlayerHandler.localPlayer?.CurrentCluster?.DisplayName ?? "Unknown",
                Region = localPlayerHandler.localPlayer?.CurrentCluster?.ClusterColor.ToString() ?? "Unknown",
                PlayerId = localPlayerHandler.localPlayer?.Id ?? 0,
                FromX = value.Position.X,
                FromY = value.Position.Y,
                ToX = value.NewPosition.X,
                ToY = value.NewPosition.Y,
                Speed = value.Speed,
                Timestamp = value.Time
            };

            // Emitir evento Core para handlers legados - DISABLED
            // await eventDispatcher.DispatchEvent(value);
            
            // Emitir evento V1 para contratos
            await eventDispatcher.DispatchEvent(playerMoveRequestV1);
        }
    }
}
