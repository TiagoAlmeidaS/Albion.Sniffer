using Albion.Network;
using Albion.Sniffer.Core.Models.Events;
using Albion.Sniffer.Core.Models.GameObjects.Localplayer;
using Albion.Sniffer.Core.Services;

namespace Albion.Sniffer.Core.Handlers
{
    class MistsPlayerJoinedInfoEventHandler : EventPacketHandler<MistsPlayerJoinedInfoEvent>
    {
        private readonly LocalPlayerHandler localPlayerHandler;
        private readonly EventDispatcher eventDispatcher;
        
        public MistsPlayerJoinedInfoEventHandler(LocalPlayerHandler localPlayerHandler, EventDispatcher eventDispatcher) : base(PacketIndexesLoader.GlobalPacketIndexes?.MistsPlayerJoinedInfo ?? 0)
        {
            this.localPlayerHandler = localPlayerHandler;
            this.eventDispatcher = eventDispatcher;
        }

        protected override async Task OnActionAsync(MistsPlayerJoinedInfoEvent value)
        {
            localPlayerHandler.UpdateClusterTimeCycle(value.TimeCycle);

            await eventDispatcher.DispatchEvent(value);

            return;
        }
    }
}
