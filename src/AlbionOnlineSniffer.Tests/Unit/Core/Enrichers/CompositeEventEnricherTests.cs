using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using AlbionOnlineSniffer.Core.Enrichers;
using Microsoft.Extensions.Logging;

namespace AlbionOnlineSniffer.Tests.Unit.Core.Enrichers
{
    public class CompositeEventEnricherTests
    {
        private readonly Mock<ILogger<CompositeEventEnricher>> _loggerMock;
        private readonly Mock<IEventEnricher> _enricher1Mock;
        private readonly Mock<IEventEnricher> _enricher2Mock;
        private readonly CompositeEventEnricher _compositeEnricher;

        public CompositeEventEnricherTests()
        {
            _loggerMock = new Mock<ILogger<CompositeEventEnricher>>();
            _enricher1Mock = new Mock<IEventEnricher>();
            _enricher2Mock = new Mock<IEventEnricher>();
            
            var enrichers = new List<IEventEnricher> { _enricher1Mock.Object, _enricher2Mock.Object };
            _compositeEnricher = new CompositeEventEnricher(_loggerMock.Object, enrichers);
        }

        [Fact]
        public void Constructor_WithValidDependencies_ShouldCreateInstance()
        {
            // Assert
            _compositeEnricher.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Arrange
            var enrichers = new List<IEventEnricher> { _enricher1Mock.Object };

            // Act & Assert
            Action act = () => new CompositeEventEnricher(null!, enrichers);
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Fact]
        public void Constructor_WithNullEnrichers_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Action act = () => new CompositeEventEnricher(_loggerMock.Object, null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("enrichers");
        }

        [Fact]
        public void Constructor_WithEmptyEnrichers_ShouldCreateInstance()
        {
            // Arrange
            var emptyEnrichers = new List<IEventEnricher>();

            // Act
            var composite = new CompositeEventEnricher(_loggerMock.Object, emptyEnrichers);

            // Assert
            composite.Should().NotBeNull();
        }

        [Fact]
        public async Task EnrichAsync_WithSingleEnricher_ShouldCallEnricher()
        {
            // Arrange
            var testEvent = new TestEvent { Id = "test-1" };
            _enricher1Mock
                .Setup(x => x.EnrichAsync(testEvent))
                .ReturnsAsync(true);

            // Act
            var result = await _compositeEnricher.EnrichAsync(testEvent);

            // Assert
            result.Should().BeTrue();
            _enricher1Mock.Verify(x => x.EnrichAsync(testEvent), Times.Once);
        }

        [Fact]
        public async Task EnrichAsync_WithMultipleEnrichers_ShouldCallAllEnrichers()
        {
            // Arrange
            var testEvent = new TestEvent { Id = "test-1" };
            _enricher1Mock
                .Setup(x => x.EnrichAsync(testEvent))
                .ReturnsAsync(true);
            _enricher2Mock
                .Setup(x => x.EnrichAsync(testEvent))
                .ReturnsAsync(true);

            // Act
            var result = await _compositeEnricher.EnrichAsync(testEvent);

            // Assert
            result.Should().BeTrue();
            _enricher1Mock.Verify(x => x.EnrichAsync(testEvent), Times.Once);
            _enricher2Mock.Verify(x => x.EnrichAsync(testEvent), Times.Once);
        }

        [Fact]
        public async Task EnrichAsync_WhenFirstEnricherFails_ShouldContinueWithOthers()
        {
            // Arrange
            var testEvent = new TestEvent { Id = "test-1" };
            _enricher1Mock
                .Setup(x => x.EnrichAsync(testEvent))
                .ReturnsAsync(false);
            _enricher2Mock
                .Setup(x => x.EnrichAsync(testEvent))
                .ReturnsAsync(true);

            // Act
            var result = await _compositeEnricher.EnrichAsync(testEvent);

            // Assert
            result.Should().BeTrue(); // Composite returns true if any enricher succeeds
            _enricher1Mock.Verify(x => x.EnrichAsync(testEvent), Times.Once);
            _enricher2Mock.Verify(x => x.EnrichAsync(testEvent), Times.Once);
        }

        [Fact]
        public async Task EnrichAsync_WhenAllEnrichersFail_ShouldReturnFalse()
        {
            // Arrange
            var testEvent = new TestEvent { Id = "test-1" };
            _enricher1Mock
                .Setup(x => x.EnrichAsync(testEvent))
                .ReturnsAsync(false);
            _enricher2Mock
                .Setup(x => x.EnrichAsync(testEvent))
                .ReturnsAsync(false);

            // Act
            var result = await _compositeEnricher.EnrichAsync(testEvent);

            // Assert
            result.Should().BeFalse();
            _enricher1Mock.Verify(x => x.EnrichAsync(testEvent), Times.Once);
            _enricher2Mock.Verify(x => x.EnrichAsync(testEvent), Times.Once);
        }

        [Fact]
        public async Task EnrichAsync_WhenEnricherThrowsException_ShouldLogAndContinue()
        {
            // Arrange
            var testEvent = new TestEvent { Id = "test-1" };
            _enricher1Mock
                .Setup(x => x.EnrichAsync(testEvent))
                .ThrowsAsync(new InvalidOperationException("Test exception"));
            _enricher2Mock
                .Setup(x => x.EnrichAsync(testEvent))
                .ReturnsAsync(true);

            // Act
            var result = await _compositeEnricher.EnrichAsync(testEvent);

            // Assert
            result.Should().BeTrue();
            _enricher1Mock.Verify(x => x.EnrichAsync(testEvent), Times.Once);
            _enricher2Mock.Verify(x => x.EnrichAsync(testEvent), Times.Once);
        }

        [Fact]
        public async Task EnrichAsync_WithNullEvent_ShouldReturnFalse()
        {
            // Act
            var result = await _compositeEnricher.EnrichAsync(null!);

            // Assert
            result.Should().BeFalse();
            _enricher1Mock.Verify(x => x.EnrichAsync(It.IsAny<object>()), Times.Never);
            _enricher2Mock.Verify(x => x.EnrichAsync(It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public async Task EnrichAsync_WithEmptyEnrichers_ShouldReturnFalse()
        {
            // Arrange
            var emptyComposite = new CompositeEventEnricher(_loggerMock.Object, new List<IEventEnricher>());
            var testEvent = new TestEvent { Id = "test-1" };

            // Act
            var result = await emptyComposite.EnrichAsync(testEvent);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task EnrichAsync_WithLargeNumberOfEnrichers_ShouldProcessAll()
        {
            // Arrange
            var enrichers = new List<IEventEnricher>();
            for (int i = 0; i < 10; i++)
            {
                var mock = new Mock<IEventEnricher>();
                mock.Setup(x => x.EnrichAsync(It.IsAny<object>())).ReturnsAsync(true);
                enrichers.Add(mock.Object);
            }

            var largeComposite = new CompositeEventEnricher(_loggerMock.Object, enrichers);
            var testEvent = new TestEvent { Id = "test-1" };

            // Act
            var result = await largeComposite.EnrichAsync(testEvent);

            // Assert
            result.Should().BeTrue();
            foreach (var enricher in enrichers)
            {
                Mock.Get(enricher).Verify(x => x.EnrichAsync(testEvent), Times.Once);
            }
        }

        // Helper class for testing
        private class TestEvent
        {
            public string Id { get; set; } = string.Empty;
        }
    }
}
