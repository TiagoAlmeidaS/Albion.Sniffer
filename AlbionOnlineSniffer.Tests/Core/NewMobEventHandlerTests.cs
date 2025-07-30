using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Interfaces;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models;
using AlbionOnlineSniffer.Core.Models.GameObjects;
using AlbionOnlineSniffer.Core.Services;
using Xunit;
using AlbionOnlineSniffer.Core.Handlers;
using Microsoft.Extensions.Logging;

namespace AlbionOnlineSniffer.Tests.Core
{
    public class NewMobEventHandlerTests
    {
        private readonly ILogger<NewMobEventHandler> _logger;
        private readonly ILogger<PositionDecryptor> _positionDecryptorLogger;
        private readonly PositionDecryptor _positionDecryptor;

        public NewMobEventHandlerTests()
        {
            var loggerFactory = LoggerFactory.Create(builder => {});
            _logger = loggerFactory.CreateLogger<NewMobEventHandler>();
            _positionDecryptorLogger = loggerFactory.CreateLogger<PositionDecryptor>();
            _positionDecryptor = new PositionDecryptor(_positionDecryptorLogger);
        }

        [Fact]
        public async Task HandleNewMob_WithValidParameters_ShouldReturnMob()
        {
            // Arrange
            var handler = new NewMobEventHandler(_logger, _positionDecryptor);
            
            var parameters = new Dictionary<byte, object>
            {
                { 1, 12345 }, // ID
                { 2, 57 }, // TypeId (57 - 15 = 42)
                { 3, new float[] { 100.5f, 200.3f } }, // Position
                { 4, 150 }, // CurrentHealth
                { 5, 150 }, // MaxHealth
                { 6, 3 } // Charge
            };

            // Act
            var result = await handler.HandleNewMob(parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(12345, result.Id);
            Assert.Equal(42, result.TypeId); // 57 - 15 = 42
            Assert.Equal(new Vector2(100.5f, 200.3f), result.Position);
            Assert.Equal(150, result.Health.Value);
            Assert.Equal(150, result.Health.MaxValue);
            Assert.Equal(3, result.Charge);
        }

        [Fact]
        public async Task HandleNewMob_WithNullParameters_ShouldReturnNull()
        {
            // Arrange
            var handler = new NewMobEventHandler(_logger, _positionDecryptor);

            // Act
            var result = await handler.HandleNewMob(null!);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task HandleNewMob_WithMissingRequiredParameters_ShouldReturnNull()
        {
            // Arrange
            var handler = new NewMobEventHandler(_logger, _positionDecryptor);
            var parameters = new Dictionary<byte, object>
            {
                { 1, 12345 } // Apenas ID, faltando outros parâmetros
            };

            // Act
            var result = await handler.HandleNewMob(parameters);

            // Assert
            Assert.Null(result);
        }
    }
} 