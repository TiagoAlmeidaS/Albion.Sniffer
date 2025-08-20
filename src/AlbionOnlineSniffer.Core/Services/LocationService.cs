using System.Numerics;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Contracts.Transformers;

namespace AlbionOnlineSniffer.Core.Services
{
    /// <summary>
    /// Serviço centralizado para gerenciamento de localização e descriptografia de posições
    /// </summary>
    public class LocationService
    {
        private readonly ILogger<LocationService> _logger;
        private readonly XorCodeSynchronizer _xorSynchronizer;
        private readonly PositionDecryptionService _positionDecryptionService;

        public LocationService(
            ILogger<LocationService> logger,
            XorCodeSynchronizer xorSynchronizer,
            PositionDecryptionService positionDecryptionService)
        {
            _logger = logger;
            _xorSynchronizer = xorSynchronizer;
            _positionDecryptionService = positionDecryptionService;
        }

        /// <summary>
        /// Descriptografa posição de bytes criptografados
        /// </summary>
        /// <param name="positionBytes">Bytes da posição criptografada</param>
        /// <param name="fallbackPosition">Posição de fallback se descriptografia falhar</param>
        /// <returns>Posição descriptografada ou fallback</returns>
        public Vector2 DecryptPosition(byte[]? positionBytes, Vector2? fallbackPosition = null)
        {
            try
            {
                // Se não há bytes de posição, usar fallback
                if (positionBytes == null || positionBytes.Length < 8)
                {
                    _logger.LogDebug("Bytes de posição inválidos para descriptografia, usando fallback");
                    return fallbackPosition ?? Vector2.Zero;
                }

                // Garantir que o código XOR está sincronizado
                if (!_xorSynchronizer.IsXorCodeSynchronized())
                {
                    _logger.LogDebug("Código XOR não sincronizado, tentando sincronizar...");
                    _xorSynchronizer.SyncXorCode();
                    
                    // Verificar novamente após sincronização
                    if (!_xorSynchronizer.IsXorCodeSynchronized())
                    {
                        _logger.LogWarning("Falha na sincronização do código XOR, usando fallback");
                        return fallbackPosition ?? Vector2.Zero;
                    }
                }

                _logger.LogDebug("Descriptografando posição com {Length} bytes", positionBytes.Length);
                
                // Descriptografar usando o serviço de descriptografia
                var decryptedPosition = _positionDecryptionService.DecryptPosition(positionBytes);
                
                _logger.LogDebug("Resultado da descriptografia: X={X}, Y={Y}", decryptedPosition.X, decryptedPosition.Y);
                
                // ✅ SEMPRE retornar a posição descriptografada, mesmo se for Vector2.Zero
                // (pode ser uma posição válida em (0,0))
                return decryptedPosition;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao descriptografar posição: {Message}", ex.Message);
                return fallbackPosition ?? Vector2.Zero;
            }
        }

        /// <summary>
        /// Converte bytes de posição para Vector2 sem descriptografia (para casos onde não há criptografia)
        /// </summary>
        /// <param name="positionBytes">Bytes da posição</param>
        /// <returns>Posição convertida</returns>
        public Vector2 ConvertPositionBytes(byte[]? positionBytes)
        {
            if (positionBytes == null || positionBytes.Length < 8)
            {
                return Vector2.Zero;
            }

            try
            {
                // Coordenadas estão em formato: [X1][X2][X3][X4][Y1][Y2][Y3][Y4]
                var x = BitConverter.ToSingle(positionBytes, 0);
                var y = BitConverter.ToSingle(positionBytes, 4);
                
                // Validar valores float para evitar Infinity/NaN
                x = ValidateFloat(x);
                y = ValidateFloat(y);
                
                return new Vector2(x, y);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao converter bytes de posição: {Message}", ex.Message);
                return Vector2.Zero;
            }
        }

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

        /// <summary>
        /// Processo inteligente de posição: tenta descriptografar, se falhar usa conversão direta
        /// </summary>
        /// <param name="positionBytes">Bytes da posição</param>
        /// <param name="fallbackPosition">Posição de fallback</param>
        /// <returns>Melhor posição disponível</returns>
        public Vector2 ProcessPosition(byte[]? positionBytes, Vector2? fallbackPosition = null)
        {
            if (positionBytes == null || positionBytes.Length < 8)
            {
                _logger.LogDebug("Bytes de posição inválidos, usando fallback");
                return fallbackPosition ?? Vector2.Zero;
            }

            // 🔑 PRIORIDADE: Se há código XOR disponível, SEMPRE tentar descriptografar primeiro
            if (_xorSynchronizer.IsXorCodeSynchronized())
            {
                _logger.LogDebug("Código XOR sincronizado, tentando descriptografar posição");
                var decryptedPosition = DecryptPosition(positionBytes, fallbackPosition);
                
                // ✅ SEMPRE retornar a posição descriptografada, mesmo se for Vector2.Zero
                // (pode ser uma posição válida em (0,0))
                _logger.LogDebug("Posição descriptografada: X={X}, Y={Y}", decryptedPosition.X, decryptedPosition.Y);
                return decryptedPosition;
            }
            else
            {
                _logger.LogDebug("Código XOR não sincronizado, usando conversão direta");
            }

            // Fallback: conversão direta dos bytes (apenas se não há descriptografia disponível)
            var directPosition = ConvertPositionBytes(positionBytes);
            if (directPosition != Vector2.Zero)
            {
                _logger.LogDebug("Usando posição convertida diretamente (sem descriptografia): X={X}, Y={Y}", directPosition.X, directPosition.Y);
                return directPosition;
            }

            // Último recurso: fallback fornecido
            _logger.LogDebug("Usando posição fallback: X={X}, Y={Y}", fallbackPosition?.X ?? 0, fallbackPosition?.Y ?? 0);
            return fallbackPosition ?? Vector2.Zero;
        }

        /// <summary>
        /// Verifica se o serviço está configurado para descriptografia
        /// </summary>
        public bool IsDecryptionAvailable => _xorSynchronizer.IsXorCodeSynchronized();

        /// <summary>
        /// Força sincronização do código XOR
        /// </summary>
        public void ForceSyncXorCode()
        {
            try
            {
                _xorSynchronizer.SyncXorCode();
                _logger.LogInformation("Sincronização do código XOR forçada");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao forçar sincronização do código XOR: {Message}", ex.Message);
            }
        }

        /// <summary>
        /// Atualiza o código XOR manualmente
        /// </summary>
        /// <param name="xorCode">Novo código XOR</param>
        public void UpdateXorCode(byte[] xorCode)
        {
            try
            {
                _xorSynchronizer.UpdateXorCode(xorCode);
                _logger.LogInformation("Código XOR atualizado manualmente: {Length} bytes", xorCode?.Length ?? 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar código XOR: {Message}", ex.Message);
            }
        }
    }
}
