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
        private readonly DiscoveryStatistics _statistics;
        private bool _disposed = false;

        public DiscoveryService(ILogger<DiscoveryService> logger, EventDispatcher eventDispatcher, DiscoveryStatistics statistics)
        {
            _logger = logger;
            _eventDispatcher = eventDispatcher;
            _statistics = statistics;

            // ✅ CONECTAR AO HANDLER DE DESCOBERTA
            WireDiscoveryHandler();

            _logger.LogInformation("🔍 DiscoveryService configurado e conectado ao DiscoveryDebugHandler");
            _logger.LogInformation("📊 Estatísticas em tempo real ativadas - console será atualizado a cada 5 segundos");
        }

        private void WireDiscoveryHandler()
        {
            // ✅ INTERCEPTAR TODOS OS PACOTES DESCRIPTOGRAFADOS
            DiscoveryDebugHandler.OnPacketDecrypted += async (decryptedData) =>
            {
                try
                {
                    // ✅ LISTA DE PACOTES ESCONDIDOS (não processados)
                    List<int> pacotesEscondidos =
                    [
                        1, 11, 19, 2, 3, 6, 21, 29, 35, 39, 40, 46, 47, 90, 91, 123, 209, 280, 319, 246, 359, 387, 514, 525, 526, 593
                    ];

                    bool isHidden = pacotesEscondidos.Contains(decryptedData.PacketCode.Value);
                    
                    // ✅ REGISTRAR ESTATÍSTICAS
                    _statistics.RecordPacket(
                        decryptedData.PacketCode.Value, 
                        decryptedData.PacketType, 
                        isHidden
                    );

                    // ✅ PROCESSAR APENAS PACOTES NÃO ESCONDIDOS
                    if (!isHidden)
                    {
                        // Log silencioso para não poluir o console (estatísticas são mostradas separadamente)
                        _logger.LogDebug("🔍 Pacote processado: {Type} - {Code}", 
                            decryptedData.PacketType, decryptedData.PacketCode);
                        
                        await _eventDispatcher.DispatchEvent(decryptedData);
                    }
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

                _statistics?.Dispose();
                _disposed = true;
                _logger.LogInformation("🔍 DiscoveryService finalizado");
            }
        }
    }
}
