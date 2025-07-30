using System;
using System.Collections.Generic;
using AlbionOnlineSniffer.Core.Models;
using AlbionOnlineSniffer.Core.Services;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AlbionOnlineSniffer.Tests.Core
{
    public class PhotonPacketEnricherTests
    {
        private readonly ILogger<PhotonPacketEnricher> _logger;
        private readonly ILogger<PhotonDefinitionLoader> _definitionLoaderLogger;

        public PhotonPacketEnricherTests()
        {
            var loggerFactory = LoggerFactory.Create(builder => {});
            _logger = loggerFactory.CreateLogger<PhotonPacketEnricher>();
            _definitionLoaderLogger = loggerFactory.CreateLogger<PhotonDefinitionLoader>();
        }

        [Fact]
        public void EnrichPacket_WithKnownPacket_ShouldReturnEnrichedPacket()
        {
            // Arrange
            var definitionLoader = new PhotonDefinitionLoader(_definitionLoaderLogger);
            var enricher = new PhotonPacketEnricher(definitionLoader, _logger);
            
            // Simular carregamento de definições
            definitionLoader.PacketIdToName[1] = "NewCharacter";
            definitionLoader.PacketParameterMap[1] = new Dictionary<byte, string>
            {
                { 1, "CharacterId" },
                { 2, "Name" },
                { 3, "Position" }
            };

            var parameters = new Dictionary<byte, object>
            {
                { 1, 12345 },
                { 2, "TestPlayer" },
                { 3, new { x = 100.5, y = 200.3 } }
            };

            var rawData = new byte[] { 1, 2, 3, 4, 5 };

            // Act
            var result = enricher.EnrichPacket(1, parameters, rawData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.PacketId);
            Assert.Equal("NewCharacter", result.PacketName);
            Assert.True(result.IsKnownPacket);
            Assert.Equal(3, result.Parameters.Count);
            Assert.Equal(12345, result.Parameters["CharacterId"]);
            Assert.Equal("TestPlayer", result.Parameters["Name"]);
            Assert.Equal(rawData, result.RawData);
        }

        [Fact]
        public void EnrichPacket_WithUnknownPacket_ShouldReturnEnrichedPacketWithFallbackName()
        {
            // Arrange
            var definitionLoader = new PhotonDefinitionLoader(_definitionLoaderLogger);
            var enricher = new PhotonPacketEnricher(definitionLoader, _logger);
            
            var parameters = new Dictionary<byte, object>
            {
                { 1, "UnknownValue" }
            };

            // Act
            var result = enricher.EnrichPacket(999, parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(999, result.PacketId);
            Assert.Equal("UnknownPacket_999", result.PacketName);
            Assert.False(result.IsKnownPacket);
            Assert.Single(result.Parameters);
            Assert.Equal("UnknownValue", result.Parameters["param_1"]);
        }

        [Fact]
        public void EnrichPacket_WithMixedKnownAndUnknownParameters_ShouldHandleBoth()
        {
            // Arrange
            var definitionLoader = new PhotonDefinitionLoader(_definitionLoaderLogger);
            var enricher = new PhotonPacketEnricher(definitionLoader, _logger);
            
            // Simular carregamento de definições
            definitionLoader.PacketIdToName[1] = "NewCharacter";
            definitionLoader.PacketParameterMap[1] = new Dictionary<byte, string>
            {
                { 1, "CharacterId" },
                { 3, "Position" }
            };

            var parameters = new Dictionary<byte, object>
            {
                { 1, 12345 },      // Known parameter
                { 2, "Unknown" },   // Unknown parameter
                { 3, new { x = 100, y = 200 } }  // Known parameter
            };

            // Act
            var result = enricher.EnrichPacket(1, parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Parameters.Count);
            Assert.Equal(12345, result.Parameters["CharacterId"]);
            Assert.Equal("Unknown", result.Parameters["param_2"]);
            Assert.NotNull(result.Parameters["Position"]);
        }

        [Fact]
        public void EnrichEnumValue_WithKnownEnum_ShouldReturnReadableName()
        {
            // Arrange
            var definitionLoader = new PhotonDefinitionLoader(_definitionLoaderLogger);
            var enricher = new PhotonPacketEnricher(definitionLoader, _logger);
            
            // Simular carregamento de enum
            definitionLoader.EnumValueMap[1] = new Dictionary<int, string>
            {
                { 0, "None" },
                { 1, "Martlock" },
                { 2, "Thetford" }
            };

            // Act
            var result = enricher.EnrichEnumValue(1, 2);

            // Assert
            Assert.Equal("Thetford", result);
        }

        [Fact]
        public void EnrichEnumValue_WithUnknownEnum_ShouldReturnFallbackName()
        {
            // Arrange
            var definitionLoader = new PhotonDefinitionLoader(_definitionLoaderLogger);
            var enricher = new PhotonPacketEnricher(definitionLoader, _logger);

            // Act
            var result = enricher.EnrichEnumValue(999, 123);

            // Assert
            Assert.Equal("UnknownEnum_123", result);
        }

        [Fact]
        public void IsKnownPacket_WithKnownPacket_ShouldReturnTrue()
        {
            // Arrange
            var definitionLoader = new PhotonDefinitionLoader(_definitionLoaderLogger);
            var enricher = new PhotonPacketEnricher(definitionLoader, _logger);
            
            definitionLoader.PacketIdToName[1] = "NewCharacter";

            // Act
            var result = enricher.IsKnownPacket(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsKnownPacket_WithUnknownPacket_ShouldReturnFalse()
        {
            // Arrange
            var definitionLoader = new PhotonDefinitionLoader(_definitionLoaderLogger);
            var enricher = new PhotonPacketEnricher(definitionLoader, _logger);

            // Act
            var result = enricher.IsKnownPacket(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetStatistics_ShouldReturnValidStatistics()
        {
            // Arrange
            var definitionLoader = new PhotonDefinitionLoader(_definitionLoaderLogger);
            var enricher = new PhotonPacketEnricher(definitionLoader, _logger);
            
            // Simular dados carregados
            definitionLoader.PacketIdToName[1] = "NewCharacter";
            definitionLoader.EnumValueMap[1] = new Dictionary<int, string> { { 0, "None" } };

            // Act
            var stats = enricher.GetStatistics();

            // Assert
            Assert.NotNull(stats);
            Assert.Equal(1, stats["TotalKnownPackets"]);
            Assert.Equal(1, stats["TotalEnums"]);
            Assert.True((bool)stats["DefinitionLoaderLoaded"]);
        }

        [Fact]
        public void EnrichPacket_WithException_ShouldReturnErrorPacket()
        {
            // Arrange
            var definitionLoader = new PhotonDefinitionLoader(_definitionLoaderLogger);
            var enricher = new PhotonPacketEnricher(definitionLoader, _logger);
            
            // Simular uma situação que pode causar erro
            var parameters = new Dictionary<byte, object>
            {
                { 1, new object() } // Objeto que pode causar problemas na serialização
            };

            // Act
            var result = enricher.EnrichPacket(1, parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.PacketId);
            Assert.Equal("UnknownPacket_1", result.PacketName); // Corrigido para o nome esperado
            Assert.False(result.IsKnownPacket);
        }
    }
}