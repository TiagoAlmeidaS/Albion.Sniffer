using System;
using System.IO;
using System.Text.Json;
using AlbionOnlineSniffer.Core.Services;
using Microsoft.Extensions.Logging;
using Xunit;
using System.Collections.Generic;

namespace AlbionOnlineSniffer.Tests.Core
{
    public class PhotonDefinitionLoaderTests
    {
        private readonly ILogger<PhotonDefinitionLoader> _logger;

        public PhotonDefinitionLoaderTests()
        {
            var loggerFactory = LoggerFactory.Create(builder => {});
            _logger = loggerFactory.CreateLogger<PhotonDefinitionLoader>();
        }

        [Fact]
        public void Load_WithValidEventsFile_ShouldLoadPacketDefinitions()
        {
            // Arrange
            var tempDir = Path.GetTempPath();
            var testEventsPath = Path.Combine(tempDir, "events.json");
            
            var testEvents = new[]
            {
                new EventDefinition
                {
                    Id = 1,
                    Name = "NewCharacter",
                    Parameters = new Dictionary<string, string>
                    {
                        { "1", "CharacterId" },
                        { "2", "Name" },
                        { "3", "Position" }
                    }
                },
                new EventDefinition
                {
                    Id = 2,
                    Name = "NewMob",
                    Parameters = new Dictionary<string, string>
                    {
                        { "1", "MobId" },
                        { "2", "TypeId" }
                    }
                }
            };

            File.WriteAllText(testEventsPath, JsonSerializer.Serialize(testEvents));

            var loader = new PhotonDefinitionLoader(_logger);

            try
            {
                // Act
                loader.Load(tempDir);

                // Assert
                Assert.Equal(2, loader.PacketIdToName.Count);
                Assert.Equal("NewCharacter", loader.GetPacketName(1));
                Assert.Equal("NewMob", loader.GetPacketName(2));
                Assert.Equal("CharacterId", loader.GetParameterName(1, 1));
                Assert.Equal("Name", loader.GetParameterName(1, 2));
                Assert.Equal("MobId", loader.GetParameterName(2, 1));
                Assert.True(loader.IsKnownPacket(1));
                Assert.True(loader.IsKnownPacket(2));
                Assert.False(loader.IsKnownPacket(999));
            }
            finally
            {
                // Cleanup
                if (File.Exists(testEventsPath))
                    File.Delete(testEventsPath);
            }
        }

        [Fact]
        public void Load_WithValidEnumsFile_ShouldLoadEnumDefinitions()
        {
            // Arrange
            var tempDir = Path.GetTempPath();
            var testEnumsPath = Path.Combine(tempDir, "enums.json");
            
            var testEnums = new[]
            {
                new EnumDefinition
                {
                    Id = 1,
                    Name = "Faction",
                    Values = new Dictionary<string, string>
                    {
                        { "0", "None" },
                        { "1", "Martlock" },
                        { "2", "Thetford" }
                    }
                }
            };

            File.WriteAllText(testEnumsPath, JsonSerializer.Serialize(testEnums));

            var loader = new PhotonDefinitionLoader(_logger);

            try
            {
                // Act
                loader.Load(tempDir);

                // Assert
                Assert.Single(loader.EnumValueMap);
                Assert.Equal("None", loader.GetEnumValueName(1, 0));
                Assert.Equal("Martlock", loader.GetEnumValueName(1, 1));
                Assert.Equal("Thetford", loader.GetEnumValueName(1, 2));
                Assert.Equal("UnknownEnum_999", loader.GetEnumValueName(1, 999));
            }
            finally
            {
                // Cleanup
                if (File.Exists(testEnumsPath))
                    File.Delete(testEnumsPath);
            }
        }

        [Fact]
        public void GetPacketName_WithUnknownPacket_ShouldReturnFallbackName()
        {
            // Arrange
            var loader = new PhotonDefinitionLoader(_logger);

            // Act
            var result = loader.GetPacketName(999);

            // Assert
            Assert.Equal("UnknownPacket_999", result);
        }

        [Fact]
        public void GetParameterName_WithUnknownParameter_ShouldReturnFallbackName()
        {
            // Arrange
            var loader = new PhotonDefinitionLoader(_logger);

            // Act
            var result = loader.GetParameterName(1, 99);

            // Assert
            Assert.Equal("param_99", result);
        }

        [Fact]
        public void Load_WithMissingFiles_ShouldHandleGracefully()
        {
            // Arrange
            var tempDir = Path.GetTempPath();
            var loader = new PhotonDefinitionLoader(_logger);

            // Act & Assert - should not throw
            var exception = Record.Exception(() => loader.Load(tempDir));
            Assert.Null(exception);
        }
    }
}