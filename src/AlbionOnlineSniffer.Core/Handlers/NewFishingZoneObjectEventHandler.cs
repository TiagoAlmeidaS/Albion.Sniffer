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
    /// Handler para eventos NewFishingZoneObject do Albion Online
    /// Baseado no sistema do albion-radar-deatheye-2pc
    /// </summary>
    public class NewFishingZoneObjectEventHandler
    {
        private readonly ILogger<NewFishingZoneObjectEventHandler> _logger;
        private readonly PositionDecryptor _positionDecryptor;
        private readonly PacketOffsets _packetOffsets;

        public NewFishingZoneObjectEventHandler(ILogger<NewFishingZoneObjectEventHandler> logger, PositionDecryptor positionDecryptor, PacketOffsets packetOffsets)
        {
            _logger = logger;
            _positionDecryptor = positionDecryptor;
            _packetOffsets = packetOffsets;
        }

        /// <summary>
        /// Processa um evento NewFishingZoneObject
        /// </summary>
        /// <param name="parameters">Parâmetros do pacote</param>
        /// <returns>Dados do fish node processados</returns>
        public async Task<FishNode?> HandleNewFishingZoneObject(Dictionary<byte, object> parameters)
        {
            try
            {
                var offsets = _packetOffsets.NewFishingZoneObject;
                
                if (offsets.Length < 4)
                {
                    _logger.LogWarning("Offsets insuficientes para NewFishingZoneObject: {OffsetCount}", offsets.Length);
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

                var size = parameters.ContainsKey(offsets[2]) ? Convert.ToInt32(parameters[offsets[2]]) : 0;
                var respawnCount = parameters.ContainsKey(offsets[3]) ? Convert.ToInt32(parameters[offsets[3]]) : 0;

                var fishNode = new FishNode(id, position, size, respawnCount);

                _logger.LogInformation("Nova zona de pesca detectada: ID {Id} (Size: {Size}, Respawn: {Respawn}) em ({X}, {Y})", 
                    id, size, respawnCount, position.X, position.Y);

                return fishNode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar evento NewFishingZoneObject: {Message}", ex.Message);
                return null;
            }
        }
    }
} 