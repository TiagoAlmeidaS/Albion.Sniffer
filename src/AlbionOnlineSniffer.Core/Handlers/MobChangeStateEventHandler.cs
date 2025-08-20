using Albion.Network;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Mobs;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class MobChangeStateEventHandler : EventPacketHandler<MobChangeStateEvent>
    {
        private readonly MobsHandler mobHandler;
        private readonly EventDispatcher eventDispatcher;
        public MobChangeStateEventHandler(MobsHandler mobHandler, EventDispatcher eventDispatcher) : base(PacketIndexesLoader.GlobalPacketIndexes?.MobChangeState ?? 0)
        {
            this.mobHandler = mobHandler;
            this.eventDispatcher = eventDispatcher;
        }

        protected override async Task OnActionAsync(MobChangeStateEvent value)
        {
            mobHandler.UpdateMobCharge(value.Id, value.Charge);
            
                            // 🚀 CRIAR E DESPACHAR EVENTO V1
                var mobStateChangedV1 = new MobStateChangedV1
                {
                    EventId = Guid.NewGuid().ToString("n"),
                    ObservedAt = DateTimeOffset.UtcNow,
                    Id = value.Id,
                    Charge = value.Charge
                };

            // Emitir evento Core para handlers legados - DISABLED
            // await eventDispatcher.DispatchEvent(value);
            
            // Emitir evento V1 para contratos
            await eventDispatcher.DispatchEvent(mobStateChangedV1);
        }
    }
}
