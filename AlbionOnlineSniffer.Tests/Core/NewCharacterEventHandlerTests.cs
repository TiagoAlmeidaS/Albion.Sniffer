using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Handlers;
using AlbionOnlineSniffer.Core.Models;
using AlbionOnlineSniffer.Core.Models.GameObjects;
using AlbionOnlineSniffer.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AlbionOnlineSniffer.Tests.Core
{
    public class NewCharacterEventHandlerTests
    {
        private Mock<ILogger<NewCharacterEventHandler>> _logger;
        private Mock<PositionDecryptor> _positionDecryptor;
        private PacketOffsets _packetOffsets;
        private NewCharacterEventHandler _handler;

        public NewCharacterEventHandlerTests()
        {
            _logger = new Mock<ILogger<NewCharacterEventHandler>>();
            _positionDecryptor = new Mock<PositionDecryptor>();
            _packetOffsets = new PacketOffsets();
            _handler = new NewCharacterEventHandler(_logger.Object, _positionDecryptor.Object, _packetOffsets);
        }

        [Fact]
        public async Task HandleNewCharacter_WithValidParameters_ShouldReturnPlayer()
        {
            // Arrange
            var parameters = new Dictionary<byte, object>
            {
                { 0, 12345 }, // ID
                { 1, "TestPlayer" }, // Name
                { 2, "TestGuild" }, // Guild
                { 3, "TestAlliance" }, // Alliance
                { 4, Faction.Martlock }, // Faction
                { 5, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 } }, // Encrypted Position
                { 6, 5.5f }, // Speed
                { 7, 1000 }, // Current Health
                { 8, 1000 }, // Max Health
                { 9, new int[] { 1, 2, 3 } }, // Equipment
                { 10, new int[] { 4, 5, 6 } } // Spells
            };

            _positionDecryptor.Setup(x => x.DecryptPosition(It.IsAny<byte[]>(), It.IsAny<int>()))
                .Returns(new Vector2(100.0f, 200.0f));

            // Act
            var result = await _handler.HandleNewCharacter(parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(12345, result.Id);
            Assert.Equal("TestPlayer", result.Name);
        }

        [Fact]
        public async Task HandleNewCharacter_WithInvalidParameters_ShouldReturnNull()
        {
            // Arrange
            var parameters = new Dictionary<byte, object>();

            // Act
            var result = await _handler.HandleNewCharacter(parameters);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task HandleNewCharacter_WithException_ShouldReturnNull()
        {
            // Arrange
            var parameters = new Dictionary<byte, object>
            {
                { 0, "invalid" } // Invalid ID type
            };

            // Act
            var result = await _handler.HandleNewCharacter(parameters);

            // Assert
            Assert.Null(result);
        }
    }
} 