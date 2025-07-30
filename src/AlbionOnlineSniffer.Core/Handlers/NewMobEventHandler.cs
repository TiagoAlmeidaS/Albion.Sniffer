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
    /// Handler para eventos NewMob do Albion Online
    /// Baseado no sistema do albion-radar-deatheye-2pc
    /// </summary>
    public class NewMobEventHandler
    {
        private readonly ILogger<NewMobEventHandler> _logger;
        private readonly PositionDecryptor _positionDecryptor;
        private readonly PacketOffsets _packetOffsets;

        public NewMobEventHandler(ILogger<NewMobEventHandler> logger, PositionDecryptor positionDecryptor)
        {
            _logger = logger;
            _positionDecryptor = positionDecryptor;
            _packetOffsets = new PacketOffsets();
        }

        /// <summary>
        /// Processa um evento NewMob
        /// </summary>
        /// <param name="parameters">Parâmetros do pacote</param>
        /// <returns>Dados do mob processados</returns>
        public async Task<Mob?> HandleNewMob(Dictionary<byte, object> parameters)
        {
            try
            {
                var offsets = _packetOffsets.NewMobEvent;
                
                // Extrair dados usando offsets (baseado no albion-radar-deatheye-2pc)
                var id = Convert.ToInt32(parameters[offsets[0]]);
                var typeId = Convert.ToInt32(parameters[offsets[1]]) - 15; // Subtrai 15 como no albion-radar-deatheye-2pc
                
                var positionBytes = parameters[offsets[2]] as float[];
                Vector2 position = Vector2.Zero;
                if (positionBytes != null && positionBytes.Length >= 2)
                {
                    position = new Vector2(positionBytes[0], positionBytes[1]);
                }

                var health = parameters.ContainsKey(offsets[3]) ?
                    new Health(Convert.ToInt32(parameters[offsets[3]]), Convert.ToInt32(parameters[offsets[4]]))
                    : new Health(Convert.ToInt32(parameters[offsets[4]]));

                var charge = (byte)(parameters.ContainsKey(offsets[5]) ? Convert.ToInt32(parameters[offsets[5]]) : 0);

                // Criar MobInfo básico (pode ser enriquecido com dados dos bin-dumps)
                var mobInfo = new MobInfo
                {
                    Id = typeId,
                    Tier = 1, // Pode ser extraído de dados adicionais
                    Type = "UNKNOWN",
                    MobName = $"Mob_{typeId}"
                };

                var mob = new Mob(id, typeId, position, charge, mobInfo, health);

                _logger.LogInformation("Novo mob detectado: {MobName} (ID: {Id}, TypeId: {TypeId})", 
                    mobInfo.MobName, id, typeId);

                return mob;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar evento NewMob: {Message}", ex.Message);
                return null;
            }
        }
    }
} 