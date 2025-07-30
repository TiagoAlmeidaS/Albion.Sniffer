using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace AlbionOnlineSniffer.Core.Services
{
    /// <summary>
    /// Serviço responsável por carregar e gerenciar as definições dos bin-dumps do Albion Online.
    /// Permite mapear IDs de pacotes para nomes legíveis e parâmetros.
    /// </summary>
    public class PhotonDefinitionLoader
    {
        private readonly ILogger<PhotonDefinitionLoader> _logger;
        
        /// <summary>
        /// Mapeamento de ID do pacote para nome legível
        /// </summary>
        public Dictionary<int, string> PacketIdToName { get; private set; } = new();
        
        /// <summary>
        /// Mapeamento de ID do pacote para parâmetros (chave do parâmetro -> nome legível)
        /// </summary>
        public Dictionary<int, Dictionary<byte, string>> PacketParameterMap { get; private set; } = new();
        
        /// <summary>
        /// Mapeamento de IDs de enum para valores legíveis
        /// </summary>
        public Dictionary<int, Dictionary<int, string>> EnumValueMap { get; private set; } = new();

        public PhotonDefinitionLoader(ILogger<PhotonDefinitionLoader> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Carrega as definições dos bin-dumps a partir do diretório especificado
        /// </summary>
        /// <param name="basePath">Caminho para o diretório dos bin-dumps</param>
        public void Load(string basePath)
        {
            try
            {
                _logger.LogInformation("Carregando definições dos bin-dumps de: {BasePath}", basePath);
                
                // Carregar eventos (se existir)
                LoadEvents(basePath);
                
                // Carregar enums (se existir)
                LoadEnums(basePath);
                
                _logger.LogInformation("Definições carregadas: {PacketCount} pacotes, {EnumCount} enums", 
                    PacketIdToName.Count, EnumValueMap.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar definições dos bin-dumps: {Message}", ex.Message);
                throw;
            }
        }

        private void LoadEvents(string basePath)
        {
            var eventsPath = Path.Combine(basePath, "events.json");
            if (!File.Exists(eventsPath))
            {
                _logger.LogWarning("Arquivo events.json não encontrado em: {Path}", eventsPath);
                return;
            }

            try
            {
                var eventsJson = File.ReadAllText(eventsPath);
                var events = JsonSerializer.Deserialize<List<EventDefinition>>(eventsJson);
                
                if (events != null)
                {
                    foreach (var evt in events)
                    {
                        PacketIdToName[evt.Id] = evt.Name;
                        
                        var paramMap = new Dictionary<byte, string>();
                        if (evt.Parameters != null)
                        {
                            foreach (var param in evt.Parameters)
                            {
                                if (byte.TryParse(param.Key, out var paramKey))
                                {
                                    paramMap[paramKey] = param.Value;
                                }
                            }
                        }
                        PacketParameterMap[evt.Id] = paramMap;
                    }
                    
                    _logger.LogInformation("Carregados {EventCount} eventos", events.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar eventos: {Message}", ex.Message);
            }
        }

        private void LoadEnums(string basePath)
        {
            var enumsPath = Path.Combine(basePath, "enums.json");
            if (!File.Exists(enumsPath))
            {
                _logger.LogWarning("Arquivo enums.json não encontrado em: {Path}", enumsPath);
                return;
            }

            try
            {
                var enumsJson = File.ReadAllText(enumsPath);
                var enums = JsonSerializer.Deserialize<List<EnumDefinition>>(enumsJson);
                
                if (enums != null)
                {
                    foreach (var enumDef in enums)
                    {
                        var valueMap = new Dictionary<int, string>();
                        if (enumDef.Values != null)
                        {
                            foreach (var value in enumDef.Values)
                            {
                                if (int.TryParse(value.Key, out var enumValue))
                                {
                                    valueMap[enumValue] = value.Value;
                                }
                            }
                        }
                        EnumValueMap[enumDef.Id] = valueMap;
                    }
                    
                    _logger.LogInformation("Carregados {EnumCount} enums", enums.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar enums: {Message}", ex.Message);
            }
        }

        /// <summary>
        /// Obtém o nome legível de um pacote pelo seu ID
        /// </summary>
        /// <param name="packetId">ID do pacote</param>
        /// <returns>Nome legível ou "UnknownPacket_{packetId}" se não encontrado</returns>
        public string GetPacketName(int packetId)
        {
            return PacketIdToName.TryGetValue(packetId, out var name) ? name : $"UnknownPacket_{packetId}";
        }

        /// <summary>
        /// Obtém o nome legível de um parâmetro de pacote
        /// </summary>
        /// <param name="packetId">ID do pacote</param>
        /// <param name="paramKey">Chave do parâmetro</param>
        /// <returns>Nome legível ou "param_{paramKey}" se não encontrado</returns>
        public string GetParameterName(int packetId, byte paramKey)
        {
            if (PacketParameterMap.TryGetValue(packetId, out var paramMap))
            {
                return paramMap.TryGetValue(paramKey, out var name) ? name : $"param_{paramKey}";
            }
            return $"param_{paramKey}";
        }

        /// <summary>
        /// Obtém o valor legível de um enum
        /// </summary>
        /// <param name="enumId">ID do enum</param>
        /// <param name="enumValue">Valor do enum</param>
        /// <returns>Nome legível ou "UnknownEnum_{enumValue}" se não encontrado</returns>
        public string GetEnumValueName(int enumId, int enumValue)
        {
            if (EnumValueMap.TryGetValue(enumId, out var valueMap))
            {
                return valueMap.TryGetValue(enumValue, out var name) ? name : $"UnknownEnum_{enumValue}";
            }
            return $"UnknownEnum_{enumValue}";
        }

        /// <summary>
        /// Verifica se um pacote é conhecido
        /// </summary>
        /// <param name="packetId">ID do pacote</param>
        /// <returns>True se o pacote é conhecido</returns>
        public bool IsKnownPacket(int packetId)
        {
            return PacketIdToName.ContainsKey(packetId);
        }
    }

    /// <summary>
    /// Definição de um evento do protocolo Photon
    /// </summary>
    public class EventDefinition
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, string>? Parameters { get; set; }
    }

    /// <summary>
    /// Definição de um enum do protocolo Photon
    /// </summary>
    public class EnumDefinition
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, string>? Values { get; set; }
    }
}