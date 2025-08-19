using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using AlbionOnlineSniffer.Core.Enrichers;
using AlbionOnlineSniffer.Options.Profiles;
using Microsoft.Extensions.Logging;

namespace AlbionOnlineSniffer.Tests.Unit.Core.Enrichers
{
    public class TierColorEnricherTests
    {
        private readonly Mock<ILogger<TierColorEnricher>> _loggerMock;
        private readonly Mock<ITierPaletteManager> _tierPaletteManagerMock;
        private readonly TierColorEnricher _enricher;

        public TierColorEnricherTests()
        {
            _loggerMock = new Mock<ILogger<TierColorEnricher>>();
            _tierPaletteManagerMock = new Mock<ITierPaletteManager>();
            _enricher = new TierColorEnricher(_loggerMock.Object, _tierPaletteManagerMock.Object);
        }

        [Fact]
        public void Constructor_WithValidDependencies_ShouldCreateInstance()
        {
            // Assert
            _enricher.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Action act = () => new TierColorEnricher(null!, _tierPaletteManagerMock.Object);
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Fact]
        public void Constructor_WithNullTierPaletteManager_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Action act = () => new TierColorEnricher(_loggerMock.Object, null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("tierPaletteManager");
        }

        [Fact]
        public async Task EnrichAsync_WithValidEvent_ShouldEnrichSuccessfully()
        {
            // Arrange
            var testEvent = new TestEvent { Tier = 5 };
            var expectedColor = "#FFD700"; // Gold color for tier 5
            
            _tierPaletteManagerMock
                .Setup(x => x.GetTierColor(5))
                .Returns(expectedColor);

            // Act
            var result = await _enricher.EnrichAsync(testEvent);

            // Assert
            result.Should().BeTrue();
            testEvent.TierColor.Should().Be(expectedColor);
        }

        [Fact]
        public async Task EnrichAsync_WithNullEvent_ShouldReturnFalse()
        {
            // Act
            var result = await _enricher.EnrichAsync(null!);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task EnrichAsync_WithEventWithoutTier_ShouldReturnFalse()
        {
            // Arrange
            var testEvent = new TestEventWithoutTier();

            // Act
            var result = await _enricher.EnrichAsync(testEvent);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task EnrichAsync_WithTierZero_ShouldUseDefaultColor()
        {
            // Arrange
            var testEvent = new TestEvent { Tier = 0 };
            var defaultColor = "#808080"; // Default gray color
            
            _tierPaletteManagerMock
                .Setup(x => x.GetTierColor(0))
                .Returns(defaultColor);

            // Act
            var result = await _enricher.EnrichAsync(testEvent);

            // Assert
            result.Should().BeTrue();
            testEvent.TierColor.Should().Be(defaultColor);
        }

        [Fact]
        public async Task EnrichAsync_WithHighTier_ShouldUseHighTierColor()
        {
            // Arrange
            var testEvent = new TestEvent { Tier = 8 };
            var highTierColor = "#FF0000"; // Red color for high tier
            
            _tierPaletteManagerMock
                .Setup(x => x.GetTierColor(8))
                .Returns(highTierColor);

            // Act
            var result = await _enricher.EnrichAsync(testEvent);

            // Assert
            result.Should().BeTrue();
            testEvent.TierColor.Should().Be(highTierColor);
        }

        [Fact]
        public async Task EnrichAsync_WhenTierPaletteManagerThrows_ShouldReturnFalse()
        {
            // Arrange
            var testEvent = new TestEvent { Tier = 5 };
            
            _tierPaletteManagerMock
                .Setup(x => x.GetTierColor(5))
                .Throws(new InvalidOperationException("Test exception"));

            // Act
            var result = await _enricher.EnrichAsync(testEvent);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task EnrichAsync_WithMultipleEvents_ShouldEnrichAll()
        {
            // Arrange
            var events = new[]
            {
                new TestEvent { Tier = 4 },
                new TestEvent { Tier = 6 },
                new TestEvent { Tier = 8 }
            };

            _tierPaletteManagerMock
                .Setup(x => x.GetTierColor(4))
                .Returns("#00FF00");
            _tierPaletteManagerMock
                .Setup(x => x.GetTierColor(6))
                .Returns("#FF00FF");
            _tierPaletteManagerMock
                .Setup(x => x.GetTierColor(8))
                .Returns("#FF0000");

            // Act & Assert
            foreach (var evt in events)
            {
                var result = await _enricher.EnrichAsync(evt);
                result.Should().BeTrue();
                evt.TierColor.Should().NotBeNullOrEmpty();
            }
        }

        // Helper classes for testing
        private class TestEvent
        {
            public int Tier { get; set; }
            public string? TierColor { get; set; }
        }

        private class TestEventWithoutTier
        {
            public string Name { get; set; } = "Test";
        }
    }
}
