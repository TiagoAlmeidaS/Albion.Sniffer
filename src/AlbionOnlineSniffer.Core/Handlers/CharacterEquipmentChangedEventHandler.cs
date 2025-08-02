using Albion.Network;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Players;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Handlers
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
