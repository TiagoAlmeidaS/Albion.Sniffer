using Albion.Network;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Localplayer;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Handlers
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
