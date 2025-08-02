using Albion.Network;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Players;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class KeySyncEventHandler : EventPacketHandler<KeySyncEvent>
    {
        private readonly PlayersHandler playersHandler;

        public KeySyncEventHandler(PlayersHandler playersHandler) : base(PacketIndexesLoader.GlobalPacketIndexes?.KeySync ?? 0)
        {
            this.playersHandler = playersHandler;
        }

        protected override Task OnActionAsync(KeySyncEvent value)
        {
            playersHandler.XorCode = value.Code;
            return Task.CompletedTask;
        }
    }
}
