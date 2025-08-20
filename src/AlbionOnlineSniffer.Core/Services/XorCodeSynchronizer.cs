using System;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Models.GameObjects.Players;
using AlbionOnlineSniffer.Core.Contracts.Transformers;

namespace AlbionOnlineSniffer.Core.Services
{
    /// <summary>
    /// Sincroniza o código XOR entre PlayersHandler e PositionDecryptionService
    /// para garantir que as posições sejam descriptografadas corretamente nos contratos V1
    /// </summary>
    public class XorCodeSynchronizer
    {
        private readonly ILogger<XorCodeSynchronizer> _logger;
        private readonly PlayersHandler _playersHandler;
        private readonly PositionDecryptionService _positionDecryptionService;

        public XorCodeSynchronizer(
            ILogger<XorCodeSynchronizer> logger,
            PlayersHandler playersHandler,
            PositionDecryptionService positionDecryptionService)
        {
            _logger = logger;
            _playersHandler = playersHandler;
            _positionDecryptionService = positionDecryptionService;
            
            _logger.LogDebug("XorCodeSynchronizer inicializado. PlayersHandler.XorCode: {Length} bytes, PositionDecryptionService.IsConfigured: {IsConfigured}", 
                _playersHandler.XorCode?.Length ?? 0, 
                _positionDecryptionService.IsConfigured);
            
            // Não sincronizar automaticamente no construtor - aguardar KeySyncEvent
            // SyncXorCode();
        }

        /// <summary>
        /// Sincroniza o código XOR do PlayersHandler para o PositionDecryptionService
        /// </summary>
        public void SyncXorCode()
        {
            try
            {
                var xorCode = _playersHandler.XorCode;
                _logger.LogDebug("Tentando sincronizar código XOR. PlayersHandler.XorCode: {Length} bytes", xorCode?.Length ?? 0);
                
                if (xorCode != null && xorCode.Length > 0)
                {
                    _positionDecryptionService.SetXorCode(xorCode);
                    _logger.LogInformation("✅ Código XOR sincronizado com sucesso: {XorCodeLength} bytes", xorCode.Length);
                    
                    // Verificar se a sincronização foi bem-sucedida
                    var isConfigured = _positionDecryptionService.IsConfigured;
                    _logger.LogDebug("PositionDecryptionService.IsConfigured após sincronização: {IsConfigured}", isConfigured);
                }
                else
                {
                    _logger.LogDebug("❌ Código XOR não disponível para sincronização");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao sincronizar código XOR: {Message}", ex.Message);
            }
        }

        /// <summary>
        /// Atualiza o código XOR no PlayersHandler e sincroniza com PositionDecryptionService
        /// </summary>
        public void UpdateXorCode(byte[] xorCode)
        {
            try
            {
                _playersHandler.XorCode = xorCode;
                _positionDecryptionService.SetXorCode(xorCode);
                _logger.LogInformation("Código XOR atualizado e sincronizado: {XorCodeLength} bytes", xorCode?.Length ?? 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar código XOR: {Message}", ex.Message);
            }
        }

        /// <summary>
        /// Verifica se o código XOR está sincronizado
        /// </summary>
        public bool IsXorCodeSynchronized()
        {
            var playersHandlerCode = _playersHandler.XorCode;
            var positionServiceConfigured = _positionDecryptionService.IsConfigured;
            
            var isSynchronized = playersHandlerCode != null && 
                   playersHandlerCode.Length > 0 && 
                   positionServiceConfigured;
            
            _logger.LogDebug("Verificação de sincronização XOR: PlayersHandler.XorCode={Length} bytes, PositionDecryptionService.IsConfigured={IsConfigured}, Resultado={IsSynchronized}", 
                playersHandlerCode?.Length ?? 0, 
                positionServiceConfigured, 
                isSynchronized);
            
            return isSynchronized;
        }
    }
}
