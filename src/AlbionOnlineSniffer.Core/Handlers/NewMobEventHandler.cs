using Albion.Network;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Mobs;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Handlers
{
    class NewMobEventHandler : EventPacketHandler<NewMobEvent>
    {
        private readonly MobsHandler mobHandler;

        public NewMobEventHandler(MobsHandler mobHandler) : base(PacketIndexesLoader.GlobalPacketIndexes?.NewMobEvent ?? 0)
        {
            this.mobHandler = mobHandler;
        }

        protected override Task OnActionAsync(NewMobEvent value)
        {
            mobHandler.AddMob(value.Id, value.TypeId, value.Position, value.Health, value.Charge);

            return Task.CompletedTask;
        }
    }
}
