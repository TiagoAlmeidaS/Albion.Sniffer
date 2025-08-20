using Albion.Network;
using Albion.Events.V1;
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
        private readonly LocationService locationService;

        public MoveEventHandler(PlayersHandler playerHandler, MobsHandler mobsHandler, EventDispatcher eventDispatcher, LocationService locationService) : base(PacketIndexesLoader.GlobalPacketIndexes?.Move ?? 0)
        {
            this.playerHandler = playerHandler;
            this.mobHandler = mobsHandler;
            this.eventDispatcher = eventDispatcher;
            this.locationService = locationService;
        }

        protected override async Task OnActionAsync(MoveEvent value)
        {
            // 🔐 DESCRIPTOGRAFAR POSIÇÕES USANDO LOCATIONSERVICE
            var fromPosition = locationService.ProcessPosition(value.PositionBytes);
            var toPosition = locationService.ProcessPosition(value.NewPositionBytes);
            
            // Atualizar posições nos handlers
            playerHandler.UpdatePlayerPosition(value.Id, value.PositionBytes, value.NewPositionBytes, value.Speed, value.Time);
            mobHandler.UpdateMobPosition(value.Id, value.PositionBytes, value.NewPositionBytes, value.Speed, value.Time);
            
            // Enriquecer o evento Core com posições decriptadas
            value.Position = fromPosition;
            value.NewPosition = toPosition;

            // 🚀 CRIAR E DESPACHAR EVENTO V1 COM POSIÇÕES DESCRIPTOGRAFADAS E VALIDADAS
            var playerMovedV1 = new PlayerMovedV1
            {
                EventId = Guid.NewGuid().ToString("n"),
                ObservedAt = DateTimeOffset.UtcNow,
                Cluster = "Unknown", // TODO: Obter do LocalPlayerHandler
                Region = "Unknown",  // TODO: Obter do LocalPlayerHandler
                PlayerId = value.Id,
                PlayerName = "Unknown", // TODO: Obter do PlayersHandler
                FromX = fromPosition.X,
                FromY = fromPosition.Y,
                ToX = toPosition.X,
                ToY = toPosition.Y,
                Speed = value.Speed,
                Timestamp = value.Time
            };

            // Emitir evento Core para handlers legados - DISABLED
            // await eventDispatcher.DispatchEvent(value);
            
            // Emitir evento V1 para contratos
            await eventDispatcher.DispatchEvent(playerMovedV1);
        }
    }
}
