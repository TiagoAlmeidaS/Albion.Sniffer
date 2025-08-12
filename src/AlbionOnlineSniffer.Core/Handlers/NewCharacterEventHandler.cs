using System.Numerics;
using Albion.Network;
using AlbionOnlineSniffer.Core.Models;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Localplayer;
using AlbionOnlineSniffer.Core.Models.GameObjects.Players;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Utility;
using System.Media;
using System.IO;
using System.Collections.Generic;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class NewCharacterEventHandler : EventPacketHandler<NewCharacterEvent>
    {
        private readonly ConfigHandler configHandler;
        private readonly LocalPlayerHandler localPlayerHandler;
        private readonly PlayersHandler playerHandler;
        private readonly EventDispatcher eventDispatcher;

        // Removido o uso de Properties.Resources - será implementado quando necessário
        // Stream beep = Properties.Resources.beep;
        // SoundPlayer player;

        public NewCharacterEventHandler(PlayersHandler playerHandler, LocalPlayerHandler localPlayerHandler, ConfigHandler configHandler, EventDispatcher eventDispatcher) : base(PacketIndexesLoader.GlobalPacketIndexes?.NewCharacter ?? 0)
        {
            // player = new SoundPlayer(beep);

            this.playerHandler = playerHandler;
            this.localPlayerHandler = localPlayerHandler;
            this.configHandler = configHandler;
            this.eventDispatcher = eventDispatcher;
        }

        // Overload required by tests: accepts only PlayersHandler and EventDispatcher
        public NewCharacterEventHandler(PlayersHandler playerHandler, EventDispatcher eventDispatcher)
            : this(
                playerHandler,
                new LocalPlayerHandler(new Dictionary<string, Cluster>()),
                new ConfigHandler(),
                eventDispatcher)
        {
        }

        protected override async Task OnActionAsync(NewCharacterEvent value)
        {
            Vector2 pos = Vector2.Zero;
            
            if (playerHandler.XorCode != null && value.PositionBytes != null)
            {
                var coords = playerHandler.Decrypt(value.PositionBytes);
                pos = new Vector2(coords[1], coords[0]);
            }
            // Caso não tenha bytes, mantém Vector2.Zero
            
            // Preencher Position no próprio evento para publicação reutilizável (IHasPosition)
            value.Position = pos;

            playerHandler.AddPlayer(value.Id, value.Name, value.GuildName, value.AllianceName, pos, new Health(0,0), Utility.Faction.NoPVP, new int[0], new int[0]);

            // Emitir evento para o EventDispatcher
            await eventDispatcher.DispatchEvent(value);

            if (localPlayerHandler.localPlayer.CurrentCluster.ClusterColor != ClusterColor.City)
            {
                #region CUSTOM LISTS

                // Comentado temporariamente até implementar recursos de som
                /*
                if (configHandler.config.FriendlyLists)
                {
                    if (configHandler.config.FriendlyPlayersList.Contains(value.Name, StringComparer.OrdinalIgnoreCase))
                    {
                        if (Convert.ToBoolean(configHandler.config.FriendlyPlayer[0]))
                            PlayBeep(Convert.ToInt32(configHandler.config.FriendlyPlayer[5]));
                        return Task.CompletedTask;
                    }

                    if (value.Guild == localPlayerHandler.localPlayer.Guild || configHandler.config.FriendlyGuildsList.Contains(value.Guild, StringComparer.OrdinalIgnoreCase))
                    {
                        if (Convert.ToBoolean(configHandler.config.FriendlyGuild[0]))
                            PlayBeep(Convert.ToInt32(configHandler.config.FriendlyGuild[5]));
                        return Task.CompletedTask;
                    }

                    if (value.Alliance == localPlayerHandler.localPlayer.Alliance || configHandler.config.FriendlyAlliancesList.Contains(value.Alliance, StringComparer.OrdinalIgnoreCase))
                    {
                        if (Convert.ToBoolean(configHandler.config.FriendlyAlliance[0]))
                            PlayBeep(Convert.ToInt32(configHandler.config.FriendlyAlliance[5]));
                        return Task.CompletedTask;
                    }
                }

                if (configHandler.config.EnemyLists)
                {
                    if (configHandler.config.EnemyPlayersList.Contains(value.Name, StringComparer.OrdinalIgnoreCase))
                    {
                        if (Convert.ToBoolean(configHandler.config.EnemyPlayer[0]))
                            PlayBeep(Convert.ToInt32(configHandler.config.EnemyPlayer[5]));
                        return Task.CompletedTask;
                    }

                    if (configHandler.config.EnemyGuildsList.Contains(value.Guild, StringComparer.OrdinalIgnoreCase))
                    {
                        if (Convert.ToBoolean(configHandler.config.EnemyGuildsList[0]))
                            PlayBeep(Convert.ToInt32(configHandler.config.EnemyGuildsList[5]));
                        return Task.CompletedTask;
                    }

                    if (configHandler.config.EnemyAlliancesList.Contains(value.Alliance, StringComparer.OrdinalIgnoreCase))
                    {
                        if (Convert.ToBoolean(configHandler.config.EnemyAlliancesList[0]))
                            PlayBeep(Convert.ToInt32(configHandler.config.EnemyAlliancesList[5]));
                        return Task.CompletedTask;
                    }
                }
                */

                #endregion

                #region FACTION WAR

                // Comentado temporariamente até implementar estrutura de configuração
                /*
                if (configHandler.config.FactionWar && (localPlayerHandler.localPlayer.Faction != Faction.NoPVP && localPlayerHandler.localPlayer.Faction != Faction.PVP))
                {
                    if (value.Faction == Faction.NoPVP) return Task.CompletedTask;

                    if (value.Faction == localPlayerHandler.localPlayer.Faction)
                    {
                        if (Convert.ToBoolean(configHandler.config.FriendlyFaction[0]))
                            PlayBeep(Convert.ToInt32(configHandler.config.FriendlyFaction[5]));
                        return Task.CompletedTask;
                    }
                    else if (value.Faction != localPlayerHandler.localPlayer.Faction && value.Faction != Faction.PVP)
                    {
                        if (Convert.ToBoolean(configHandler.config.EnemyFaction[0]))
                            PlayBeep(Convert.ToInt32(configHandler.config.EnemyFaction[5]));
                        return Task.CompletedTask;
                    }
                    else
                    {
                        if (Convert.ToBoolean(configHandler.config.Pvp[0]))
                            PlayBeep(Convert.ToInt32(configHandler.config.Pvp[5]));
                        return Task.CompletedTask;
                    }
                }
                */

                #endregion

                // Comentado temporariamente até implementar estrutura de configuração
                /*
                if (localPlayerHandler.localPlayer.CurrentCluster.ClusterColor == ClusterColor.Default)
                {
                    switch (value.Faction)
                    {
                        case Faction.NoPVP:
                            if (Convert.ToBoolean(configHandler.config.NoPvp[0]))
                                PlayBeep(Convert.ToInt32(configHandler.config.NoPvp[5]));
                            break;

                        case Faction.Martlock:
                        case Faction.Lymhurst:
                        case Faction.Bridjewatch:
                        case Faction.ForthSterling:
                        case Faction.Thetford:
                        case Faction.Caerleon:
                            if (Convert.ToBoolean(configHandler.config.Faction[0]))
                                PlayBeep(Convert.ToInt32(configHandler.config.Faction[5]));
                            break;

                        case Faction.PVP:
                            if (Convert.ToBoolean(configHandler.config.Pvp[0]))
                                PlayBeep(Convert.ToInt32(configHandler.config.Pvp[5]));
                            break;
                    }
                }
                else
                {
                    if (Convert.ToBoolean(configHandler.config.Pvp[0]))
                        PlayBeep(Convert.ToInt32(configHandler.config.Pvp[5]));
                }
                */
            }
        }

        private void PlayBeep(int play)
        {
            if (play == 0) return;

            // player.Play(); // Removido o uso de SoundPlayer
        }
    }
}
