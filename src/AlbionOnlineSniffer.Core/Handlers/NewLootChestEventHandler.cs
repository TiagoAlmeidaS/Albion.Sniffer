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
    /// Handler para eventos NewLootChest do Albion Online
    /// Baseado no sistema do albion-radar-deatheye-2pc
    /// </summary>
    public class NewLootChestEventHandler
    {
        private readonly ILogger<NewLootChestEventHandler> _logger;
        private readonly PositionDecryptor _positionDecryptor;
        private readonly PacketOffsets _packetOffsets;

        public NewLootChestEventHandler(ILogger<NewLootChestEventHandler> logger, PositionDecryptor positionDecryptor)
        {
            _logger = logger;
            _positionDecryptor = positionDecryptor;
            _packetOffsets = new PacketOffsets();
        }

        /// <summary>
        /// Processa um evento NewLootChest
        /// </summary>
        /// <param name="parameters">Parâmetros do pacote</param>
        /// <returns>Dados do baú processados</returns>
        public async Task<LootChest?> HandleNewLootChest(Dictionary<byte, object> parameters)
        {
            try
            {
                var offsets = _packetOffsets.NewLootChest;
                
                // Extrair dados usando offsets (baseado no albion-radar-deatheye-2pc)
                var id = Convert.ToInt32(parameters[offsets[0]]);
                
                var positionBytes = parameters[offsets[1]] as float[];
                Vector2 position = Vector2.Zero;
                if (positionBytes != null && positionBytes.Length >= 2)
                {
                    position = new Vector2(positionBytes[0], positionBytes[1]);
                }

                var name = parameters[offsets[2]] as string ?? "Unknown Chest";
                var charge = parameters.ContainsKey(offsets[3]) ? Convert.ToInt32(parameters[offsets[3]]) : 0;

                var lootChest = new LootChest(id, position, name, charge);

                _logger.LogInformation("Novo baú detectado: {Name} (ID: {Id}, Charge: {Charge})", 
                    name, id, charge);

                return lootChest;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar evento NewLootChest: {Message}", ex.Message);
                return null;
            }
        }
    }
} 