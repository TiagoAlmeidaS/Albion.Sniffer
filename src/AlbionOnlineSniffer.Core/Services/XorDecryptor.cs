using System;
using System.Numerics;
using Microsoft.Extensions.Logging;

namespace AlbionOnlineSniffer.Core.Services
{
    /// <summary>
    /// Serviço para decriptar posições usando algoritmo XOR
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class XorDecryptor
    {
        private readonly ILogger<XorDecryptor> _logger;
        private byte[]? _xorKey;

        public XorDecryptor(ILogger<XorDecryptor> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Define a chave XOR para decriptação
        /// </summary>
        /// <param name="key">Chave XOR</param>
        public void SetXorKey(byte[] key)
        {
            _xorKey = key;
            _logger.LogDebug("Chave XOR definida: {KeyLength} bytes", key.Length);
        }

        /// <summary>
        /// Verifica se a chave XOR está definida
        /// </summary>
        public bool HasXorKey => _xorKey != null && _xorKey.Length > 0;

        /// <summary>
        /// Decripta uma posição usando XOR
        /// </summary>
        /// <param name="encryptedData">Dados encriptados</param>
        /// <returns>Posição decriptada ou Vector2.Zero se falhar</returns>
        public Vector2 DecryptPosition(byte[] encryptedData)
        {
            if (_xorKey == null || encryptedData == null || encryptedData.Length < 8)
            {
                _logger.LogWarning("Não foi possível decriptar posição: chave XOR não definida ou dados inválidos");
                return Vector2.Zero;
            }

            try
            {
                // Aplicar XOR para decriptar
                var decryptedData = new byte[encryptedData.Length];
                for (int i = 0; i < encryptedData.Length; i++)
                {
                    decryptedData[i] = (byte)(encryptedData[i] ^ _xorKey[i % _xorKey.Length]);
                }

                // Extrair coordenadas (formato: float X, float Y)
                var x = BitConverter.ToSingle(decryptedData, 0);
                var y = BitConverter.ToSingle(decryptedData, 4);

                var position = new Vector2(x, y);
                _logger.LogDebug("Posição decriptada: ({X}, {Y})", position.X, position.Y);

                return position;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao decriptar posição: {Message}", ex.Message);
                return Vector2.Zero;
            }
        }

        /// <summary>
        /// Decripta posições de movimento (formato específico do albion-radar)
        /// </summary>
        /// <param name="positionBytes">Bytes da posição</param>
        /// <param name="newPositionBytes">Bytes da nova posição</param>
        /// <returns>Tupla com posição atual e nova posição</returns>
        public (Vector2 Position, Vector2 NewPosition) DecryptMovementPositions(byte[] positionBytes, byte[]? newPositionBytes = null)
        {
            try
            {
                var position = DecryptPosition(positionBytes);
                var newPosition = newPositionBytes != null ? DecryptPosition(newPositionBytes) : position;

                return (position, newPosition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao decriptar posições de movimento: {Message}", ex.Message);
                return (Vector2.Zero, Vector2.Zero);
            }
        }

        /// <summary>
        /// Limpa a chave XOR
        /// </summary>
        public void ClearXorKey()
        {
            _xorKey = null;
            _logger.LogDebug("Chave XOR limpa");
        }
    }
} 