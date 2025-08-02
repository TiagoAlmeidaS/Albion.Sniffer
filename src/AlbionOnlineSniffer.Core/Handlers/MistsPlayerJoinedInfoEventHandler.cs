using Albion.Network;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Localplayer;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Handlers
{
    class MistsPlayerJoinedInfoEventHandler : EventPacketHandler<MistsPlayerJoinedInfoEvent>
    {
        private readonly LocalPlayerHandler localPlayerHandler;
        
        public MistsPlayerJoinedInfoEventHandler(LocalPlayerHandler localPlayerHandler) : base(PacketIndexesLoader.GlobalPacketIndexes?.MistsPlayerJoinedInfo ?? 0)
        {
            this.localPlayerHandler = localPlayerHandler;
        }

        protected override Task OnActionAsync(MistsPlayerJoinedInfoEvent value)
        {
            localPlayerHandler.UpdateClusterTimeCycle(value.TimeCycle);

            return Task.CompletedTask;
        }
    }
}
