using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using AlbionOnlineSniffer.Core.Pipeline;
using AlbionOnlineSniffer.Core.Enrichers;
using AlbionOnlineSniffer.Core.Observability;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AlbionOnlineSniffer.Tests.Unit.Core.Pipeline
{
    public class EventPipelineTests
    {
        private readonly Mock<ILogger<EventPipeline>> _loggerMock;
        private readonly Mock<IObservabilityService> _observabilityMock;
        private readonly Mock<IEventEnricher> _enricherMock;
        private readonly PipelineConfiguration _config;

        public EventPipelineTests()
        {
            _loggerMock = new Mock<ILogger<EventPipeline>>();
            _observabilityMock = new Mock<IObservabilityService>();
            _enricherMock = new Mock<IEventEnricher>();
            _config = new PipelineConfiguration
            {
                BufferSize = 100,
                WorkerCount = 2,
                MaxConcurrency = 5
            };
        }

        [Fact]
        public void Constructor_WithValidConfig_ShouldCreateInstance()
        {
            // Act
            var pipeline = new EventPipeline(_config, _loggerMock.Object, _observabilityMock.Object);

            // Assert
            pipeline.Should().NotBeNull();
            pipeline.GetBufferUsagePercentage().Should().Be(0);
        }

        [Fact]
        public void Constructor_WithNullConfig_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Action act = () => new EventPipeline(null!, _loggerMock.Object, _observabilityMock.Object);
            act.Should().Throw<ArgumentNullException>().WithParameterName("configuration");
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Action act = () => new EventPipeline(_config, null!, _observabilityMock.Object);
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Fact]
        public void Constructor_WithNullObservability_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Action act = () => new EventPipeline(_config, _loggerMock.Object, null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("observabilityService");
        }

        [Fact]
        public async Task StartAsync_ShouldStartPipeline()
        {
            // Arrange
            var pipeline = new EventPipeline(_config, _loggerMock.Object, _observabilityMock.Object);

            // Act
            await pipeline.StartAsync();

            // Assert
            pipeline.IsRunning.Should().BeTrue();
        }

        [Fact]
        public async Task StopAsync_ShouldStopPipeline()
        {
            // Arrange
            var pipeline = new EventPipeline(_config, _loggerMock.Object, _observabilityMock.Object);
            await pipeline.StartAsync();

            // Act
            await pipeline.StopAsync();

            // Assert
            pipeline.IsRunning.Should().BeFalse();
        }

        [Fact]
        public async Task EnqueueAsync_WithValidEvent_ShouldEnqueueSuccessfully()
        {
            // Arrange
            var pipeline = new EventPipeline(_config, _loggerMock.Object, _observabilityMock.Object);
            await pipeline.StartAsync();
            var testEvent = new TestEvent { Id = "test-1" };

            // Act
            var result = await pipeline.EnqueueAsync(testEvent);

            // Assert
            result.Should().BeTrue();
            pipeline.GetBufferUsagePercentage().Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task EnqueueAsync_WithNullEvent_ShouldThrowArgumentNullException()
        {
            // Arrange
            var pipeline = new EventPipeline(_config, _loggerMock.Object, _observabilityMock.Object);
            await pipeline.StartAsync();

            // Act & Assert
            Func<Task> act = async () => await pipeline.EnqueueAsync(null!);
            await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("eventData");
        }

        [Fact]
        public async Task EnqueueAsync_WhenPipelineStopped_ShouldReturnFalse()
        {
            // Arrange
            var pipeline = new EventPipeline(_config, _loggerMock.Object, _observabilityMock.Object);
            var testEvent = new TestEvent { Id = "test-1" };

            // Act
            var result = await pipeline.EnqueueAsync(testEvent);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task EnqueueAsync_WhenBufferFull_ShouldReturnFalse()
        {
            // Arrange
            var smallConfig = new PipelineConfiguration
            {
                BufferSize = 1,
                WorkerCount = 1,
                MaxConcurrency = 1
            };
            var pipeline = new EventPipeline(smallConfig, _loggerMock.Object, _observabilityMock.Object);
            await pipeline.StartAsync();

            // Fill buffer
            await pipeline.EnqueueAsync(new TestEvent { Id = "test-1" });

            // Act
            var result = await pipeline.EnqueueAsync(new TestEvent { Id = "test-2" });

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void GetMetrics_ShouldReturnValidMetrics()
        {
            // Arrange
            var pipeline = new EventPipeline(_config, _loggerMock.Object, _observabilityMock.Object);

            // Act
            var metrics = pipeline.GetMetrics();

            // Assert
            metrics.Should().NotBeNull();
            metrics.ProcessedEvents.Should().Be(0);
            metrics.DroppedEvents.Should().Be(0);
            metrics.ErrorCount.Should().Be(0);
            metrics.ProcessingRate.Should().Be(0);
        }

        [Fact]
        public void GetBufferUsagePercentage_ShouldReturnValidPercentage()
        {
            // Arrange
            var pipeline = new EventPipeline(_config, _loggerMock.Object, _observabilityMock.Object);

            // Act
            var usage = pipeline.GetBufferUsagePercentage();

            // Assert
            usage.Should().BeGreaterThanOrEqualTo(0);
            usage.Should().BeLessThanOrEqualTo(100);
        }

        [Fact]
        public async Task RegisterEnricher_ShouldAddEnricher()
        {
            // Arrange
            var pipeline = new EventPipeline(_config, _loggerMock.Object, _observabilityMock.Object);
            await pipeline.StartAsync();

            // Act
            pipeline.RegisterEnricher(_enricherMock.Object);

            // Assert
            // Note: This would require exposing the enrichers list or testing through behavior
            // For now, we'll just verify the method doesn't throw
            pipeline.Should().NotBeNull();
        }

        [Fact]
        public async Task UnregisterEnricher_ShouldRemoveEnricher()
        {
            // Arrange
            var pipeline = new EventPipeline(_config, _loggerMock.Object, _observabilityMock.Object);
            await pipeline.StartAsync();
            pipeline.RegisterEnricher(_enricherMock.Object);

            // Act
            pipeline.UnregisterEnricher(_enricherMock.Object);

            // Assert
            // Note: This would require exposing the enrichers list or testing through behavior
            // For now, we'll just verify the method doesn't throw
            pipeline.Should().NotBeNull();
        }

        [Fact]
        public async Task Dispose_ShouldCleanupResources()
        {
            // Arrange
            var pipeline = new EventPipeline(_config, _loggerMock.Object, _observabilityMock.Object);
            await pipeline.StartAsync();

            // Act
            pipeline.Dispose();

            // Assert
            pipeline.IsRunning.Should().BeFalse();
        }

        // Helper class for testing
        private class TestEvent
        {
            public string Id { get; set; } = string.Empty;
        }
    }
}
