using System;
using System.Collections.Generic;
using System.IO;
using AlbionOnlineSniffer.Core.Models;
using AlbionOnlineSniffer.Core.Services;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AlbionOnlineSniffer.Tests.Core
{
    public class PhotonPacketParserTests
    {
        private readonly ILogger<PhotonPacketParser> _logger;
        private readonly ILogger<PhotonPacketEnricher> _enricherLogger;
        private readonly ILogger<PhotonDefinitionLoader> _definitionLoaderLogger;

        public PhotonPacketParserTests()
        {
            var loggerFactory = LoggerFactory.Create(builder => {});
            _logger = loggerFactory.CreateLogger<PhotonPacketParser>();
            _enricherLogger = loggerFactory.CreateLogger<PhotonPacketEnricher>();
            _definitionLoaderLogger = loggerFactory.CreateLogger<PhotonDefinitionLoader>();
        }

        [Fact]
        public void ParsePacket_WithNullPayload_ShouldReturnNull()
        {
            // Arrange
            var definitionLoader = new PhotonDefinitionLoader(_definitionLoaderLogger);
            var enricher = new PhotonPacketEnricher(definitionLoader, _enricherLogger);
            var parser = new PhotonPacketParser(enricher, _logger);

            // Act
            var result = parser.ParsePacket(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ParsePacket_WithEmptyPayload_ShouldReturnNull()
        {
            // Arrange
            var definitionLoader = new PhotonDefinitionLoader(_definitionLoaderLogger);
            var enricher = new PhotonPacketEnricher(definitionLoader, _enricherLogger);
            var parser = new PhotonPacketParser(enricher, _logger);

            // Act
            var result = parser.ParsePacket(new byte[0]);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ParsePacket_WithSmallPayload_ShouldReturnNull()
        {
            // Arrange
            var definitionLoader = new PhotonDefinitionLoader(_definitionLoaderLogger);
            var enricher = new PhotonPacketEnricher(definitionLoader, _enricherLogger);
            var parser = new PhotonPacketParser(enricher, _logger);

            // Act
            var result = parser.ParsePacket(new byte[] { 1, 2, 3 });

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ParsePacket_WithInvalidPhotonPacket_ShouldReturnNull()
        {
            // Arrange
            var definitionLoader = new PhotonDefinitionLoader(_definitionLoaderLogger);
            var enricher = new PhotonPacketEnricher(definitionLoader, _enricherLogger);
            var parser = new PhotonPacketParser(enricher, _logger);

            // Payload que não é um pacote Photon válido
            var invalidPayload = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

            // Act
            var result = parser.ParsePacket(invalidPayload);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ParsePacket_WithValidPhotonPacket_ShouldReturnEnrichedPacket()
        {
            // Arrange
            var definitionLoader = new PhotonDefinitionLoader(_definitionLoaderLogger);
            
            // Simular carregamento de definições
            definitionLoader.PacketIdToName[1] = "NewCharacter";
            definitionLoader.PacketParameterMap[1] = new Dictionary<byte, string>
            {
                { 1, "CharacterId" },
                { 2, "Name" }
            };
            
            var enricher = new PhotonPacketEnricher(definitionLoader, _enricherLogger);
            var parser = new PhotonPacketParser(enricher, _logger);

            // Criar um payload simulado de pacote Photon válido
            var validPayload = CreateMockPhotonPacket(1, new Dictionary<byte, object>
            {
                { 1, 12345 },
                { 2, "TestPlayer" }
            });

            // Act
            var result = parser.ParsePacket(validPayload);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.PacketId);
            Assert.Equal("NewCharacter", result.PacketName);
            Assert.True(result.IsKnownPacket);
            Assert.Equal(2, result.Parameters.Count);
        }

        [Fact]
        public void ParsePacket_WithUnknownPacket_ShouldReturnEnrichedPacketWithFallback()
        {
            // Arrange
            var definitionLoader = new PhotonDefinitionLoader(_definitionLoaderLogger);
            var enricher = new PhotonPacketEnricher(definitionLoader, _enricherLogger);
            var parser = new PhotonPacketParser(enricher, _logger);

            // Criar um payload para um pacote desconhecido
            var unknownPacketPayload = CreateMockPhotonPacket(999, new Dictionary<byte, object>
            {
                { 1, "UnknownValue" }
            });

            // Act
            var result = parser.ParsePacket(unknownPacketPayload);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(999, result.PacketId);
            Assert.Equal("UnknownPacket_999", result.PacketName);
            Assert.False(result.IsKnownPacket);
        }

        [Fact]
        public void ParsePacket_WithException_ShouldReturnNull()
        {
            // Arrange
            var definitionLoader = new PhotonDefinitionLoader(_definitionLoaderLogger);
            var enricher = new PhotonPacketEnricher(definitionLoader, _enricherLogger);
            var parser = new PhotonPacketParser(enricher, _logger);

            // Payload que causará exceção durante o parsing
            var problematicPayload = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A };

            // Act
            var result = parser.ParsePacket(problematicPayload);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Cria um payload simulado de pacote Photon para testes
        /// </summary>
        /// <param name="packetId">ID do pacote</param>
        /// <param name="parameters">Parâmetros do pacote</param>
        /// <returns>Array de bytes simulando um pacote Photon</returns>
        private byte[] CreateMockPhotonPacket(int packetId, Dictionary<byte, object> parameters)
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            // Cabeçalho do pacote Photon (simulado)
            writer.Write(new byte[] { 0x01, 0x02 }); // Assinatura
            writer.Write((byte)0x01); // Tipo de mensagem (evento)
            
            // Informações do pacote
            writer.Write((ushort)packetId); // ID do pacote
            writer.Write((uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds()); // Timestamp
            writer.Write((byte)parameters.Count); // Número de parâmetros

            // Parâmetros
            foreach (var kv in parameters)
            {
                writer.Write(kv.Key); // Chave do parâmetro
                
                // Tipo do valor (simplificado para string)
                writer.Write((byte)7); // Protocol16Type.String
                
                // Valor como string
                var valueStr = kv.Value.ToString() ?? "";
                var valueBytes = System.Text.Encoding.UTF8.GetBytes(valueStr);
                writer.Write((ushort)valueBytes.Length);
                writer.Write(valueBytes);
            }

            return stream.ToArray();
        }
    }
}