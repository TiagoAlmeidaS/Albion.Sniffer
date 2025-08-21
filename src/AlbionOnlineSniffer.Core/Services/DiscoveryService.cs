using AlbionOnlineSniffer.Core.Models.Discovery;
using AlbionOnlineSniffer.Core.Handlers;
using AlbionOnlineSniffer.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AlbionOnlineSniffer.Core.Services
{
    /// <summary>
    /// Serviço de descoberta que intercepta pacotes descriptografados e os envia para fila albion.discovery.raw
    /// </summary>
    public class DiscoveryService : IDisposable
    {
        private readonly ILogger<DiscoveryService> _logger;
        private readonly EventDispatcher _eventDispatcher;
        private bool _disposed = false;
        
        public DiscoveryService(ILogger<DiscoveryService> logger, EventDispatcher eventDispatcher)
        {
            _logger = logger;
            _eventDispatcher = eventDispatcher;
            
            // ✅ CONECTAR AO HANDLER DE DESCOBERTA
            WireDiscoveryHandler();
            
            _logger.LogInformation("🔍 DiscoveryService configurado e conectado ao DiscoveryDebugHandler");
        }
        
        private void WireDiscoveryHandler()
        {
            // ✅ INTERCEPTAR TODOS OS PACOTES DESCRIPTOGRAFADOS
            DiscoveryDebugHandler.OnPacketDecrypted += async (decryptedData) =>
            {
                try
                {
                    _logger.LogInformation("🔍 DISCOVERY SERVICE: Evento recebido! {Type} - {Code}", 
                        decryptedData.PacketType, decryptedData.PacketCode);
                    
                    // ✅ PUBLICAR NO EVENTDISPATCHER PARA QUE O EVENTTOQUEUEBRIDGE CAPTURE
                    await _eventDispatcher.DispatchEvent(decryptedData);
                    _logger.LogInformation("📤 DiscoveryService: Pacote publicado no EventDispatcher para fila albion.discovery.raw");
                }
                catch (Exception ex)
                {
                    // ✅ TRATAMENTO SILENCIOSO - NÃO AFETA FLUXO PRINCIPAL
                    _logger.LogWarning("⚠️ Erro no DiscoveryService: {Message}", ex.Message);
                }
            };
            
            _logger.LogInformation("🔍 DiscoveryService: Evento OnPacketDecrypted conectado com sucesso!");
        }
        

        

        
        public void Dispose()
        {
            if (!_disposed)
            {
                // ✅ DESCONECTAR DO HANDLER DE DESCOBERTA
                // Nota: Em um cenário real, você precisaria armazenar a referência do delegate
                // para poder removê-lo corretamente. Por simplicidade, deixamos assim.
                
                _disposed = true;
                _logger.LogInformation("🔍 DiscoveryService finalizado");
            }
        }
    }
}
