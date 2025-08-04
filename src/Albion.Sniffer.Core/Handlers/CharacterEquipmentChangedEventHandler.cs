using Albion.Network;
using Albion.Sniffer.Core.Models.Events;
using Albion.Sniffer.Core.Models.GameObjects.Players;
using Albion.Sniffer.Core.Services;

namespace Albion.Sniffer.Core.Handlers
{
    class CharacterEquipmentChangedEventHandler : EventPacketHandler<CharacterEquipmentChanged>
    {
        private readonly PlayersHandler playerHandler;
        private readonly EventDispatcher eventDispatcher;

        public CharacterEquipmentChangedEventHandler(PlayersHandler playerHandler, EventDispatcher eventDispatcher) : base(PacketIndexesLoader.GlobalPacketIndexes?.CharacterEquipmentChanged ?? 0)
        {
            this.playerHandler = playerHandler;
            this.eventDispatcher = eventDispatcher;
        }

        protected override async Task OnActionAsync(CharacterEquipmentChanged value)
        {
            playerHandler.UpdateItems(value.Id, value.Equipments, value.Spells);
            
            // Emitir evento para o EventDispatcher
            await eventDispatcher.DispatchEvent(value);
        }
    }
}
