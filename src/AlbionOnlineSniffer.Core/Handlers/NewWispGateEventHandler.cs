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
    /// Handler para eventos NewWispGate do Albion Online
    /// Baseado no sistema do albion-radar-deatheye-2pc
    /// </summary>
    public class NewWispGateEventHandler
    {
        private readonly ILogger<NewWispGateEventHandler> _logger;
        private readonly PositionDecryptor _positionDecryptor;
        private readonly PacketOffsets _packetOffsets;

        public NewWispGateEventHandler(ILogger<NewWispGateEventHandler> logger, PositionDecryptor positionDecryptor, PacketOffsets packetOffsets)
        {
            _logger = logger;
            _positionDecryptor = positionDecryptor;
            _packetOffsets = packetOffsets;
        }

        /// <summary>
        /// Processa um evento NewWispGate
        /// </summary>
        /// <param name="parameters">Parâmetros do pacote</param>
        /// <returns>Dados do gated wisp processados</returns>
        public async Task<GatedWisp?> HandleNewWispGate(Dictionary<byte, object> parameters)
        {
            try
            {
                var offsets = _packetOffsets.NewWispGate;
                
                if (offsets.Length < 3)
                {
                    _logger.LogWarning("Offsets insuficientes para NewWispGate: {OffsetCount}", offsets.Length);
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

                var isCollected = parameters.ContainsKey(offsets[2]) && parameters[offsets[2]].ToString() == "2";

                var gatedWisp = new GatedWisp(id, position);

                _logger.LogInformation("Novo gated wisp detectado: ID {Id} (Collected: {IsCollected}) em ({X}, {Y})", 
                    id, isCollected, position.X, position.Y);

                return gatedWisp;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar evento NewWispGate: {Message}", ex.Message);
                return null;
            }
        }
    }
} 