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
    public class NewCharacterEventHandlerTests
    {
        private readonly ILogger<NewCharacterEventHandler> _logger;
        private readonly ILogger<PositionDecryptor> _positionDecryptorLogger;
        private readonly PositionDecryptor _positionDecryptor;

        public NewCharacterEventHandlerTests()
        {
            var loggerFactory = LoggerFactory.Create(builder => {});
            _logger = loggerFactory.CreateLogger<NewCharacterEventHandler>();
            _positionDecryptorLogger = loggerFactory.CreateLogger<PositionDecryptor>();
            _positionDecryptor = new PositionDecryptor(_positionDecryptorLogger);
        }

        [Fact]
        public async Task HandleNewCharacter_WithValidParameters_ShouldReturnPlayer()
        {
            // Arrange
            var handler = new NewCharacterEventHandler(_logger, _positionDecryptor);
            
            var parameters = new Dictionary<byte, object>
            {
                { 1, 12345 }, // ID
                { 2, "TestPlayer" }, // Name
                { 3, "TestGuild" }, // Guild
                { 4, "TestAlliance" }, // Alliance
                { 5, Faction.NoPVP }, // Faction
                { 6, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 } }, // EncryptedPosition
                { 7, 5.5f }, // Speed
                { 8, 100 }, // CurrentHealth
                { 9, 100 }, // MaxHealth
                { 10, new int[] { 1, 2, 3 } }, // Equipments
                { 11, new int[] { 10, 20, 30 } } // Spells
            };

            // Act
            var result = await handler.HandleNewCharacter(parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(12345, result.Id);
            Assert.Equal("TestPlayer", result.Name);
            Assert.Equal("TestGuild", result.Guild);
            Assert.Equal("TestAlliance", result.Alliance);
            Assert.Equal(Faction.NoPVP, result.Faction);
        }

        [Fact]
        public async Task HandleNewCharacter_WithNullParameters_ShouldReturnNull()
        {
            // Arrange
            var handler = new NewCharacterEventHandler(_logger, _positionDecryptor);

            // Act
            var result = await handler.HandleNewCharacter(null!);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task HandleNewCharacter_WithMissingRequiredParameters_ShouldReturnNull()
        {
            // Arrange
            var handler = new NewCharacterEventHandler(_logger, _positionDecryptor);
            var parameters = new Dictionary<byte, object>
            {
                { 1, 12345 } // Apenas ID, faltando outros parâmetros
            };

            // Act
            var result = await handler.HandleNewCharacter(parameters);

            // Assert
            Assert.Null(result);
        }
    }
} 