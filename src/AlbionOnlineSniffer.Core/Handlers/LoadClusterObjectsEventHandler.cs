using Albion.Network;
using AlbionOnlineSniffer.Core.Models;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Localplayer;
using AlbionOnlineSniffer.Core.Services;
using System.Media;
using System.IO;

namespace AlbionOnlineSniffer.Core.Handlers
{
    class LoadClusterObjectsEventHandler : EventPacketHandler<LoadClusterObjectsEvent>
    {
        private readonly LocalPlayerHandler localPlayerHandler;
        private readonly ConfigHandler configHandler;

        // Removido o uso de Properties.Resources - será implementado quando necessário
        // private Stream announce = Properties.Resources.announce;
        // private SoundPlayer player;

        public LoadClusterObjectsEventHandler(LocalPlayerHandler localPlayerHandler, ConfigHandler configHandler) : base(PacketIndexesLoader.GlobalPacketIndexes?.LoadClusterObjects ?? 0)
        {
            // player = new SoundPlayer(announce);
            this.localPlayerHandler = localPlayerHandler;
            this.configHandler = configHandler;
        }

        protected override Task OnActionAsync(LoadClusterObjectsEvent value)
        {
            if (localPlayerHandler.localPlayer.CurrentCluster.Subtype != ClusterSubtype.Unknown)
            {
                // Comentado temporariamente até implementar recursos de som
                // if (value.ClusterObjectives != null && configHandler.config.MistOverlayEnabled)
                //     player.Play();

                localPlayerHandler.UpdateClusterObjectives(value.ClusterObjectives);
            }

            return Task.CompletedTask;
        }
    }
}
