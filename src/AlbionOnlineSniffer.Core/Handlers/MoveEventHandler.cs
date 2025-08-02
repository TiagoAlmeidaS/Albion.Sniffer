using Albion.Network;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Mobs;
using AlbionOnlineSniffer.Core.Models.GameObjects.Players;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class MoveEventHandler : EventPacketHandler<MoveEvent>
    {
        private readonly PlayersHandler playerHandler;
        private readonly MobsHandler mobHandler;

        public MoveEventHandler(PlayersHandler playerHandler, MobsHandler mobsHandler) : base(PacketIndexesLoader.GlobalPacketIndexes?.Move ?? 0)
        {
            this.playerHandler = playerHandler;
            this.mobHandler = mobsHandler;
        }

        protected override Task OnActionAsync(MoveEvent value)
        {
            playerHandler.UpdatePlayerPosition(value.Id, value.PositionBytes, value.NewPositionBytes, value.Speed, value.Time);
            mobHandler.UpdateMobPosition(value.Id, value.PositionBytes, value.NewPositionBytes, value.Speed, value.Time);

            return Task.CompletedTask;
        }
    }
}
