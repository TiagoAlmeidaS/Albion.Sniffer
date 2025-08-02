using Albion.Network;
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
            
            // Emitir evento para o EventDispatcher
            await eventDispatcher.DispatchEvent(value);
        }
    }
}
