using Albion.Network;
using Albion.Sniffer.Core.Models;
using Albion.Sniffer.Core.Models.Events;
using Albion.Sniffer.Core.Models.GameObjects.Localplayer;
using Albion.Sniffer.Core.Services;
using System.Media;
using System.IO;

namespace Albion.Sniffer.Core.Handlers
{
    class LoadClusterObjectsEventHandler : EventPacketHandler<LoadClusterObjectsEvent>
    {
        private readonly LocalPlayerHandler localPlayerHandler;
        private readonly ConfigHandler configHandler;
        private readonly EventDispatcher eventDispatcher;

        // Removido o uso de Properties.Resources - será implementado quando necessário
        // private Stream announce = Properties.Resources.announce;
        // private SoundPlayer player;

        public LoadClusterObjectsEventHandler(LocalPlayerHandler localPlayerHandler, ConfigHandler configHandler, EventDispatcher eventDispatcher) : base(PacketIndexesLoader.GlobalPacketIndexes?.LoadClusterObjects ?? 0)
        {
            // player = new SoundPlayer(announce);
            this.localPlayerHandler = localPlayerHandler;
            this.configHandler = configHandler;
            this.eventDispatcher = eventDispatcher;
        }

        protected override async Task OnActionAsync(LoadClusterObjectsEvent value)
        {
            if (localPlayerHandler.localPlayer.CurrentCluster.Subtype != ClusterSubtype.Unknown)
            {
                // Comentado temporariamente até implementar recursos de som
                // if (value.ClusterObjectives != null && configHandler.config.MistOverlayEnabled)
                //     player.Play();

                localPlayerHandler.UpdateClusterObjectives(value.ClusterObjectives);
                
                // Emitir evento para o EventDispatcher
                await eventDispatcher.DispatchEvent(value);
            }
        }
    }
}
