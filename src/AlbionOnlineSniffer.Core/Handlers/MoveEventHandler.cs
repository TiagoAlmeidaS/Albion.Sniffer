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
    /// Handler para eventos Move do Albion Online
    /// Baseado no sistema do albion-radar-deatheye-2pc
    /// </summary>
    public class MoveEventHandler
    {
        private readonly ILogger<MoveEventHandler> _logger;
        private readonly PositionDecryptor _positionDecryptor;
        private readonly PacketOffsets _packetOffsets;

        public MoveEventHandler(ILogger<MoveEventHandler> logger, PositionDecryptor positionDecryptor, PacketOffsets packetOffsets)
        {
            _logger = logger;
            _positionDecryptor = positionDecryptor;
            _packetOffsets = packetOffsets;
        }

        /// <summary>
        /// Processa um evento Move
        /// </summary>
        /// <param name="parameters">Parâmetros do pacote</param>
        /// <returns>Dados de movimento processados</returns>
        public async Task<MoveData?> HandleMove(Dictionary<byte, object> parameters)
        {
            try
            {
                var offsets = _packetOffsets.Move;
                
                if (offsets.Length < 2)
                {
                    _logger.LogWarning("Offsets insuficientes para Move: {OffsetCount}", offsets.Length);
                    return null;
                }

                // Extrair dados usando offsets (baseado no albion-radar-deatheye-2pc)
                var id = Convert.ToInt32(parameters[offsets[0]]);
                var positionBytes = parameters[offsets[1]] as byte[];

                Vector2 position = Vector2.Zero;
                if (positionBytes != null)
                {
                    position = _positionDecryptor.DecryptPosition(positionBytes);
                }

                var moveData = new MoveData
                {
                    PlayerId = id,
                    Position = position,
                    Timestamp = DateTime.UtcNow
                };

                _logger.LogDebug("Movimento detectado: Player ID {PlayerId} -> Posição ({X}, {Y})", 
                    id, position.X, position.Y);

                return moveData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar evento Move: {Message}", ex.Message);
                return null;
            }
        }
    }

    /// <summary>
    /// Dados de movimento de um jogador
    /// </summary>
    public class MoveData
    {
        public int PlayerId { get; set; }
        public Vector2 Position { get; set; }
        public DateTime Timestamp { get; set; }
    }
} 