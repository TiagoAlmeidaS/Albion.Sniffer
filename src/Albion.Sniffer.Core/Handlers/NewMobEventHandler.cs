using Albion.Network;
using Albion.Sniffer.Core.Models.Events;
using Albion.Sniffer.Core.Models.GameObjects.Mobs;
using Albion.Sniffer.Core.Services;

namespace Albion.Sniffer.Core.Handlers
{
    class NewMobEventHandler : EventPacketHandler<NewMobEvent>
    {
        private readonly MobsHandler mobHandler;
        private readonly EventDispatcher eventDispatcher;

        public NewMobEventHandler(MobsHandler mobHandler, EventDispatcher eventDispatcher) : base(PacketIndexesLoader.GlobalPacketIndexes?.NewMobEvent ?? 0)
        {
            this.mobHandler = mobHandler;
            this.eventDispatcher = eventDispatcher;
        }

        protected override async Task OnActionAsync(NewMobEvent value)
        {
            mobHandler.AddMob(value.Id, value.TypeId, value.Position, value.Health, value.Charge);
            
            // Emitir evento para o EventDispatcher
            await eventDispatcher.DispatchEvent(value);
        }
    }
}
