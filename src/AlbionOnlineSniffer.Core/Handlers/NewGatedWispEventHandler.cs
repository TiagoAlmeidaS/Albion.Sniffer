using Albion.Network;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.GatedWisps;
using AlbionOnlineSniffer.Core.Services;
using System.Numerics;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class NewGatedWispEventHandler : EventPacketHandler<NewGatedWispEvent>
    {
        private readonly GatedWispsHandler wispInGateHandler;
        private readonly EventDispatcher eventDispatcher;

        public NewGatedWispEventHandler(GatedWispsHandler wispInGateHandler, EventDispatcher eventDispatcher) : base(PacketIndexesLoader.GlobalPacketIndexes?.NewWispGate ?? 0)
        {
            this.wispInGateHandler = wispInGateHandler;
            this.eventDispatcher = eventDispatcher;
        }

        protected override async Task OnActionAsync(NewGatedWispEvent value)
        {
            Vector2 position = Vector2.Zero;
            if (value.PositionBytes != null && value.PositionBytes.Length >= 8)
            {
                position = new Vector2(BitConverter.ToSingle(value.PositionBytes, 4), BitConverter.ToSingle(value.PositionBytes, 0));
            }

            wispInGateHandler.AddWispInGate(value.Id, position);
            
                            // 🚀 CRIAR E DESPACHAR EVENTO V1 COM POSIÇÃO
                var gatedWispFoundV1 = new GatedWispFoundV1
                {
                    EventId = Guid.NewGuid().ToString("n"),
                    ObservedAt = DateTimeOffset.UtcNow,
                    Id = value.Id,
                    X = position.X,
                    Y = position.Y
                };

            // Emitir evento Core para handlers legados - DISABLED
            // await eventDispatcher.DispatchEvent(value);
            
            // Emitir evento V1 para contratos
            await eventDispatcher.DispatchEvent(gatedWispFoundV1);
        }
    }
}
