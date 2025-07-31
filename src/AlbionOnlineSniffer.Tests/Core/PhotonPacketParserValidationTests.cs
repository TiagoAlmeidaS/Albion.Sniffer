using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Enums;
using Xunit;

namespace AlbionOnlineSniffer.Tests.Core
{
    public class PhotonPacketParserValidationTests
    {
        private readonly ILogger<PhotonPacketParser> _logger;
        private readonly PhotonPacketEnricher _packetEnricher;
        private readonly PhotonPacketParser _parser;

        public PhotonPacketParserValidationTests()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            _logger = loggerFactory.CreateLogger<PhotonPacketParser>();
            _packetEnricher = new PhotonPacketEnricher(loggerFactory.CreateLogger<PhotonPacketEnricher>());
            _parser = new PhotonPacketParser(_packetEnricher, _logger);
        }

        [Fact]
        public void Should_Validate_Valid_Event_Packet()
        {
            // Arrange - Pacote de evento válido (0x01, 0x00)
            var validEventPacket = new byte[]
            {
                0x01, 0x00,  // Message Type: Event, SubType: 0
                0x00, 0x01,  // Packet ID: 1
                0x00, 0x00, 0x00, 0x00,  // Timestamp: 0
                0x02,        // Parameter Count: 2
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08  // Dados extras
            };

            // Act
            var result = _parser.ParsePacket(validEventPacket);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Unknown", result.PacketName); // Nome padrão para pacotes não mapeados
        }

        [Fact]
        public void Should_Validate_Valid_Operation_Packet()
        {
            // Arrange - Pacote de operação válido (0x02, 0x01)
            var validOperationPacket = new byte[]
            {
                0x02, 0x01,  // Message Type: Operation, SubType: 1
                0x00, 0x02,  // Packet ID: 2
                0x00, 0x00, 0x00, 0x00,  // Timestamp: 0
                0x01,        // Parameter Count: 1
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08  // Dados extras
            };

            // Act
            var result = _parser.ParsePacket(validOperationPacket);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Unknown", result.PacketName); // Nome padrão para pacotes não mapeados
        }

        [Fact]
        public void Should_Reject_Invalid_Message_Type()
        {
            // Arrange - Pacote com tipo de mensagem inválido (0x03)
            var invalidPacket = new byte[]
            {
                0x03, 0x00,  // Message Type: Inválido
                0x00, 0x03,  // Packet ID: 3
                0x00, 0x00, 0x00, 0x00,  // Timestamp: 0
                0x01,        // Parameter Count: 1
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08  // Dados extras
            };

            // Act
            var result = _parser.ParsePacket(invalidPacket);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Should_Reject_Invalid_SubType()
        {
            // Arrange - Pacote com subtipo inválido (Event com SubType 1)
            var invalidSubTypePacket = new byte[]
            {
                0x01, 0x01,  // Message Type: Event, SubType: 1 (deveria ser 0)
                0x00, 0x04,  // Packet ID: 4
                0x00, 0x00, 0x00, 0x00,  // Timestamp: 0
                0x01,        // Parameter Count: 1
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08  // Dados extras
            };

            // Act
            var result = _parser.ParsePacket(invalidSubTypePacket);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Should_Reject_Too_Small_Packet()
        {
            // Arrange - Pacote muito pequeno
            var smallPacket = new byte[] { 0x01, 0x00, 0x00, 0x01 };

            // Act
            var result = _parser.ParsePacket(smallPacket);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Should_Reject_Null_Packet()
        {
            // Act
            var result = _parser.ParsePacket(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Should_Reject_Empty_Packet()
        {
            // Arrange
            var emptyPacket = new byte[0];

            // Act
            var result = _parser.ParsePacket(emptyPacket);

            // Assert
            Assert.Null(result);
        }
    }
} 