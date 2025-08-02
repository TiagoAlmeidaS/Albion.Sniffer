using Albion.Network;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Players;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Handlers
{
    class CharacterEquipmentChangedEventHandler : EventPacketHandler<CharacterEquipmentChanged>
    {
        private readonly PlayersHandler playerHandler;

        public CharacterEquipmentChangedEventHandler(PlayersHandler playerHandler) : base(PacketIndexesLoader.GlobalPacketIndexes?.CharacterEquipmentChanged ?? 0)
        {
            this.playerHandler = playerHandler;
        }

        protected override Task OnActionAsync(CharacterEquipmentChanged value)
        {
            playerHandler.UpdateItems(value.Id, value.Equipments, value.Spells);

            return Task.CompletedTask;
        }
    }
}
