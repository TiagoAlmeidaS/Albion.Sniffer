using Albion.Network;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Mobs;
using AlbionOnlineSniffer.Core.Services;
using System.Numerics;
using AlbionOnlineSniffer.Core.Models.GameObjects.Players;

namespace AlbionOnlineSniffer.Core.Handlers
{
    class NewMobEventHandler : EventPacketHandler<NewMobEvent>
    {
        private readonly MobsHandler mobHandler;
        private readonly EventDispatcher eventDispatcher;
        private readonly LocationService locationService;

        public NewMobEventHandler(MobsHandler mobHandler, EventDispatcher eventDispatcher, LocationService locationService) : base(PacketIndexesLoader.GlobalPacketIndexes?.NewMobEvent ?? 0)
        {
            this.mobHandler = mobHandler;
            this.eventDispatcher = eventDispatcher;
            this.locationService = locationService;
        }

        protected override async Task OnActionAsync(NewMobEvent value)
        {
            // 🔐 DESCRIPTOGRAFAR POSIÇÃO USANDO LOCATIONSERVICE
            Vector2 position = locationService.ProcessPosition(value.PositionBytes);

            var healthObj = new Health((int)value.Health, (int)value.MaxHealth);
            var ench = value.EnchantmentLevel;

            mobHandler.AddMob(value.Id, value.TypeId, position, healthObj, ench);
            
            // 🚀 CRIAR E DESPACHAR EVENTO V1 COM POSIÇÃO DESCRIPTOGRAFADA
            var mobSpawnedV1 = new MobSpawnedV1
            {
                EventId = Guid.NewGuid().ToString("n"),
                ObservedAt = DateTimeOffset.UtcNow,
                MobId = value.Id,
                TypeId = value.TypeId,
                Tier = value.EnchantmentLevel,
                X = position.X,
                Y = position.Y,
                Health = value.Health,
                MaxHealth = value.MaxHealth
            };

            // Emitir evento Core para handlers legados - DISABLED
            // await eventDispatcher.DispatchEvent(value);
            
            // Emitir evento V1 para contratos
            await eventDispatcher.DispatchEvent(mobSpawnedV1);
        }
    }
}
