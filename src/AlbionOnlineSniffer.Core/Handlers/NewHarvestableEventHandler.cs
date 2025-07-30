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
    /// Handler para eventos NewHarvestable do Albion Online
    /// Baseado no sistema do albion-radar-deatheye-2pc
    /// </summary>
    public class NewHarvestableEventHandler
    {
        private readonly ILogger<NewHarvestableEventHandler> _logger;
        private readonly PositionDecryptor _positionDecryptor;
        private readonly PacketOffsets _packetOffsets;

        public NewHarvestableEventHandler(ILogger<NewHarvestableEventHandler> logger, PositionDecryptor positionDecryptor)
        {
            _logger = logger;
            _positionDecryptor = positionDecryptor;
            _packetOffsets = new PacketOffsets();
        }

        /// <summary>
        /// Processa um evento NewHarvestable
        /// </summary>
        /// <param name="parameters">Parâmetros do pacote</param>
        /// <returns>Dados do harvestable processados</returns>
        public async Task<Harvestable?> HandleNewHarvestable(Dictionary<byte, object> parameters)
        {
            try
            {
                var offsets = _packetOffsets.NewHarvestableObject;
                
                // Extrair dados usando offsets (baseado no albion-radar-deatheye-2pc)
                var id = Convert.ToInt32(parameters[offsets[0]]);
                var type = Convert.ToInt32(parameters[offsets[1]]);
                var tier = Convert.ToInt32(parameters[offsets[2]]);
                
                var positionBytes = parameters[offsets[3]] as float[];
                Vector2 position = Vector2.Zero;
                if (positionBytes != null && positionBytes.Length >= 2)
                {
                    position = new Vector2(positionBytes[0], positionBytes[1]);
                }

                var count = parameters.ContainsKey(offsets[4]) ? Convert.ToInt32(parameters[offsets[4]]) : 0;
                var charge = parameters.ContainsKey(offsets[5]) ? Convert.ToInt32(parameters[offsets[5]]) : 0;

                // Converter tipo para string (pode ser enriquecido com dados dos bin-dumps)
                var typeString = GetHarvestableType(type);

                var harvestable = new Harvestable(id, typeString, tier, position, count, charge);

                _logger.LogInformation("Novo harvestable detectado: {Type} T{Level} (ID: {Id}, Count: {Count})", 
                    typeString, tier, id, count);

                return harvestable;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar evento NewHarvestable: {Message}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Converte ID do tipo para string legível
        /// </summary>
        private string GetHarvestableType(int typeId)
        {
            return typeId switch
            {
                1 => "FIBER",
                2 => "HIDE",
                3 => "ORE",
                4 => "WOOD",
                5 => "STONE",
                _ => $"UNKNOWN_{typeId}"
            };
        }
    }
} 