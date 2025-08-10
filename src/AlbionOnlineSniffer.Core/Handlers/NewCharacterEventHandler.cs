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

        protected override async Task OnActionAsync(NewCharacterEvent value)
        {
            Vector2 pos = Vector2.Zero;
            
            if (playerHandler.XorCode != null && value.PositionBytes != null)
            {
                var coords = playerHandler.Decrypt(value.PositionBytes);
                pos = new Vector2(coords[1], coords[0]);
            }
            else if (value.Position != Vector2.Zero)
            {
                pos = value.Position;
            }
            
            // Preencher Position no próprio evento para publicação reutilizável (IHasPosition)
            value.Position = pos;

            // Valores padrão para dados ainda não mapeados no evento
            var defaultHealth = new Health(100);
            var defaultFaction = Faction.NoPVP;
            var defaultEquipment = Array.Empty<int>();
            var defaultSpells = Array.Empty<int>();

            playerHandler.AddPlayer(value.Id, value.Name, value.GuildName, value.AllianceName, pos, defaultHealth, defaultFaction, defaultEquipment, defaultSpells);

            // Emitir evento para o EventDispatcher
            await eventDispatcher.DispatchEvent(value);

            // Demais lógicas de listas e alertas permanecem comentadas até implementação de configuração
        }
    }
}
