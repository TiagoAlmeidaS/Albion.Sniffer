using Albion.Network;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Mobs;
using AlbionOnlineSniffer.Core.Models.GameObjects.Players;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Handlers
{
    class HealthUpdateEventHandler : EventPacketHandler<HealthUpdateEvent>
    {
        private readonly PlayersHandler playerHandler;
        private readonly MobsHandler mobHandler;

        public HealthUpdateEventHandler(PlayersHandler playerHandler, MobsHandler mobHandler) : base(PacketIndexesLoader.GlobalPacketIndexes?.HealthUpdateEvent ?? 0)
        {
            this.playerHandler = playerHandler;
            this.mobHandler = mobHandler;
        }

        protected override Task OnActionAsync(HealthUpdateEvent value)
        {
            playerHandler.UpdateHealth(value.Id, value.Health);
            mobHandler.UpdateHealth(value.Id, value.Health);

            return Task.CompletedTask;
        }
    }
}
