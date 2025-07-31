using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Models;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Queue.Interfaces;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace AlbionOnlineSniffer.Tests.Integration
{
    /// <summary>
    /// Template de teste de integração para validar o fluxo completo do Sniffer
    /// Este template serve como modelo para testes reais e validação do sistema
    /// </summary>
    public class SnifferIntegrationTestTemplate
    {
        private readonly ITestOutputHelper _output;
        private readonly ILogger<SnifferIntegrationTestTemplate> _logger;

        public SnifferIntegrationTestTemplate(ITestOutputHelper output)
        {
            _output = output;
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            _logger = loggerFactory.CreateLogger<SnifferIntegrationTestTemplate>();
        }

        /// <summary>
        /// Template de dados de teste representando um pacote Photon típico do Albion Online
        /// Baseado nos padrões observados no projeto albion-radar-deatheye-2pc
        /// </summary>
        public static class TestPacketTemplates
        {
            /// <summary>
            /// Pacote simulado de movimento de jogador (NewCharacter Event)
            /// </summary>
            public static byte[] NewCharacterPacket => new byte[]
            {
                // Cabeçalho Photon simulado
                0xF0, 0x01, // Assinatura
                0x01,       // Tipo: Event
                0x01, 0x00, // Packet ID: 1 (NewCharacter)
                0x00, 0x00, 0x00, 0x01, // Timestamp
                0x03,       // Número de parâmetros: 3
                
                // Parâmetro 1: CharacterId (key=1, type=Integer, value=12345)
                0x01,       // Key
                0x69,       // Type: Integer
                0x39, 0x30, 0x00, 0x00, // Value: 12345
                
                // Parâmetro 2: Name (key=2, type=String, value="TestPlayer")
                0x02,       // Key
                0x73,       // Type: String
                0x0A, 0x00, // Length: 10
                0x54, 0x65, 0x73, 0x74, 0x50, 0x6C, 0x61, 0x79, 0x65, 0x72, // "TestPlayer"
                
                // Parâmetro 3: Position (key=3, type=Array, value=[100.5, 200.3])
                0x03,       // Key
                0x79,       // Type: Array
                0x02, 0x00, // Array length: 2
                0x66,       // Element type: Float
                0x00, 0x00, 0xC9, 0x42, // 100.5f
                0x66,       // Element type: Float
                0x9A, 0x99, 0x48, 0x43  // 200.3f
            };

            /// <summary>
            /// Pacote simulado de morte de mob (MobKilled Event)
            /// </summary>
            public static byte[] MobKilledPacket => new byte[]
            {
                // Cabeçalho Photon simulado
                0xF0, 0x01, // Assinatura
                0x01,       // Tipo: Event
                0x15, 0x00, // Packet ID: 21 (MobKilled)
                0x00, 0x00, 0x00, 0x02, // Timestamp
                0x02,       // Número de parâmetros: 2
                
                // Parâmetro 1: MobId (key=1, type=Integer, value=67890)
                0x01,       // Key
                0x69,       // Type: Integer
                0x32, 0x09, 0x01, 0x00, // Value: 67890
                
                // Parâmetro 2: KillerId (key=2, type=Integer, value=12345)
                0x02,       // Key
                0x69,       // Type: Integer
                0x39, 0x30, 0x00, 0x00  // Value: 12345
            };

            /// <summary>
            /// Pacote inválido para testar tratamento de erros
            /// </summary>
            public static byte[] InvalidPacket => new byte[]
            {
                0x00, 0x01, 0x02 // Dados insuficientes
            };

            /// <summary>
            /// Pacote com estrutura Photon válida mas ID desconhecido
            /// </summary>
            public static byte[] UnknownPacket => new byte[]
            {
                // Cabeçalho Photon simulado
                0xF0, 0x01, // Assinatura
                0x01,       // Tipo: Event
                0xFF, 0xFF, // Packet ID: 65535 (desconhecido)
                0x00, 0x00, 0x00, 0x03, // Timestamp
                0x01,       // Número de parâmetros: 1
                
                // Parâmetro 1: Unknown (key=255, type=Integer, value=999)
                0xFF,       // Key
                0x69,       // Type: Integer
                0xE7, 0x03, 0x00, 0x00  // Value: 999
            };
        }

        /// <summary>
        /// Template de resultados esperados para validação
        /// </summary>
        public static class ExpectedResults
        {
            public static EnrichedPhotonPacket NewCharacterExpected => new EnrichedPhotonPacket(1, "NewCharacter", true)
            {
                Parameters = new Dictionary<string, object>
                {
                    { "CharacterId", 12345 },
                    { "Name", "TestPlayer" },
                    { "Position", new float[] { 100.5f, 200.3f } }
                }
            };

            public static EnrichedPhotonPacket MobKilledExpected => new EnrichedPhotonPacket(21, "MobKilled", true)
            {
                Parameters = new Dictionary<string, object>
                {
                    { "MobId", 67890 },
                    { "KillerId", 12345 }
                }
            };
        }

        /// <summary>
        /// Mock de publisher para capturar mensagens enviadas para a fila
        /// </summary>
        public class TestQueuePublisher : IQueuePublisher
        {
            public List<string> PublishedMessages { get; } = new List<string>();
            public List<EnrichedPhotonPacket> PublishedPackets { get; } = new List<EnrichedPhotonPacket>();

            public async Task PublishAsync(string topic, object message)
            {
                var messageStr = message?.ToString() ?? "";
                PublishedMessages.Add(messageStr);
                
                // Tentar deserializar como EnrichedPhotonPacket
                try
                {
                    var packet = JsonSerializer.Deserialize<EnrichedPhotonPacket>(messageStr);
                    if (packet != null)
                    {
                        PublishedPackets.Add(packet);
                    }
                }
                catch
                {
                    // Ignorar erros de deserialização para este teste
                }
                
                await Task.CompletedTask;
            }

            public void Dispose()
            {
                // Cleanup se necessário
            }
        }

        /// <summary>
        /// Teste de integração completo: Captura → Parser → Enriquecimento → Fila
        /// </summary>
        [Fact]
        public async Task CompleteIntegrationFlow_ShouldProcessPacketsCorrectly()
        {
            // Arrange
            var testPublisher = new TestQueuePublisher();
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            
            var definitionLoader = new PhotonDefinitionLoader(
                loggerFactory.CreateLogger<PhotonDefinitionLoader>());
            
            var packetEnricher = new PhotonPacketEnricher(
                definitionLoader,
                loggerFactory.CreateLogger<PhotonPacketEnricher>());
            
            var packetParser = new PhotonPacketParser(
                packetEnricher,
                loggerFactory.CreateLogger<PhotonPacketParser>());

            var receivedPackets = new List<EnrichedPhotonPacket>();
            var packetProcessor = new PacketProcessor(testPublisher);

            // Configurar captura de pacotes processados
            packetProcessor.OnPacketProcessed += packet => receivedPackets.Add(packet);

            // Act & Assert - Teste com pacote NewCharacter
            _output.WriteLine("=== Testando pacote NewCharacter ===");
            var newCharPacket = packetParser.ParsePacket(TestPacketTemplates.NewCharacterPacket);
            
            Assert.NotNull(newCharPacket);
            Assert.Equal(1, newCharPacket.PacketId);
            Assert.True(newCharPacket.IsKnownPacket);
            Assert.Contains("CharacterId", newCharPacket.Parameters.Keys);
            
            await packetProcessor.ProcessPacketAsync(newCharPacket);
            
            // Verificar se foi publicado na fila
            Assert.Single(testPublisher.PublishedPackets);
            Assert.Equal(1, testPublisher.PublishedPackets[0].PacketId);

            // Act & Assert - Teste com pacote MobKilled
            _output.WriteLine("=== Testando pacote MobKilled ===");
            var mobKilledPacket = packetParser.ParsePacket(TestPacketTemplates.MobKilledPacket);
            
            Assert.NotNull(mobKilledPacket);
            Assert.Equal(21, mobKilledPacket.PacketId);
            Assert.True(mobKilledPacket.IsKnownPacket);
            
            await packetProcessor.ProcessPacketAsync(mobKilledPacket);
            
            // Verificar se ambos foram publicados
            Assert.Equal(2, testPublisher.PublishedPackets.Count);

            // Act & Assert - Teste com pacote inválido
            _output.WriteLine("=== Testando pacote inválido ===");
            var invalidPacket = packetParser.ParsePacket(TestPacketTemplates.InvalidPacket);
            
            Assert.Null(invalidPacket); // Deve retornar null para pacotes inválidos

            // Act & Assert - Teste com pacote desconhecido
            _output.WriteLine("=== Testando pacote desconhecido ===");
            var unknownPacket = packetParser.ParsePacket(TestPacketTemplates.UnknownPacket);
            
            Assert.NotNull(unknownPacket);
            Assert.Equal(65535, unknownPacket.PacketId);
            Assert.False(unknownPacket.IsKnownPacket); // Deve marcar como desconhecido

            _output.WriteLine($"✅ Teste de integração completo finalizado. Pacotes processados: {testPublisher.PublishedPackets.Count}");
        }

        /// <summary>
        /// Teste de performance: Processar múltiplos pacotes rapidamente
        /// </summary>
        [Fact]
        public async Task PerformanceTest_ShouldProcessMultiplePacketsQuickly()
        {
            // Arrange
            var testPublisher = new TestQueuePublisher();
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            
            var definitionLoader = new PhotonDefinitionLoader(
                loggerFactory.CreateLogger<PhotonDefinitionLoader>());
            
            var packetEnricher = new PhotonPacketEnricher(
                definitionLoader,
                loggerFactory.CreateLogger<PhotonPacketEnricher>());
            
            var packetParser = new PhotonPacketParser(
                packetEnricher,
                loggerFactory.CreateLogger<PhotonPacketParser>());

            var packetProcessor = new PacketProcessor(testPublisher);
            
            const int packetCount = 1000;
            var startTime = DateTime.UtcNow;

            // Act
            _output.WriteLine($"=== Teste de performance: processando {packetCount} pacotes ===");
            
            for (int i = 0; i < packetCount; i++)
            {
                var packet = packetParser.ParsePacket(TestPacketTemplates.NewCharacterPacket);
                if (packet != null)
                {
                    await packetProcessor.ProcessPacketAsync(packet);
                }
            }

            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;
            var packetsPerSecond = packetCount / duration.TotalSeconds;

            // Assert
            Assert.Equal(packetCount, testPublisher.PublishedPackets.Count);
            Assert.True(packetsPerSecond > 100, $"Performance insuficiente: {packetsPerSecond:F2} pacotes/segundo");
            
            _output.WriteLine($"✅ Performance test: {packetsPerSecond:F2} pacotes/segundo");
            _output.WriteLine($"✅ Tempo total: {duration.TotalMilliseconds:F2}ms");
        }

        /// <summary>
        /// Teste de stress: Verificar comportamento sob carga
        /// </summary>
        [Fact]
        public async Task StressTest_ShouldHandleHighVolumeGracefully()
        {
            // Arrange
            var testPublisher = new TestQueuePublisher();
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            
            var definitionLoader = new PhotonDefinitionLoader(
                loggerFactory.CreateLogger<PhotonDefinitionLoader>());
            
            var packetEnricher = new PhotonPacketEnricher(
                definitionLoader,
                loggerFactory.CreateLogger<PhotonPacketEnricher>());
            
            var packetParser = new PhotonPacketParser(
                packetEnricher,
                loggerFactory.CreateLogger<PhotonPacketParser>());

            var packetProcessor = new PacketProcessor(testPublisher);

            const int threadCount = 10;
            const int packetsPerThread = 100;
            var tasks = new List<Task>();
            var exceptions = new List<Exception>();

            // Act
            _output.WriteLine($"=== Teste de stress: {threadCount} threads, {packetsPerThread} pacotes cada ===");
            
            for (int t = 0; t < threadCount; t++)
            {
                var task = Task.Run(async () =>
                {
                    try
                    {
                        for (int i = 0; i < packetsPerThread; i++)
                        {
                            var packetTemplate = i % 2 == 0 
                                ? TestPacketTemplates.NewCharacterPacket 
                                : TestPacketTemplates.MobKilledPacket;
                                
                            var packet = packetParser.ParsePacket(packetTemplate);
                            if (packet != null)
                            {
                                await packetProcessor.ProcessPacketAsync(packet);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        lock (exceptions)
                        {
                            exceptions.Add(ex);
                        }
                    }
                });
                
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            // Assert
            Assert.Empty(exceptions); // Não deve haver exceções
            Assert.Equal(threadCount * packetsPerThread, testPublisher.PublishedPackets.Count);
            
            _output.WriteLine($"✅ Stress test concluído: {testPublisher.PublishedPackets.Count} pacotes processados");
        }
    }

    /// <summary>
    /// Processador de pacotes simulado para testes
    /// </summary>
    public class PacketProcessor
    {
        private readonly IQueuePublisher _publisher;
        
        public event Action<EnrichedPhotonPacket>? OnPacketProcessed;

        public PacketProcessor(IQueuePublisher publisher)
        {
            _publisher = publisher;
        }

        public async Task ProcessPacketAsync(EnrichedPhotonPacket packet)
        {
            // Simular processamento
            await Task.Delay(1); // Simular latência mínima
            
            // Serializar e publicar
            var json = JsonSerializer.Serialize(packet, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            await _publisher.PublishAsync("albion_packets", json);
            
            // Notificar processamento
            OnPacketProcessed?.Invoke(packet);
        }
    }
}