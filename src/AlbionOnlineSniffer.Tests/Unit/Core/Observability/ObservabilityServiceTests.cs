using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using AlbionOnlineSniffer.Core.Observability;
using AlbionOnlineSniffer.Core.Observability.Metrics;
using AlbionOnlineSniffer.Core.Observability.HealthChecks;
using AlbionOnlineSniffer.Core.Observability.Tracing;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace AlbionOnlineSniffer.Tests.Unit.Core.Observability
{
    public class ObservabilityServiceTests
    {
        private readonly Mock<ILogger<ObservabilityService>> _loggerMock;
        private readonly Mock<IMetricsCollector> _metricsCollectorMock;
        private readonly Mock<IHealthCheckService> _healthCheckServiceMock;
        private readonly Mock<ITracingService> _tracingServiceMock;
        private readonly ObservabilityService _observabilityService;

        public ObservabilityServiceTests()
        {
            _loggerMock = new Mock<ILogger<ObservabilityService>>();
            _metricsCollectorMock = new Mock<IMetricsCollector>();
            _healthCheckServiceMock = new Mock<IHealthCheckService>();
            _tracingServiceMock = new Mock<ITracingService>();
            
            _observabilityService = new ObservabilityService(
                _loggerMock.Object,
                _metricsCollectorMock.Object,
                _healthCheckServiceMock.Object,
                _tracingServiceMock.Object);
        }

        [Fact]
        public void Constructor_WithValidDependencies_ShouldCreateInstance()
        {
            // Assert
            _observabilityService.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Action act = () => new ObservabilityService(
                null!, 
                _metricsCollectorMock.Object, 
                _healthCheckServiceMock.Object, 
                _tracingServiceMock.Object);
            
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Fact]
        public void Constructor_WithNullMetricsCollector_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Action act = () => new ObservabilityService(
                _loggerMock.Object, 
                null!, 
                _healthCheckServiceMock.Object, 
                _tracingServiceMock.Object);
            
            act.Should().Throw<ArgumentNullException>().WithParameterName("metricsCollector");
        }

        [Fact]
        public void Constructor_WithNullHealthCheckService_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Action act = () => new ObservabilityService(
                _loggerMock.Object, 
                _metricsCollectorMock.Object, 
                null!, 
                _tracingServiceMock.Object);
            
            act.Should().Throw<ArgumentNullException>().WithParameterName("healthCheckService");
        }

        [Fact]
        public void Constructor_WithNullTracingService_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Action act = () => new ObservabilityService(
                _loggerMock.Object, 
                _metricsCollectorMock.Object, 
                _healthCheckServiceMock.Object, 
                null!);
            
            act.Should().Throw<ArgumentNullException>().WithParameterName("tracingService");
        }

        [Fact]
        public async Task InitializeAsync_ShouldInitializeAllServices()
        {
            // Arrange
            _tracingServiceMock
                .Setup(x => x.InitializeAsync())
                .Returns(Task.CompletedTask);
            
            _healthCheckServiceMock
                .Setup(x => x.RunHealthChecksAsync())
                .ReturnsAsync(new HealthReport { Status = "Healthy" });

            // Act
            await _observabilityService.InitializeAsync();

            // Assert
            _tracingServiceMock.Verify(x => x.InitializeAsync(), Times.Once);
            _healthCheckServiceMock.Verify(x => x.RunHealthChecksAsync(), Times.Once);
            _metricsCollectorMock.Verify(x => x.IncrementCounter(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task InitializeAsync_WhenTracingServiceThrows_ShouldPropagateException()
        {
            // Arrange
            var expectedException = new InvalidOperationException("Test exception");
            _tracingServiceMock
                .Setup(x => x.InitializeAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _observabilityService.InitializeAsync());
            
            exception.Should().Be(expectedException);
        }

        [Fact]
        public void StartTrace_ShouldCallTracingService()
        {
            // Arrange
            var operationName = "TestOperation";
            var operationId = "test-123";

            // Act
            _observabilityService.StartTrace(operationName, operationId);

            // Assert
            _tracingServiceMock.Verify(x => x.StartTrace(operationName, operationId), Times.Once);
            _metricsCollectorMock.Verify(x => x.IncrementCounter("traces.started", It.IsAny<KeyValuePair<string, object?>>()), Times.Once);
        }

        [Fact]
        public void StartTrace_WithNullOperationName_ShouldHandleGracefully()
        {
            // Act
            var result = _observabilityService.StartTrace(null!);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void RecordMetric_ShouldCallMetricsCollector()
        {
            // Arrange
            var metricName = "test.metric";
            var metricValue = 42.5;

            // Act
            _observabilityService.RecordMetric(metricName, metricValue);

            // Assert
            _metricsCollectorMock.Verify(x => x.RecordValue(metricName, metricValue), Times.Once);
        }

        [Fact]
        public void IncrementCounter_ShouldCallMetricsCollector()
        {
            // Arrange
            var counterName = "test.counter";

            // Act
            _observabilityService.IncrementCounter(counterName);

            // Assert
            _metricsCollectorMock.Verify(x => x.IncrementCounter(counterName), Times.Once);
        }

        [Fact]
        public void IncrementCounter_WithTags_ShouldCallMetricsCollectorWithTags()
        {
            // Arrange
            var counterName = "test.counter";
            var tags = new[] { new KeyValuePair<string, object?>("tag1", "value1") };

            // Act
            _observabilityService.IncrementCounter(counterName, tags);

            // Assert
            _metricsCollectorMock.Verify(x => x.IncrementCounter(counterName, tags), Times.Once);
        }

        [Fact]
        public void RecordBusinessEvent_ShouldRecordMetricsAndTracing()
        {
            // Arrange
            var eventName = "TestEvent";
            var category = "TestCategory";
            var tags = new[] { new KeyValuePair<string, object?>("tag1", "value1") };

            _tracingServiceMock
                .Setup(x => x.GetCurrentTrace())
                .Returns((Activity?)null);

            // Act
            _observabilityService.RecordBusinessEvent(eventName, category, tags);

            // Assert
            _metricsCollectorMock.Verify(x => x.IncrementCounter("business.events", 
                It.IsAny<KeyValuePair<string, object?>>(), 
                It.IsAny<KeyValuePair<string, object?>>()), Times.Once);
        }

        [Fact]
        public async Task CheckHealthAsync_ShouldReturnHealthReport()
        {
            // Arrange
            var expectedReport = new HealthReport { Status = "Healthy" };
            _healthCheckServiceMock
                .Setup(x => x.RunHealthChecksAsync())
                .ReturnsAsync(expectedReport);

            // Act
            var result = await _observabilityService.CheckHealthAsync();

            // Assert
            result.Should().Be(expectedReport);
            _metricsCollectorMock.Verify(x => x.IncrementCounter("health.checks.executed"), Times.Once);
            _metricsCollectorMock.Verify(x => x.SetGauge("health.status", It.IsAny<double>()), Times.Once);
        }

        [Fact]
        public async Task CheckHealthAsync_WhenHealthCheckServiceThrows_ShouldPropagateException()
        {
            // Arrange
            var expectedException = new InvalidOperationException("Health check failed");
            _healthCheckServiceMock
                .Setup(x => x.RunHealthChecksAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _observabilityService.CheckHealthAsync());
            
            exception.Should().Be(expectedException);
        }

        [Fact]
        public void GetPrometheusMetrics_ShouldReturnMetricsFromCollector()
        {
            // Arrange
            var expectedMetrics = "# Test metrics";
            _metricsCollectorMock
                .Setup(x => x.GetPrometheusMetrics())
                .Returns(expectedMetrics);

            // Act
            var result = _observabilityService.GetPrometheusMetrics();

            // Assert
            result.Should().Be(expectedMetrics);
        }

        [Fact]
        public void GetJsonMetrics_ShouldReturnMetricsFromCollector()
        {
            // Arrange
            var expectedMetrics = new { test = "metrics" };
            _metricsCollectorMock
                .Setup(x => x.GetJsonMetrics())
                .Returns(expectedMetrics);

            // Act
            var result = _observabilityService.GetJsonMetrics();

            // Assert
            result.Should().Be(expectedMetrics);
        }

        [Fact]
        public void ClearOldMetrics_ShouldCallMetricsCollector()
        {
            // Arrange
            var age = TimeSpan.FromHours(1);

            // Act
            _observabilityService.ClearOldMetrics(age);

            // Assert
            _metricsCollectorMock.Verify(x => x.ClearOldMetrics(age), Times.Once);
        }

        [Fact]
        public void Dispose_ShouldCleanupResources()
        {
            // Act
            _observabilityService.Dispose();

            // Assert
            // Note: We can't easily verify timer disposal without exposing internals
            // The main goal is to ensure Dispose doesn't throw
            _observabilityService.Should().NotBeNull();
        }

        [Fact]
        public void Dispose_MultipleTimes_ShouldNotThrow()
        {
            // Act & Assert
            _observabilityService.Dispose();
            Action act = () => _observabilityService.Dispose();
            act.Should().NotThrow();
        }
    }
}
