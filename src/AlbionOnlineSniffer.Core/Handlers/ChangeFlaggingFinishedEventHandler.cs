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

        public ChangeFlaggingFinishedEventHandler(LocalPlayerHandler localPlayerHandler, PlayersHandler playerHandler) : base(PacketIndexesLoader.GlobalPacketIndexes?.ChangeFlaggingFinished ?? 0)
        {
            this.localPlayerHandler = localPlayerHandler;
            this.playerHandler = playerHandler;
        }

        protected override Task OnActionAsync(ChangeFlaggingFinishedEvent value)
        {
            localPlayerHandler.SetFaction(value.Id, value.Faction);
            playerHandler.SetFaction(value.Id, value.Faction);

            return Task.CompletedTask;
        }
    }
}
