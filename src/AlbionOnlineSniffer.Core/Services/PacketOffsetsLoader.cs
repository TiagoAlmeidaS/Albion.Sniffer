using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Models;

namespace AlbionOnlineSniffer.Core.Services
{
    /// <summary>
    /// Serviço responsável por carregar os offsets dos pacotes do JSON
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class PacketOffsetsLoader
    {
        private readonly ILogger<PacketOffsetsLoader> _logger;

        public PacketOffsetsLoader(ILogger<PacketOffsetsLoader> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Propriedade estática para acesso global aos offsets
        /// Baseado no padrão do albion-radar-deatheye-2pc
        /// </summary>
        public static PacketOffsets? GlobalPacketOffsets { get; private set; }

        /// <summary>
        /// Carrega os offsets do arquivo JSON
        /// </summary>
        /// <param name="jsonPath">Caminho para o arquivo offsets.json</param>
        /// <returns>PacketOffsets carregado</returns>
        public PacketOffsets LoadOffsets(string jsonPath)
        {
            try
            {
                if (!File.Exists(jsonPath))
                {
                    _logger.LogWarning("Arquivo offsets.json não encontrado em: {Path}", jsonPath);
                    return new PacketOffsets();
                }

                var jsonContent = File.ReadAllText(jsonPath);
                // Deserializar primeiro como Dictionary para converter arrays de int para byte[]
                var tempDict = JsonSerializer.Deserialize<Dictionary<string, int[]>>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (tempDict == null)
                {
                    _logger.LogWarning("Falha ao deserializar offsets.json");
                    return new PacketOffsets();
                }

                // Converter para PacketOffsets
                var offsets = new PacketOffsets();
                foreach (var kvp in tempDict)
                {
                    var property = typeof(PacketOffsets).GetProperty(kvp.Key);
                    if (property != null)
                    {
                        var byteArray = kvp.Value.Select(x => (byte)x).ToArray();
                        property.SetValue(offsets, byteArray);
                    }
                }
                
                if (offsets != null)
                {
                    _logger.LogInformation("Offsets carregados com sucesso de: {Path}", jsonPath);
                    GlobalPacketOffsets = offsets; // Atualizar a propriedade estática
                    _logger.LogInformation("✅ GlobalPacketOffsets atualizado com {Count} tipos de eventos", 
                        typeof(PacketOffsets).GetProperties().Length);
                    return offsets;
                }
                
                _logger.LogWarning("Falha ao deserializar offsets.json");
                return new PacketOffsets();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar offsets: {Message}", ex.Message);
                return new PacketOffsets();
            }
        }
    }
} 