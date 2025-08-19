using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using AlbionOnlineSniffer.Core.Contracts.Transformers;
using Albion.Events.V1;

namespace AlbionOnlineSniffer.Tests.Unit.Core.Contracts.Transformers
{
    public class NewCharacterToPlayerSpottedV1Tests
    {
        private readonly Mock<PositionDecryptionService> _positionDecryptionServiceMock;
        private readonly NewCharacterToPlayerSpottedV1 _transformer;

        public NewCharacterToPlayerSpottedV1Tests()
        {
            _positionDecryptionServiceMock = new Mock<PositionDecryptionService>(MockBehavior.Strict, (Microsoft.Extensions.Logging.ILogger<PositionDecryptionService>)Mock.Of<Microsoft.Extensions.Logging.ILogger<PositionDecryptionService>>());
            _transformer = new NewCharacterToPlayerSpottedV1(_positionDecryptionServiceMock.Object);
        }

        [Fact]
        public void Constructor_WithValidDependencies_ShouldCreateInstance()
        {
            // Assert
            _transformer.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithNullPositionDecryptionService_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Action act = () => new NewCharacterToPlayerSpottedV1(null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("positionDecryptionService");
        }

        [Fact]
        public void CanTransform_WithNewCharacterEvent_ShouldReturnTrue()
        {
            // Arrange
            var newCharacterEvent = new NewCharacterEvent();

            // Act
            var result = _transformer.CanTransform(newCharacterEvent);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void CanTransform_WithOtherEventType_ShouldReturnFalse()
        {
            // Arrange
            var otherEvent = new MoveEvent();

            // Act
            var result = _transformer.CanTransform(otherEvent);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CanTransform_WithNullEvent_ShouldReturnFalse()
        {
            // Act
            var result = _transformer.CanTransform(null!);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task TryTransform_WithValidNewCharacterEvent_ShouldTransformSuccessfully()
        {
            // Arrange
            var newCharacterEvent = new NewCharacterEvent
            {
                CharacterId = "char-123",
                CharacterName = "TestPlayer",
                GuildName = "TestGuild",
                AllianceName = "TestAlliance",
                Position = new Position { X = 100.5f, Y = 200.3f },
                PositionBytes = new byte[] { 0x01, 0x02, 0x03, 0x04 },
                Tier = 6
            };

            _positionDecryptionServiceMock
                .Setup(x => x.IsConfigured())
                .Returns(true);

            _positionDecryptionServiceMock
                .Setup(x => x.DecryptPosition(newCharacterEvent.PositionBytes))
                .Returns(new Position { X = 150.7f, Y = 250.9f });

            // Act
            var (success, topic, contract) = await _transformer.TryTransform(newCharacterEvent);

            // Assert
            success.Should().BeTrue();
            topic.Should().Be("players.spotted.v1");

            contract.Should().BeOfType<PlayerSpottedV1>();
            var playerSpotted = (PlayerSpottedV1)contract;
            playerSpotted.PlayerId.Should().Be("char-123");
            playerSpotted.PlayerName.Should().Be("TestPlayer");
            playerSpotted.GuildName.Should().Be("TestGuild");
            playerSpotted.AllianceName.Should().Be("TestAlliance");
            playerSpotted.X.Should().Be(150.7f); // Decrypted position
            playerSpotted.Y.Should().Be(250.9f); // Decrypted position
            playerSpotted.Tier.Should().Be(6);
        }

        [Fact]
        public async Task TryTransform_WithNewCharacterEventWithoutPositionBytes_ShouldUsePosition()
        {
            // Arrange
            var newCharacterEvent = new NewCharacterEvent
            {
                CharacterId = "char-456",
                CharacterName = "TestPlayer2",
                Position = new Position { X = 300.1f, Y = 400.2f },
                PositionBytes = null,
                Tier = 7
            };

            _positionDecryptionServiceMock
                .Setup(x => x.IsConfigured())
                .Returns(true);

            // Act
            var (success, topic, contract) = await _transformer.TryTransform(newCharacterEvent);

            // Assert
            success.Should().BeTrue();
            topic.Should().Be("players.spotted.v1");

            contract.Should().BeOfType<PlayerSpottedV1>();
            var playerSpotted = (PlayerSpottedV1)contract;
            playerSpotted.X.Should().Be(300.1f); // Original position
            playerSpotted.Y.Should().Be(400.2f); // Original position
        }

        [Fact]
        public async Task TryTransform_WithNewCharacterEventWithNullPosition_ShouldHandleGracefully()
        {
            // Arrange
            var newCharacterEvent = new NewCharacterEvent
            {
                CharacterId = "char-789",
                CharacterName = "TestPlayer3",
                Position = null,
                PositionBytes = null,
                Tier = 5
            };

            _positionDecryptionServiceMock
                .Setup(x => x.IsConfigured())
                .Returns(true);

            // Act
            var (success, topic, contract) = await _transformer.TryTransform(newCharacterEvent);

            // Assert
            success.Should().BeTrue();
            topic.Should().Be("players.spotted.v1");

            contract.Should().BeOfType<PlayerSpottedV1>();
            var playerSpotted = (PlayerSpottedV1)contract;
            playerSpotted.X.Should().Be(0f);
            playerSpotted.Y.Should().Be(0f);
        }

        [Fact]
        public async Task TryTransform_WhenPositionDecryptionServiceNotConfigured_ShouldUseOriginalPosition()
        {
            // Arrange
            var newCharacterEvent = new NewCharacterEvent
            {
                CharacterId = "char-999",
                CharacterName = "TestPlayer4",
                Position = new Position { X = 500.0f, Y = 600.0f },
                PositionBytes = new byte[] { 0x05, 0x06, 0x07, 0x08 },
                Tier = 8
            };

            _positionDecryptionServiceMock
                .Setup(x => x.IsConfigured())
                .Returns(false);

            // Act
            var (success, topic, contract) = await _transformer.TryTransform(newCharacterEvent);

            // Assert
            success.Should().BeTrue();
            topic.Should().Be("players.spotted.v1");

            contract.Should().BeOfType<PlayerSpottedV1>();
            var playerSpotted = (PlayerSpottedV1)contract;
            playerSpotted.X.Should().Be(500.0f); // Original position
            playerSpotted.Y.Should().Be(600.0f); // Original position
        }

        [Fact]
        public async Task TryTransform_WhenPositionDecryptionServiceThrows_ShouldUseOriginalPosition()
        {
            // Arrange
            var newCharacterEvent = new NewCharacterEvent
            {
                CharacterId = "char-error",
                CharacterName = "TestPlayer5",
                Position = new Position { X = 700.0f, Y = 800.0f },
                PositionBytes = new byte[] { 0x09, 0x0A, 0x0B, 0x0C },
                Tier = 4
            };

            _positionDecryptionServiceMock
                .Setup(x => x.IsConfigured())
                .Returns(true);

            _positionDecryptionServiceMock
                .Setup(x => x.DecryptPosition(newCharacterEvent.PositionBytes))
                .Throws(new InvalidOperationException("Decryption failed"));

            // Act
            var (success, topic, contract) = await _transformer.TryTransform(newCharacterEvent);

            // Assert
            success.Should().BeTrue();
            topic.Should().Be("players.spotted.v1");

            contract.Should().BeOfType<PlayerSpottedV1>();
            var playerSpotted = (PlayerSpottedV1)contract;
            playerSpotted.X.Should().Be(700.0f); // Original position (fallback)
            playerSpotted.Y.Should().Be(800.0f); // Original position (fallback)
        }

        [Fact]
        public async Task TryTransform_WithMinimalNewCharacterEvent_ShouldTransformSuccessfully()
        {
            // Arrange
            var newCharacterEvent = new NewCharacterEvent
            {
                CharacterId = "char-minimal",
                CharacterName = "MinimalPlayer"
            };

            _positionDecryptionServiceMock
                .Setup(x => x.IsConfigured())
                .Returns(false);

            // Act
            var (success, topic, contract) = await _transformer.TryTransform(newCharacterEvent);

            // Assert
            success.Should().BeTrue();
            topic.Should().Be("players.spotted.v1");

            contract.Should().BeOfType<PlayerSpottedV1>();
            var playerSpotted = (PlayerSpottedV1)contract;
            playerSpotted.PlayerId.Should().Be("char-minimal");
            playerSpotted.PlayerName.Should().Be("MinimalPlayer");
            playerSpotted.GuildName.Should().BeNull();
            playerSpotted.AllianceName.Should().BeNull();
            playerSpotted.X.Should().Be(0f);
            playerSpotted.Y.Should().Be(0f);
            playerSpotted.Tier.Should().Be(0);
        }

        // Helper classes for testing
        private class NewCharacterEvent
        {
            public string? CharacterId { get; set; }
            public string? CharacterName { get; set; }
            public string? GuildName { get; set; }
            public string? AllianceName { get; set; }
            public Position? Position { get; set; }
            public byte[]? PositionBytes { get; set; }
            public int Tier { get; set; }
        }

        private class MoveEvent
        {
            public string? CharacterId { get; set; }
        }

        private class Position
        {
            public float X { get; set; }
            public float Y { get; set; }
        }
    }
}
