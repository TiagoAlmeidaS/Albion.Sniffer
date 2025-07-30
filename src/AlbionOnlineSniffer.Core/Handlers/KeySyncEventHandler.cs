using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Handlers
{
    /// <summary>
    /// Handler para eventos de sincronização de chave XOR
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class KeySyncEventHandler
    {
        private readonly ILogger<KeySyncEventHandler> _logger;
        private readonly XorDecryptor _xorDecryptor;

        public KeySyncEventHandler(ILogger<KeySyncEventHandler> logger, XorDecryptor xorDecryptor)
        {
            _logger = logger;
            _xorDecryptor = xorDecryptor;
        }

        /// <summary>
        /// Processa evento de sincronização de chave XOR
        /// </summary>
        /// <param name="keySyncEvent">Evento de sincronização de chave</param>
        /// <returns>Task</returns>
        public async Task HandleAsync(KeySyncEvent keySyncEvent)
        {
            try
            {
                if (keySyncEvent.Code != null && keySyncEvent.Code.Length > 0)
                {
                    _xorDecryptor.SetXorKey(keySyncEvent.Code);
                    
                    _logger.LogInformation("🔑 Chave XOR sincronizada: {KeyLength} bytes", keySyncEvent.Code.Length);
                    
                    // Log dos primeiros bytes da chave para debug (sem expor a chave completa)
                    var preview = BitConverter.ToString(keySyncEvent.Code.Take(Math.Min(4, keySyncEvent.Code.Length)).ToArray());
                    _logger.LogDebug("Chave XOR preview: {Preview}...", preview);
                }
                else
                {
                    _logger.LogWarning("Chave XOR recebida é nula ou vazia");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar KeySyncEvent: {Message}", ex.Message);
            }

            await Task.CompletedTask;
        }
    }
} 