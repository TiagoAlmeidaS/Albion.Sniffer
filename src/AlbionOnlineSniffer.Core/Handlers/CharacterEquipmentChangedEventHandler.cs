using Albion.Network;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Players;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Handlers
{
    class CharacterEquipmentChangedEventHandler : EventPacketHandler<CharacterEquipmentChangedEvent>
    {
        private readonly PlayersHandler playerHandler;
        private readonly EventDispatcher eventDispatcher;

        public CharacterEquipmentChangedEventHandler(PlayersHandler playerHandler, EventDispatcher eventDispatcher) : base(PacketIndexesLoader.GlobalPacketIndexes?.CharacterEquipmentChanged ?? 0)
        {
            this.playerHandler = playerHandler;
            this.eventDispatcher = eventDispatcher;
        }

        protected override async Task OnActionAsync(CharacterEquipmentChangedEvent value)
        {
            playerHandler.UpdateItems(value.Id, value.Equipments, value.Spells);
            
                            // 🚀 CRIAR E DESPACHAR EVENTO V1
                var equipmentChangedV1 = new EquipmentChangedV1
                {
                    EventId = Guid.NewGuid().ToString("n"),
                    ObservedAt = DateTimeOffset.UtcNow,
                    Id = value.Id,
                    Equipments = value.Equipments ?? Array.Empty<int>(),
                    Spells = value.Spells ?? Array.Empty<int>()
                };

            // Emitir evento Core para handlers legados - DISABLED
            // await eventDispatcher.DispatchEvent(value);
            
            // Emitir evento V1 para contratos
            await eventDispatcher.DispatchEvent(equipmentChangedV1);
        }
    }
}
