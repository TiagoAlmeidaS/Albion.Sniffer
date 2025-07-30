using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Models;
using AlbionOnlineSniffer.Core.Models.GameObjects;
using AlbionOnlineSniffer.Core.Services;
using Microsoft.Extensions.Logging;

namespace AlbionOnlineSniffer.Core.Handlers
{
    /// <summary>
    /// Handler para eventos NewCharacter do Albion Online
    /// Baseado no sistema do albion-radar-deatheye-2pc
    /// </summary>
    public class NewCharacterEventHandler
    {
        private readonly ILogger<NewCharacterEventHandler> _logger;
        private readonly PositionDecryptor _positionDecryptor;
        private readonly PacketOffsets _packetOffsets;

        public NewCharacterEventHandler(ILogger<NewCharacterEventHandler> logger, PositionDecryptor positionDecryptor)
        {
            _logger = logger;
            _positionDecryptor = positionDecryptor;
            _packetOffsets = new PacketOffsets();
        }

        /// <summary>
        /// Processa um evento NewCharacter
        /// </summary>
        /// <param name="parameters">Parâmetros do pacote</param>
        /// <returns>Dados do jogador processados</returns>
        public async Task<Player?> HandleNewCharacter(Dictionary<byte, object> parameters)
        {
            try
            {
                var offsets = _packetOffsets.NewCharacter;
                
                // Extrair dados usando offsets (baseado no albion-radar-deatheye-2pc)
                var id = Convert.ToInt32(parameters[offsets[0]]);
                var name = parameters[offsets[1]] as string ?? string.Empty;
                var guild = parameters.ContainsKey(offsets[2]) ? parameters[offsets[2]] as string ?? string.Empty : string.Empty;
                var alliance = parameters.ContainsKey(offsets[3]) ? parameters[offsets[3]] as string ?? string.Empty : string.Empty;
                var faction = (Faction)parameters[offsets[4]];
                
                var encryptedPosition = parameters[offsets[5]] as byte[];
                var speed = parameters.ContainsKey(offsets[6]) ? (float)parameters[offsets[6]] : 5.5f;

                var health = parameters.ContainsKey(offsets[7]) ?
                    new Health(Convert.ToInt32(parameters[offsets[7]]), Convert.ToInt32(parameters[offsets[8]]))
                    : new Health(Convert.ToInt32(parameters[offsets[8]]));

                var equipments = ConvertArray(parameters[offsets[9]]);
                var spells = ConvertArray(parameters[offsets[10]]);

                // Decriptar posição
                Vector2 position = Vector2.Zero;
                if (encryptedPosition != null)
                {
                    position = _positionDecryptor.DecryptPosition(encryptedPosition);
                }

                // Criar equipamento
                var equipment = new Equipment();
                if (equipments != null)
                {
                    equipment.Items = new List<PlayerItem>();
                    foreach (var itemId in equipments)
                    {
                        equipment.Items.Add(new PlayerItem { Id = itemId, Name = $"Item_{itemId}", ItemPower = 0 });
                    }
                }

                var player = new Player(id, name, guild, alliance, position, health, faction, equipment, spells ?? Array.Empty<int>());

                _logger.LogInformation("Novo jogador detectado: {Name} (ID: {Id}, Guild: {Guild}, Faction: {Faction})", 
                    name, id, guild, faction);

                return player;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar evento NewCharacter: {Message}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Converte array de diferentes tipos para int[]
        /// Baseado no método ConvertArray do albion-radar-deatheye-2pc
        /// </summary>
        private int[]? ConvertArray(object value)
        {
            if (value == null) return null;

            return value switch
            {
                byte[] numArray2 => Array.ConvertAll(numArray2, b => (int)b),
                short[] numArray3 => Array.ConvertAll(numArray3, s => (int)s),
                int[] numArray1 => numArray1,
                _ => null
            };
        }
    }
} 