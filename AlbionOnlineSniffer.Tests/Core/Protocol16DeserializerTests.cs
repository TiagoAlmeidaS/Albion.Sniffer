using System;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Models;
using Microsoft.Extensions.Logging;
using Xunit;
using Albion.Network;

namespace AlbionOnlineSniffer.Tests.Core
{
    public class Protocol16DeserializerTests
    {
        private class TestHandler
        {
            public bool EventCalled { get; private set; }
            public void Handle(byte[] payload)
            {
                EventCalled = true;
            }
        }

        [Fact]
        public void Constructor_WithValidParameters_ShouldNotThrow()
        {
            // Arrange
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<Protocol16Deserializer>();
            var definitionLoader = new PhotonDefinitionLoader(loggerFactory.CreateLogger<PhotonDefinitionLoader>());
            var packetEnricher = new PhotonPacketEnricher(definitionLoader, loggerFactory.CreateLogger<PhotonPacketEnricher>());
            var eventDispatcher = new EventDispatcher(loggerFactory.CreateLogger<EventDispatcher>());
            var packetProcessor = new PacketProcessor(
                loggerFactory.CreateLogger<PacketProcessor>(),
                new PacketOffsets(),
                null!, // PlayersManager
                null!, // MobsManager
                null!, // HarvestablesManager
                null!, // LootChestsManager
                null!, // DungeonsManager
                null!, // FishNodesManager
                null!, // GatedWispsManager
                null!, // PositionDecryptor
                eventDispatcher // EventDispatcher
            );

            // Criar um IPhotonReceiver mock para teste
            var mockPhotonReceiver = ReceiverBuilder.Create().Build();

            // Act & Assert
            var exception = Record.Exception(() => 
                new Protocol16Deserializer(packetEnricher, packetProcessor, mockPhotonReceiver, logger));

            Assert.Null(exception);
        }

        [Fact]
        public void ReceivePacket_WithNullPayload_ShouldNotThrow()
        {
            // Arrange
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<Protocol16Deserializer>();
            var definitionLoader = new PhotonDefinitionLoader(loggerFactory.CreateLogger<PhotonDefinitionLoader>());
            var packetEnricher = new PhotonPacketEnricher(definitionLoader, loggerFactory.CreateLogger<PhotonPacketEnricher>());
            var eventDispatcher = new EventDispatcher(loggerFactory.CreateLogger<EventDispatcher>());
            var packetProcessor = new PacketProcessor(
                loggerFactory.CreateLogger<PacketProcessor>(),
                new PacketOffsets(),
                null!, // PlayersManager
                null!, // MobsManager
                null!, // HarvestablesManager
                null!, // LootChestsManager
                null!, // DungeonsManager
                null!, // FishNodesManager
                null!, // GatedWispsManager
                null!, // PositionDecryptor
                eventDispatcher // EventDispatcher
            );
            
            var mockPhotonReceiver = ReceiverBuilder.Create().Build();
            var deserializer = new Protocol16Deserializer(packetEnricher, packetProcessor, mockPhotonReceiver, logger);

            // Act & Assert
            var exception = Record.Exception(() => deserializer.ReceivePacket(null!));
            Assert.Null(exception);
        }

        [Fact]
        public void ReceivePacket_WithEmptyPayload_ShouldNotThrow()
        {
            // Arrange
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<Protocol16Deserializer>();
            var definitionLoader = new PhotonDefinitionLoader(loggerFactory.CreateLogger<PhotonDefinitionLoader>());
            var packetEnricher = new PhotonPacketEnricher(definitionLoader, loggerFactory.CreateLogger<PhotonPacketEnricher>());
            var eventDispatcher = new EventDispatcher(loggerFactory.CreateLogger<EventDispatcher>());
            var packetProcessor = new PacketProcessor(
                loggerFactory.CreateLogger<PacketProcessor>(),
                new PacketOffsets(),
                null!, // PlayersManager
                null!, // MobsManager
                null!, // HarvestablesManager
                null!, // LootChestsManager
                null!, // DungeonsManager
                null!, // FishNodesManager
                null!, // GatedWispsManager
                null!, // PositionDecryptor
                eventDispatcher // EventDispatcher
            );
            
            var mockPhotonReceiver = ReceiverBuilder.Create().Build();
            var deserializer = new Protocol16Deserializer(packetEnricher, packetProcessor, mockPhotonReceiver, logger);

            // Act & Assert
            var exception = Record.Exception(() => deserializer.ReceivePacket(new byte[0]));
            Assert.Null(exception);
        }
    }
} 