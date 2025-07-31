using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Models;
using AlbionOnlineSniffer.Core.Models.GameObjects;
using AlbionOnlineSniffer.Core.Services;
using Microsoft.Extensions.Logging;

namespace AlbionOnlineSniffer.Core.Handlers
{
    /// <summary>
    /// Handler para eventos WispGateOpened do Albion Online
    /// Baseado no sistema do albion-radar-deatheye-2pc
    /// </summary>
    public class WispGateOpenedEventHandler
    {
        private readonly ILogger<WispGateOpenedEventHandler> _logger;
        private readonly PacketOffsets _packetOffsets;

        public WispGateOpenedEventHandler(ILogger<WispGateOpenedEventHandler> logger, PacketOffsets packetOffsets)
        {
            _logger = logger;
            _packetOffsets = packetOffsets;
        }

        /// <summary>
        /// Processa um evento WispGateOpened
        /// </summary>
        /// <param name="parameters">Parâmetros do pacote</param>
        /// <returns>Dados do wisp gate opened processados</returns>
        public async Task<GatedWisp?> HandleWispGateOpened(Dictionary<byte, object> parameters)
        {
            try
            {
                var offsets = _packetOffsets.WispGateOpened;
                
                if (offsets.Length < 2)
                {
                    _logger.LogWarning("Offsets insuficientes para WispGateOpened: {OffsetCount}", offsets.Length);
                    return null;
                }

                // Extrair dados usando offsets (baseado no albion-radar-deatheye-2pc)
                var id = Convert.ToInt32(parameters[offsets[0]]);
                var isCollected = parameters.ContainsKey(offsets[1]) && parameters[offsets[1]].ToString() == "2";

                // Para WispGateOpened, não temos posição no pacote, então criamos um wisp com posição zero
                // A posição real seria obtida do wisp existente no manager
                var gatedWisp = new GatedWisp(id, System.Numerics.Vector2.Zero);

                _logger.LogInformation("Wisp Gate aberto: ID {Id} (Collected: {IsCollected})", id, isCollected);

                return gatedWisp;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar evento WispGateOpened: {Message}", ex.Message);
                return null;
            }
        }
    }
} 