using Albion.Network;
using Albion.Sniffer.Core.Models;
using Albion.Sniffer.Core.Models.Events;
using Albion.Sniffer.Core.Models.GameObjects.Localplayer;
using Albion.Sniffer.Core.Models.GameObjects.Players;
using Albion.Sniffer.Core.Models.ResponseObj;
using Albion.Sniffer.Core.Services;

namespace Albion.Sniffer.Core.Handlers
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
            
            // Emitir evento para o EventDispatcher
            await eventDispatcher.DispatchEvent(value);
        }
    }
}
