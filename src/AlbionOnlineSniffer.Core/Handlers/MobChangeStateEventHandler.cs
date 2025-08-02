using Albion.Network;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Mobs;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class MobChangeStateEventHandler : EventPacketHandler<MobChangeStateEvent>
    {
        private readonly MobsHandler mobHandler;
        public MobChangeStateEventHandler(MobsHandler mobHandler) : base(PacketIndexesLoader.GlobalPacketIndexes?.MobChangeState ?? 0)
        {
            this.mobHandler = mobHandler;
        }

        protected override Task OnActionAsync(MobChangeStateEvent value)
        {
            mobHandler.UpdateMobCharge(value.Id, value.Charge);

            return Task.CompletedTask;
        }
    }
}
