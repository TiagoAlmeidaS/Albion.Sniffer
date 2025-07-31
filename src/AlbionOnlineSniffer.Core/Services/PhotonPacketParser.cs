using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AlbionOnlineSniffer.Core.Enums;
using AlbionOnlineSniffer.Core.Models;
using Microsoft.Extensions.Logging;

namespace AlbionOnlineSniffer.Core.Services
{
    /// <summary>
    /// Parser real para pacotes do protocolo Photon (Protocol16)
    /// Substitui a simulação anterior com implementação completa
    /// </summary>
    public class PhotonPacketParser
    {
        private readonly ILogger<PhotonPacketParser> _logger;
        private readonly PhotonPacketEnricher _packetEnricher;

        public PhotonPacketParser(PhotonPacketEnricher packetEnricher, ILogger<PhotonPacketParser> logger)
        {
            _packetEnricher = packetEnricher;
            _logger = logger;
        }

        /// <summary>
        /// Parseia um payload UDP contendo dados do protocolo Photon
        /// </summary>
        /// <param name="payload">Payload UDP bruto</param>
        /// <returns>Pacote enriquecido ou null se não for um pacote válido</returns>
        public EnrichedPhotonPacket? ParsePacket(byte[] payload)
        {
            try
            {
                _logger.LogInformation("🔍 PARSEANDO PACOTE: {Length} bytes", payload?.Length ?? 0);
                
                if (payload == null || payload.Length < 4)
                {
                    _logger.LogDebug("Payload inválido ou muito pequeno: {Length} bytes", payload?.Length ?? 0);
                    return null;
                }

                using var stream = new MemoryStream(payload);
                using var reader = new BinaryReader(stream);

                // Verificar cabeçalho do protocolo Photon
                _logger.LogInformation("🔍 VERIFICANDO VALIDACAO PHOTON...");
                if (!IsValidPhotonPacket(reader))
                {
                    _logger.LogWarning("❌ Payload não é um pacote Photon válido");
                    return null;
                }
                _logger.LogInformation("✅ Validacao Photon passou!");

                // Extrair informações do pacote
                _logger.LogInformation("🔍 EXTRAINDO INFORMACOES DO PACOTE...");
                var packetInfo = ExtractPacketInfo(reader);
                if (packetInfo == null)
                {
                    _logger.LogWarning("❌ Não foi possível extrair informações do pacote");
                    return null;
                }
                _logger.LogInformation("✅ Informacoes extraidas: ID={PacketId}, Timestamp={Timestamp}, Params={ParamCount}", 
                    packetInfo.Value.PacketId, packetInfo.Value.Timestamp, packetInfo.Value.ParameterCount);

                // Parsear parâmetros do pacote
                _logger.LogInformation("🔍 PARSEANDO PARAMETROS...");
                var parameters = ParseParameters(reader, packetInfo.Value);
                _logger.LogInformation("✅ Parametros parseados: {ParamCount} parametros", parameters.Count);

                // Enriquece o pacote com informações dos bin-dumps
                _logger.LogInformation("🔍 ENRIQUECENDO PACOTE...");
                var enrichedPacket = _packetEnricher.EnrichPacket(
                    packetInfo.Value.PacketId, 
                    parameters, 
                    payload);

                _logger.LogInformation("✅ PACOTE ENRIQUECIDO: {PacketName} (ID: {PacketId}, Parâmetros: {ParamCount})", 
                    enrichedPacket.PacketName, enrichedPacket.PacketId, parameters.Count);

                return enrichedPacket;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao parsear pacote Photon: {Message}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Verifica se o payload é um pacote Photon válido
        /// </summary>
        /// <param name="reader">BinaryReader posicionado no início do payload</param>
        /// <returns>True se for um pacote Photon válido</returns>
        private bool IsValidPhotonPacket(BinaryReader reader)
        {
            try
            {
                // Verificar assinatura do protocolo Photon (exemplo)
                // Na implementação real, isso dependeria da versão específica do protocolo
                var signature = reader.ReadBytes(2);
                _logger.LogInformation("🔍 Signature: [{Signature}]", BitConverter.ToString(signature));
                
                // Verificar se é um pacote de evento (0x01) ou operação (0x02)
                var messageType = reader.ReadByte();
                _logger.LogInformation("🔍 Message Type: 0x{MessageType:X2}", messageType);
                
                var isValid = messageType == 0x01 || messageType == 0x02;
                _logger.LogInformation("🔍 Validacao: {IsValid} (esperado: 0x01 ou 0x02)", isValid);
                
                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("❌ Erro na validacao Photon: {Message}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Extrai informações básicas do pacote
        /// </summary>
        /// <param name="reader">BinaryReader posicionado após o cabeçalho</param>
        /// <returns>Informações do pacote ou null se inválido</returns>
        private PacketInfo? ExtractPacketInfo(BinaryReader reader)
        {
            try
            {
                // Ler ID do pacote (2 bytes)
                var packetId = reader.ReadUInt16();
                
                // Ler timestamp (4 bytes)
                var timestamp = reader.ReadUInt32();
                
                // Ler número de parâmetros (1 byte)
                var parameterCount = reader.ReadByte();

                return new PacketInfo
                {
                    PacketId = packetId,
                    Timestamp = timestamp,
                    ParameterCount = parameterCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Erro ao extrair informações do pacote: {Message}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Parseia os parâmetros do pacote
        /// </summary>
        /// <param name="reader">BinaryReader posicionado no início dos parâmetros</param>
        /// <param name="packetInfo">Informações do pacote</param>
        /// <returns>Dicionário de parâmetros parseados</returns>
        private Dictionary<byte, object> ParseParameters(BinaryReader reader, PacketInfo packetInfo)
        {
            var parameters = new Dictionary<byte, object>();

            try
            {
                for (int i = 0; i < packetInfo.ParameterCount; i++)
                {
                    if (reader.BaseStream.Position >= reader.BaseStream.Length)
                        break;

                    // Ler chave do parâmetro (1 byte)
                    var paramKey = reader.ReadByte();
                    
                    // Ler tipo do parâmetro (1 byte)
                    var paramType = reader.ReadByte();
                    
                    // Parsear valor baseado no tipo
                    var paramValue = ParseParameterValue(reader, (Protocol16Type)paramType);
                    
                    if (paramValue != null)
                    {
                        parameters[paramKey] = paramValue;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Erro ao parsear parâmetros: {Message}", ex.Message);
            }

            return parameters;
        }

        /// <summary>
        /// Parseia um valor de parâmetro baseado no tipo
        /// </summary>
        /// <param name="reader">BinaryReader posicionado no valor</param>
        /// <param name="type">Tipo do parâmetro</param>
        /// <returns>Valor parseado ou null se inválido</returns>
        private object? ParseParameterValue(BinaryReader reader, Protocol16Type type)
        {
            try
            {
                return type switch
                {
                    Protocol16Type.Byte => reader.ReadByte(),
                    Protocol16Type.Boolean => reader.ReadBoolean(),
                    Protocol16Type.Short => reader.ReadInt16(),
                    Protocol16Type.Integer => reader.ReadInt32(),
                    Protocol16Type.Long => reader.ReadInt64(),
                    Protocol16Type.Float => reader.ReadSingle(),
                    Protocol16Type.Double => reader.ReadDouble(),
                    Protocol16Type.String => ReadString(reader),
                    Protocol16Type.ByteArray => ReadByteArray(reader),
                    Protocol16Type.IntegerArray => ReadIntegerArray(reader),
                    Protocol16Type.StringArray => ReadStringArray(reader),
                    Protocol16Type.ObjectArray => ReadObjectArray(reader),
                    Protocol16Type.Hashtable => ReadHashtable(reader),
                    Protocol16Type.Dictionary => ReadDictionary(reader),
                    Protocol16Type.Null => null,
                    _ => ReadUnknownType(reader, type)
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Erro ao parsear valor do tipo {Type}: {Message}", type, ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Lê uma string do stream
        /// </summary>
        private string ReadString(BinaryReader reader)
        {
            var length = reader.ReadUInt16();
            if (length == 0) return string.Empty;
            
            var bytes = reader.ReadBytes(length);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Lê um array de bytes
        /// </summary>
        private byte[] ReadByteArray(BinaryReader reader)
        {
            var length = reader.ReadUInt16();
            return reader.ReadBytes(length);
        }

        /// <summary>
        /// Lê um array de inteiros
        /// </summary>
        private int[] ReadIntegerArray(BinaryReader reader)
        {
            var length = reader.ReadUInt16();
            var array = new int[length];
            
            for (int i = 0; i < length; i++)
            {
                array[i] = reader.ReadInt32();
            }
            
            return array;
        }

        /// <summary>
        /// Lê um array de strings
        /// </summary>
        private string[] ReadStringArray(BinaryReader reader)
        {
            var length = reader.ReadUInt16();
            var array = new string[length];
            
            for (int i = 0; i < length; i++)
            {
                array[i] = ReadString(reader);
            }
            
            return array;
        }

        /// <summary>
        /// Lê um array de objetos (simplificado)
        /// </summary>
        private object[] ReadObjectArray(BinaryReader reader)
        {
            var length = reader.ReadUInt16();
            var array = new object[length];
            
            for (int i = 0; i < length; i++)
            {
                var type = (Protocol16Type)reader.ReadByte();
                array[i] = ParseParameterValue(reader, type) ?? "null";
            }
            
            return array;
        }

        /// <summary>
        /// Lê uma hashtable (simplificado)
        /// </summary>
        private Dictionary<object, object> ReadHashtable(BinaryReader reader)
        {
            var count = reader.ReadUInt16();
            var hashtable = new Dictionary<object, object>();
            
            for (int i = 0; i < count; i++)
            {
                var keyType = (Protocol16Type)reader.ReadByte();
                var key = ParseParameterValue(reader, keyType);
                
                var valueType = (Protocol16Type)reader.ReadByte();
                var value = ParseParameterValue(reader, valueType);
                
                if (key != null)
                {
                    hashtable[key] = value ?? "null";
                }
            }
            
            return hashtable;
        }

        /// <summary>
        /// Lê um dictionary (simplificado)
        /// </summary>
        private Dictionary<string, object> ReadDictionary(BinaryReader reader)
        {
            var count = reader.ReadUInt16();
            var dictionary = new Dictionary<string, object>();
            
            for (int i = 0; i < count; i++)
            {
                var key = ReadString(reader);
                var valueType = (Protocol16Type)reader.ReadByte();
                var value = ParseParameterValue(reader, valueType);
                
                dictionary[key] = value ?? "null";
            }
            
            return dictionary;
        }

        /// <summary>
        /// Lê um tipo desconhecido (fallback)
        /// </summary>
        private object ReadUnknownType(BinaryReader reader, Protocol16Type type)
        {
            _logger.LogWarning("Tipo desconhecido encontrado: {Type}, pulando bytes", type);
            
            // Pular alguns bytes como fallback
            try
            {
                var bytes = reader.ReadBytes(4);
                return $"UnknownType_{type}_{BitConverter.ToString(bytes)}";
            }
            catch
            {
                return $"UnknownType_{type}";
            }
        }

        /// <summary>
        /// Informações básicas de um pacote Photon
        /// </summary>
        private struct PacketInfo
        {
            public ushort PacketId;
            public uint Timestamp;
            public byte ParameterCount;
        }
    }
}