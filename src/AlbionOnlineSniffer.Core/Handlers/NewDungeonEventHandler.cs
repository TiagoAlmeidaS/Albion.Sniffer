using Albion.Network;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Dungeons;
using AlbionOnlineSniffer.Core.Services;
using System.Numerics;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class NewDungeonEventHandler : EventPacketHandler<NewDungeonEvent>
    {
        private readonly DungeonsHandler dungeonsHandler;
        private readonly EventDispatcher eventDispatcher;
        private readonly LocationService locationService;

        public NewDungeonEventHandler(DungeonsHandler dungeonsHandler, EventDispatcher eventDispatcher, LocationService locationService) : base(PacketIndexesLoader.GlobalPacketIndexes?.NewDungeonExit ?? 0)
        {
            this.dungeonsHandler = dungeonsHandler;
            this.eventDispatcher = eventDispatcher;
            this.locationService = locationService;
        }

        protected override async Task OnActionAsync(NewDungeonEvent value)
        {
            // 🔐 DESCRIPTOGRAFAR POSIÇÃO USANDO LOCATIONSERVICE
            Vector2 position = locationService.ProcessPosition(value.PositionBytes);

            dungeonsHandler.AddDungeon(value.Id, value.Type ?? "NULL", position, value.Charges);
            
            // 🚀 CRIAR E DESPACHAR EVENTO V1 COM POSIÇÃO DESCRIPTOGRAFADA
            var dungeonFoundV1 = new DungeonFoundV1
            {
                EventId = Guid.NewGuid().ToString("n"),
                ObservedAt = DateTimeOffset.UtcNow,
                Id = value.Id,
                Type = value.Type ?? "Unknown",
                X = position.X,
                Y = position.Y,
                Charges = value.Charges
            };

            // Emitir evento Core para handlers legados - DISABLED
            // await eventDispatcher.DispatchEvent(value);
            
            // Emitir evento V1 para contratos
            await eventDispatcher.DispatchEvent(dungeonFoundV1);
        }
    }
}
