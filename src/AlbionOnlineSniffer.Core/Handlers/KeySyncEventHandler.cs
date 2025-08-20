using Albion.Network;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Players;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class KeySyncEventHandler : EventPacketHandler<KeySyncEvent>
    {
        private readonly PlayersHandler playersHandler;
        private readonly EventDispatcher eventDispatcher;

        public KeySyncEventHandler(PlayersHandler playersHandler, EventDispatcher eventDispatcher) : base(PacketIndexesLoader.GlobalPacketIndexes?.KeySync ?? 0)
        {
            this.playersHandler = playersHandler;
            this.eventDispatcher = eventDispatcher;
        }

        protected override async Task OnActionAsync(KeySyncEvent value)
        {
            playersHandler.XorCode = value.Code;
            
                            // 🚀 CRIAR E DESPACHAR EVENTO V1
                var keySyncV1 = new KeySyncV1
                {
                    EventId = Guid.NewGuid().ToString("n"),
                    ObservedAt = DateTimeOffset.UtcNow,
                    Code = value.Code,
                    Key = 0 // TODO: Extrair key se disponível
                };

            // Emitir evento Core para handlers legados - DISABLED
            // await eventDispatcher.DispatchEvent(value);
            
            // Emitir evento V1 para contratos
            await eventDispatcher.DispatchEvent(keySyncV1);
        }
    }
}
