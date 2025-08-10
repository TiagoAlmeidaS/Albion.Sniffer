using Albion.Network;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Mobs;
using AlbionOnlineSniffer.Core.Models.GameObjects.Players;
using AlbionOnlineSniffer.Core.Services;
using System.Numerics;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class MoveEventHandler : EventPacketHandler<MoveEvent>
    {
        private readonly PlayersHandler playerHandler;
        private readonly MobsHandler mobHandler;
        private readonly EventDispatcher eventDispatcher;

        public MoveEventHandler(PlayersHandler playerHandler, MobsHandler mobsHandler, EventDispatcher eventDispatcher) : base(PacketIndexesLoader.GlobalPacketIndexes?.Move ?? 0)
        {
            this.playerHandler = playerHandler;
            this.mobHandler = mobsHandler;
            this.eventDispatcher = eventDispatcher;
        }

        protected override async Task OnActionAsync(MoveEvent value)
        {
            playerHandler.UpdatePlayerPosition(value.Id, value.PositionBytes, value.NewPositionBytes, value.Speed, value.Time);
            mobHandler.UpdateMobPosition(value.Id, value.PositionBytes, value.NewPositionBytes, value.Speed, value.Time);
            
            // Enriquecer o evento com posições decriptadas (se houver chave)
            if (playerHandler.XorCode != null)
            {
                var pos = playerHandler.Decrypt(value.PositionBytes);
                var newPos = playerHandler.Decrypt(value.NewPositionBytes);
                value.Position = new Vector2(pos[1], pos[0]);
                value.NewPosition = new Vector2(newPos[1], newPos[0]);
            }

            // Emitir evento para o EventDispatcher
            await eventDispatcher.DispatchEvent(value);
        }
    }
}
