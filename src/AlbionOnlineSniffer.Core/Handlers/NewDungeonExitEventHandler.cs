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
    /// Handler para eventos NewDungeonExit do Albion Online
    /// Baseado no sistema do albion-radar-deatheye-2pc
    /// </summary>
    public class NewDungeonExitEventHandler
    {
        private readonly ILogger<NewDungeonExitEventHandler> _logger;
        private readonly PositionDecryptor _positionDecryptor;
        private readonly PacketOffsets _packetOffsets;

        public NewDungeonExitEventHandler(ILogger<NewDungeonExitEventHandler> logger, PositionDecryptor positionDecryptor, PacketOffsets packetOffsets)
        {
            _logger = logger;
            _positionDecryptor = positionDecryptor;
            _packetOffsets = packetOffsets;
        }

        /// <summary>
        /// Processa um evento NewDungeonExit
        /// </summary>
        /// <param name="parameters">Parâmetros do pacote</param>
        /// <returns>Dados do dungeon processados</returns>
        public async Task<Dungeon?> HandleNewDungeonExit(Dictionary<byte, object> parameters)
        {
            try
            {
                var offsets = _packetOffsets.NewDungeonExit;
                
                if (offsets.Length < 4)
                {
                    _logger.LogWarning("Offsets insuficientes para NewDungeonExit: {OffsetCount}", offsets.Length);
                    return null;
                }

                // Extrair dados usando offsets (baseado no albion-radar-deatheye-2pc)
                var id = Convert.ToInt32(parameters[offsets[0]]);
                
                var positionBytes = parameters[offsets[1]] as float[];
                Vector2 position = Vector2.Zero;
                if (positionBytes != null && positionBytes.Length >= 2)
                {
                    position = new Vector2(positionBytes[0], positionBytes[1]);
                }

                var type = parameters.ContainsKey(offsets[2]) ? parameters[offsets[2]] as string ?? "Unknown" : "Unknown";
                var charges = parameters.ContainsKey(offsets[3]) ? Convert.ToInt32(parameters[offsets[3]]) : 0;

                // Converter string para DungeonType enum
                DungeonType dungeonType = DungeonType.Group; // Default
                if (Enum.TryParse<DungeonType>(type, true, out var parsedType))
                {
                    dungeonType = parsedType;
                }

                var dungeon = new Dungeon(id, type, position, charges);

                _logger.LogInformation("Novo dungeon detectado: {Type} (ID: {Id}, Charges: {Charges}) em ({X}, {Y})", 
                    dungeonType, id, charges, position.X, position.Y);

                return dungeon;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar evento NewDungeonExit: {Message}", ex.Message);
                return null;
            }
        }
    }
} 