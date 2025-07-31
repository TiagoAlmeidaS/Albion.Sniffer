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
    public class NewMobEventHandlerTests
    {
        private Mock<ILogger<NewMobEventHandler>> _logger;
        private Mock<PositionDecryptor> _positionDecryptor;
        private PacketOffsets _packetOffsets;
        private NewMobEventHandler _handler;

        public NewMobEventHandlerTests()
        {
            _logger = new Mock<ILogger<NewMobEventHandler>>();
            _positionDecryptor = new Mock<PositionDecryptor>();
            _packetOffsets = new PacketOffsets();
            _handler = new NewMobEventHandler(_logger.Object, _positionDecryptor.Object, _packetOffsets);
        }

        [Fact]
        public async Task HandleNewMob_WithValidParameters_ShouldReturnMob()
        {
            // Arrange
            var parameters = new Dictionary<byte, object>
            {
                { 0, 12345 }, // ID
                { 1, 100 }, // TypeId
                { 2, new float[] { 100.0f, 200.0f } }, // Position
                { 3, 1000 }, // Current Health
                { 4, 1000 }, // Max Health
                { 5, 1000 } // Max Health (duplicate)
            };

            // Act
            var result = await _handler.HandleNewMob(parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(12345, result.Id);
            Assert.Equal(85, result.TypeId); // 100 - 15
        }

        [Fact]
        public async Task HandleNewMob_WithInvalidParameters_ShouldReturnNull()
        {
            // Arrange
            var parameters = new Dictionary<byte, object>();

            // Act
            var result = await _handler.HandleNewMob(parameters);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task HandleNewMob_WithException_ShouldReturnNull()
        {
            // Arrange
            var parameters = new Dictionary<byte, object>
            {
                { 0, "invalid" } // Invalid ID type
            };

            // Act
            var result = await _handler.HandleNewMob(parameters);

            // Assert
            Assert.Null(result);
        }
    }
} 