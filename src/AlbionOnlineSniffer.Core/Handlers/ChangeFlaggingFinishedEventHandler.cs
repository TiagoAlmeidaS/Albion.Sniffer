using Albion.Network;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Models;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Localplayer;
using AlbionOnlineSniffer.Core.Models.GameObjects.Players;
using AlbionOnlineSniffer.Core.Models.ResponseObj;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Handlers
{
    class ChangeFlaggingFinishedEventHandler : EventPacketHandler<ChangeFlaggingFinishedEvent>
    {
        private readonly LocalPlayerHandler localPlayerHandler;
        private readonly PlayersHandler playerHandler;
        private readonly EventDispatcher eventDispatcher;

        public ChangeFlaggingFinishedEventHandler(LocalPlayerHandler localPlayerHandler, PlayersHandler playerHandler, EventDispatcher eventDispatcher) : base(PacketIndexesLoader.GlobalPacketIndexes?.ChangeFlaggingFinished ?? 0)
        {
            this.localPlayerHandler = localPlayerHandler;
            this.playerHandler = playerHandler;
            this.eventDispatcher = eventDispatcher;
        }

        protected override async Task OnActionAsync(ChangeFlaggingFinishedEvent value)
        {
            localPlayerHandler.SetFaction(value.Id, value.Faction);
            playerHandler.SetFaction(value.Id, value.Faction);
            
                            // 🚀 CRIAR E DESPACHAR EVENTO V1
                var flaggingFinishedV1 = new FlaggingFinishedV1
                {
                    EventId = Guid.NewGuid().ToString("n"),
                    ObservedAt = DateTimeOffset.UtcNow,
                    Id = value.Id,
                    Faction = value.Faction.ToString()
                };

            // Emitir evento Core para handlers legados - DISABLED
            // await eventDispatcher.DispatchEvent(value);
            
            // Emitir evento V1 para contratos
            await eventDispatcher.DispatchEvent(flaggingFinishedV1);
        }
    }
}
