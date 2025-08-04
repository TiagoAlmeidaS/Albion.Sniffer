using System;
using System.Linq;
using System.Numerics;
using Microsoft.Extensions.Logging;

namespace Albion.Sniffer.Core.Services
{
    /// <summary>
    /// Serviço para decriptar posições de coordenadas do Albion Online
    /// Baseado no sistema de decriptação XOR do albion-radar-deatheye-2pc
    /// </summary>
    public class PositionDecryptor
    {
        private readonly ILogger<PositionDecryptor> _logger;
        private byte[]? _xorCode;

        public PositionDecryptor(ILogger<PositionDecryptor> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Define o código XOR para decriptação
        /// </summary>
        /// <param name="code">Código XOR de 8 bytes</param>
        public void SetXorCode(byte[] code)
        {
            if (code == null || code.Length != 8)
            {
                _logger.LogWarning("Código XOR inválido. Deve ter exatamente 8 bytes.");
                return;
            }

            _xorCode = code;
            _logger.LogDebug("Código XOR definido: {XorCode}", BitConverter.ToString(code));
        }

        /// <summary>
        /// Decripta coordenadas usando XOR
        /// </summary>
        /// <param name="coordinates">Coordenadas criptografadas</param>
        /// <param name="offset">Offset inicial (padrão: 0)</param>
        /// <returns>Coordenadas decriptadas como Vector2</returns>
        public Vector2 DecryptPosition(byte[] coordinates, int offset = 0)
        {
            try
            {
                if (coordinates == null || coordinates.Length < offset + 8)
                {
                    _logger.LogWarning("Coordenadas inválidas ou insuficientes para decriptação");
                    return Vector2.Zero;
                }

                // Se não há código XOR, retorna coordenadas sem decriptação
                if (_xorCode == null)
                {
                    var x = BitConverter.ToSingle(coordinates, offset);
                    var y = BitConverter.ToSingle(coordinates, offset + 4);
                    return new Vector2(x, y);
                }

                // Extrair coordenadas X e Y
                var xBytes = coordinates.Skip(offset).Take(4).ToArray();
                var yBytes = coordinates.Skip(offset + 4).Take(4).ToArray();

                // Aplicar decriptação XOR
                DecryptBytes(xBytes, _xorCode, 0);
                DecryptBytes(yBytes, _xorCode, 4);

                // Converter para float
                var coordX = BitConverter.ToSingle(xBytes, 0);
                var coordY = BitConverter.ToSingle(yBytes, 0);

                _logger.LogDebug("Posição decriptada: X={X}, Y={Y}", coordX, coordY);
                return new Vector2(coordX, coordY);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao decriptar posição: {Message}", ex.Message);
                return Vector2.Zero;
            }
        }

        /// <summary>
        /// Decripta um array de bytes usando XOR
        /// </summary>
        /// <param name="bytes">Bytes a serem decriptados</param>
        /// <param name="saltBytes">Bytes do salt (código XOR)</param>
        /// <param name="saltPos">Posição inicial no salt</param>
        private static void DecryptBytes(byte[] bytes, byte[] saltBytes, int saltPos)
        {
            for (var i = 0; i < bytes.Length; i++)
            {
                var saltIndex = i % (saltBytes.Length - saltPos) + saltPos;
                bytes[i] ^= saltBytes[saltIndex];
            }
        }

        /// <summary>
        /// Verifica se o decriptador está configurado
        /// </summary>
        public bool IsConfigured => _xorCode != null;

        /// <summary>
        /// Obtém o código XOR atual (para debug)
        /// </summary>
        public byte[]? GetXorCode() => _xorCode?.Clone() as byte[];
    }
} 