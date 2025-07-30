using System;
using System.Collections.Generic;
using AlbionOnlineSniffer.Core.Models;
using Microsoft.Extensions.Logging;

namespace AlbionOnlineSniffer.Core.Services
{
    /// <summary>
    /// Serviço responsável por enriquecer pacotes Photon com informações dos bin-dumps
    /// </summary>
    public class PhotonPacketEnricher
    {
        private readonly PhotonDefinitionLoader _definitionLoader;
        private readonly ILogger<PhotonPacketEnricher> _logger;

        public PhotonPacketEnricher(PhotonDefinitionLoader definitionLoader, ILogger<PhotonPacketEnricher> logger)
        {
            _definitionLoader = definitionLoader;
            _logger = logger;
        }

        /// <summary>
        /// Enriquece um pacote Photon com informações dos bin-dumps
        /// </summary>
        /// <param name="packetId">ID do pacote</param>
        /// <param name="parameters">Parâmetros brutos do pacote</param>
        /// <param name="rawData">Dados brutos do pacote (opcional)</param>
        /// <returns>Pacote enriquecido com nomes legíveis</returns>
        public EnrichedPhotonPacket EnrichPacket(int packetId, Dictionary<byte, object> parameters, byte[]? rawData = null)
        {
            try
            {
                // Obter nome legível do pacote
                var packetName = _definitionLoader.GetPacketName(packetId);
                var isKnownPacket = _definitionLoader.IsKnownPacket(packetId);

                // Criar pacote enriquecido
                var enrichedPacket = new EnrichedPhotonPacket(packetId, packetName, isKnownPacket)
                {
                    RawData = rawData
                };

                // Processar parâmetros
                foreach (var kv in parameters)
                {
                    var paramKey = kv.Key;
                    var paramValue = kv.Value;
                    
                    // Obter nome legível do parâmetro
                    var readableName = _definitionLoader.GetParameterName(packetId, paramKey);
                    
                    // Adicionar parâmetro enriquecido
                    enrichedPacket.AddParameter(readableName, paramValue);
                }

                _logger.LogDebug("Pacote enriquecido: {PacketName} (ID: {PacketId}, Parâmetros: {ParamCount})", 
                    packetName, packetId, parameters.Count);

                return enrichedPacket;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enriquecer pacote {PacketId}: {Message}", packetId, ex.Message);
                
                // Retornar pacote básico em caso de erro
                return new EnrichedPhotonPacket(packetId, $"ErrorPacket_{packetId}", false)
                {
                    RawData = rawData
                };
            }
        }

        /// <summary>
        /// Enriquece um valor de enum com informações dos bin-dumps
        /// </summary>
        /// <param name="enumId">ID do enum</param>
        /// <param name="enumValue">Valor do enum</param>
        /// <returns>Nome legível do valor do enum</returns>
        public string EnrichEnumValue(int enumId, int enumValue)
        {
            return _definitionLoader.GetEnumValueName(enumId, enumValue);
        }

        /// <summary>
        /// Verifica se um pacote é conhecido nos bin-dumps
        /// </summary>
        /// <param name="packetId">ID do pacote</param>
        /// <returns>True se o pacote é conhecido</returns>
        public bool IsKnownPacket(int packetId)
        {
            return _definitionLoader.IsKnownPacket(packetId);
        }

        /// <summary>
        /// Obtém estatísticas sobre os pacotes processados
        /// </summary>
        /// <returns>Dicionário com estatísticas</returns>
        public Dictionary<string, object> GetStatistics()
        {
            return new Dictionary<string, object>
            {
                ["TotalKnownPackets"] = _definitionLoader.PacketIdToName.Count,
                ["TotalEnums"] = _definitionLoader.EnumValueMap.Count,
                ["DefinitionLoaderLoaded"] = _definitionLoader.PacketIdToName.Count > 0
            };
        }
    }
}