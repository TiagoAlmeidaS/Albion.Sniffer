using Albion.Network;
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
            
            // Emitir evento para o EventDispatcher
            await eventDispatcher.DispatchEvent(value);
        }
    }
}
