using Albion.Network;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Players;
using AlbionOnlineSniffer.Core.Models.GameObjects.Mobs;
using AlbionOnlineSniffer.Core.Services;
using System.Numerics;

namespace AlbionOnlineSniffer.Core.Handlers
{
    class MoveEventHandler : EventPacketHandler<MoveEvent>
    {
        private readonly PlayersHandler playerHandler;
        private readonly MobsHandler mobHandler;
        private readonly EventDispatcher eventDispatcher;

        public MoveEventHandler(PlayersHandler playerHandler, MobsHandler mobHandler, EventDispatcher eventDispatcher) : base(PacketIndexesLoader.GlobalPacketIndexes?.Move ?? 0)
        {
            this.playerHandler = playerHandler;
            this.mobHandler = mobHandler;
            this.eventDispatcher = eventDispatcher;
        }

        protected override async Task OnActionAsync(MoveEvent value)
        {
            // Enriquecer MoveEvent com posições decriptadas (se possível)
            if (playerHandler.XorCode != null && value.PositionBytes != null && value.PositionBytes.Length >= 8)
            {
                var coords = playerHandler.Decrypt(value.PositionBytes);
                value.Position = new Vector2(coords[1], coords[0]);
            }
            if (playerHandler.XorCode != null && value.NewPositionBytes != null && value.NewPositionBytes.Length >= 8)
            {
                var newCoords = playerHandler.Decrypt(value.NewPositionBytes);
                value.NewPosition = new Vector2(newCoords[1], newCoords[0]);
            }

            playerHandler.UpdatePlayerPosition(value.Id, value.PositionBytes, value.NewPositionBytes, value.Speed, value.Time);
            mobHandler.UpdateMobPosition(value.Id, value.PositionBytes, value.NewPositionBytes, value.Speed, value.Time);

            await eventDispatcher.DispatchEvent(value);
        }
    }
}
