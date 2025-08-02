using Albion.Network;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Players;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Handlers
{
    class MountedEventHandler : EventPacketHandler<MountedEvent>
    {
        private readonly PlayersHandler playerHandler;

        public MountedEventHandler(PlayersHandler playerHandler) : base(PacketIndexesLoader.GlobalPacketIndexes?.Mounted ?? 0)
        {
            this.playerHandler = playerHandler;
        }

        protected override Task OnActionAsync(MountedEvent value)
        {
            playerHandler.Mounted(value.Id, value.IsMounted);

            return Task.CompletedTask;
        }
    }
}
