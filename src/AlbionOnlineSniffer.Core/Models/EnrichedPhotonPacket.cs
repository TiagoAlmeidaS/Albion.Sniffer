using System;
using System.Collections.Generic;

namespace AlbionOnlineSniffer.Core.Models
{
    /// <summary>
    /// Representa um pacote Photon enriquecido com informações dos bin-dumps
    /// </summary>
    public class EnrichedPhotonPacket
    {
        /// <summary>
        /// ID original do pacote
        /// </summary>
        public int PacketId { get; set; }
        
        /// <summary>
        /// Nome legível do pacote (ex: "NewCharacter", "MobKilled")
        /// </summary>
        public string PacketName { get; set; } = string.Empty;
        
        /// <summary>
        /// Parâmetros do pacote com nomes legíveis
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new();
        
        /// <summary>
        /// Timestamp de quando o pacote foi processado
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Indica se o pacote é conhecido (existe nos bin-dumps)
        /// </summary>
        public bool IsKnownPacket { get; set; }
        
        /// <summary>
        /// Dados brutos do pacote (opcional, para debug)
        /// </summary>
        public byte[]? RawData { get; set; }
        
        /// <summary>
        /// Cria uma instância de EnrichedPhotonPacket
        /// </summary>
        public EnrichedPhotonPacket(int packetId, string packetName, bool isKnownPacket = true)
        {
            PacketId = packetId;
            PacketName = packetName;
            IsKnownPacket = isKnownPacket;
            Timestamp = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Adiciona um parâmetro ao pacote
        /// </summary>
        /// <param name="name">Nome do parâmetro</param>
        /// <param name="value">Valor do parâmetro</param>
        public void AddParameter(string name, object value)
        {
            Parameters[name] = value;
        }
        
        /// <summary>
        /// Converte o pacote para um objeto anônimo para serialização
        /// </summary>
        /// <returns>Objeto anônimo com os dados do pacote</returns>
        public object ToSerializableObject()
        {
            return new
            {
                type = PacketName,
                packetId = PacketId,
                isKnownPacket = IsKnownPacket,
                data = Parameters,
                timestamp = Timestamp.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };
        }
    }
}