using Albion.Network;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Players;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Handlers
{
    class RegenerationChangedEventHandler : EventPacketHandler<RegenerationChangedEvent>
    {
        private readonly PlayersHandler playerHandler;

        public RegenerationChangedEventHandler(PlayersHandler playerHandler) : base(PacketIndexesLoader.GlobalPacketIndexes?.RegenerationHealthChangedEvent ?? 0)
        {
            this.playerHandler = playerHandler;
        }

        protected override Task OnActionAsync(RegenerationChangedEvent value)
        {
            playerHandler.SetRegeneration(value.Id, value.Health);

            return Task.CompletedTask;
        }
    }
}
