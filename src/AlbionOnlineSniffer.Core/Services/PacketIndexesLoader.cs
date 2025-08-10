using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Models;
using AlbionOnlineSniffer.Core.Models.ResponseObj;

namespace AlbionOnlineSniffer.Core.Services
{
    /// <summary>
    /// Carregador de índices de pacotes do arquivo JSON
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class PacketIndexesLoader
    {
        private readonly ILogger<PacketIndexesLoader> _logger;

        public PacketIndexesLoader(ILogger<PacketIndexesLoader> logger)
        {
            _logger = logger;
        }

        // Construtor auxiliar sem logger para cenários de teste sem provider de logging
        public PacketIndexesLoader()
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<PacketIndexesLoader>.Instance;
        }

        public static PacketIndexes? GlobalPacketIndexes { get; private set; }

        /// <summary>
        /// Carrega os índices de pacotes do arquivo JSON
        /// </summary>
        /// <param name="filePath">Caminho para o arquivo indexes.json</param>
        /// <returns>Instância de PacketIndexes carregada</returns>
        public PacketIndexes LoadIndexes(string filePath)
        {
            try
            {
                _logger.LogInformation("📂 Carregando PacketIndexes de: {FilePath}", filePath);

                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("⚠️ Arquivo de índices não encontrado: {FilePath}. Usando valores padrão.", filePath);
                    var empty = new PacketIndexes();
                    GlobalPacketIndexes = empty;
                    return empty;
                }

                var jsonContent = File.ReadAllText(filePath);
                var indexes = JsonSerializer.Deserialize<PacketIndexes>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                GlobalPacketIndexes =  indexes;

                if (indexes == null)
                {
                    _logger.LogError("❌ Falha ao deserializar PacketIndexes");
                    throw new InvalidOperationException("Falha ao deserializar PacketIndexes");
                }

                _logger.LogInformation("✅ PacketIndexes carregado com sucesso: {IndexesCount} índices", 
                    typeof(PacketIndexes).GetProperties().Length);

                return indexes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao carregar PacketIndexes: {Message}", ex.Message);
                throw;
            }
        }
    }
} 