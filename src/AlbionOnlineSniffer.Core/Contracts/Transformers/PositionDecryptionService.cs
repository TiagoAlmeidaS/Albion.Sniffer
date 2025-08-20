using System;
using System.Numerics;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace AlbionOnlineSniffer.Core.Contracts.Transformers
{
    /// <summary>
    /// Serviço para descriptografar posições dos eventos Core antes de transformar para V1
    /// </summary>
    public class PositionDecryptionService
    {
        private readonly ILogger<PositionDecryptionService> _logger;
        private byte[]? _xorCode;

        public PositionDecryptionService(ILogger<PositionDecryptionService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Define o código XOR para descriptografia
        /// </summary>
        public void SetXorCode(byte[]? xorCode)
        {
            _xorCode = xorCode;
            if (xorCode != null)
            {
                _logger.LogDebug("Código XOR definido para descriptografia de posições");
            }
        }

        /// <summary>
        /// Descriptografa posição de bytes criptografados
        /// </summary>
        public Vector2 DecryptPosition(byte[]? positionBytes, int offset = 0)
        {
            if (positionBytes == null || positionBytes.Length < offset + 8)
            {
                _logger.LogDebug("Bytes de posição inválidos para descriptografia");
                return Vector2.Zero;
            }

            try
            {
                _logger.LogDebug("Descriptografando posição: {Length} bytes, offset: {Offset}", positionBytes.Length, offset);
                
                // Se não há código XOR, retorna coordenadas sem descriptografia
                if (_xorCode == null || _xorCode.Length == 0)
                {
                    _logger.LogDebug("Sem código XOR, retornando coordenadas sem descriptografia");
                    var x = BitConverter.ToSingle(positionBytes, offset);
                    var y = BitConverter.ToSingle(positionBytes, offset + 4);
                    var result = new Vector2(x, y);
                    _logger.LogDebug("Coordenadas sem descriptografia: X={X}, Y={Y}", result.X, result.Y);
                    return result;
                }

                _logger.LogDebug("Usando código XOR de {Length} bytes para descriptografia", _xorCode.Length);

                // Extrair coordenadas X e Y
                var xBytes = positionBytes.Skip(offset).Take(4).ToArray();
                var yBytes = positionBytes.Skip(offset + 4).Take(4).ToArray();

                _logger.LogDebug("Bytes X: [{Bytes}], Bytes Y: [{Bytes}]", 
                    string.Join(",", xBytes.Select(b => $"0x{b:X2}")),
                    string.Join(",", yBytes.Select(b => $"0x{b:X2}")));

                // Aplicar descriptografia XOR
                DecryptBytes(xBytes, _xorCode, 0);
                DecryptBytes(yBytes, _xorCode, 4);

                _logger.LogDebug("Bytes descriptografados X: [{Bytes}], Y: [{Bytes}]", 
                    string.Join(",", xBytes.Select(b => $"0x{b:X2}")),
                    string.Join(",", yBytes.Select(b => $"0x{b:X2}")));

                // Converter para float
                var coordX = BitConverter.ToSingle(xBytes, 0);
                var coordY = BitConverter.ToSingle(yBytes, 0);

                _logger.LogDebug("Coordenadas float brutas: X={X}, Y={Y}", coordX, coordY);

                // Validar valores float para evitar Infinity/NaN
                coordX = ValidateFloat(coordX);
                coordY = ValidateFloat(coordY);

                var finalResult = new Vector2(coordX, coordY);
                _logger.LogDebug("Posição descriptografada final: X={X}, Y={Y}", finalResult.X, finalResult.Y);
                
                return finalResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao descriptografar posição: {Message}", ex.Message);
                return Vector2.Zero;
            }
        }

        /// <summary>
        /// Descriptografa bytes usando XOR
        /// </summary>
        private static void DecryptBytes(byte[] bytes, byte[] saltBytes, int saltPos)
        {
            if (saltBytes == null || saltBytes.Length <= saltPos)
            {
                return;
            }

            for (var i = 0; i < bytes.Length; i++)
            {
                var modulo = saltBytes.Length - saltPos;
                if (modulo <= 0) break;
                var saltIndex = i % modulo + saltPos;
                bytes[i] ^= saltBytes[saltIndex];
            }
        }

        /// <summary>
        /// Verifica se o serviço está configurado com código XOR
        /// </summary>
        public bool IsConfigured => _xorCode != null && _xorCode.Length > 0;

        /// <summary>
        /// Valida float e substitui valores inválidos por 0
        /// </summary>
        private static float ValidateFloat(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                return 0f;
            }
            return value;
        }
    }
}
