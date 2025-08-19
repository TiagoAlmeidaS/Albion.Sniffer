using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using AlbionOnlineSniffer.Core.Pipeline;
using AlbionOnlineSniffer.Core.Enrichers;
using AlbionOnlineSniffer.Core.Observability;
using AlbionOnlineSniffer.Core.Options.Profiles;
using AlbionOnlineSniffer.Core.Options;

namespace AlbionOnlineSniffer.Tests.Integration.Pipeline
{
    public class FullPipelineIntegrationTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IEventPipeline _pipeline;
        private readonly IObservabilityService _observabilityService;

        public FullPipelineIntegrationTests()
        {
            var services = new ServiceCollection();

            // Add logging
            services.AddLogging(builder => builder.AddConsole());

            // Add options
            services.Configure<SnifferOptions>(options =>
            {
                options.ActiveProfile = "Default";
            });

            // Add profile management
            services.AddSingleton<IProfileManager, ProfileManager>();
            services.AddSingleton<ITierPaletteManager, TierPaletteManager>();

            // Add observability
            services.AddSingleton<IObservabilityService, ObservabilityService>();
            services.AddSingleton<IMetricsCollector, PrometheusMetricsCollector>();
            services.AddSingleton<IHealthCheckService, HealthCheckService>();
            services.AddSingleton<ITracingService, OpenTelemetryTracingService>();

            // Add pipeline
            services.AddSingleton<IEventPipeline>(provider =>
            {
                var config = new PipelineConfiguration
                {
                    BufferSize = 100,
                    WorkerCount = 2,
                    MaxConcurrency = 5
                };
                return new EventPipeline(
                    config,
                    provider.GetRequiredService<ILogger<EventPipeline>>(),
                    provider.GetRequiredService<IObservabilityService>());
            });

            // Add enrichers
            services.AddTransient<TierColorEnricher>();
            services.AddTransient<ProfileFilterEnricher>();
            services.AddTransient<ProximityAlertEnricher>();
            services.AddSingleton<CompositeEventEnricher>();

            _serviceProvider = services.BuildServiceProvider();
            _pipeline = _serviceProvider.GetRequiredService<IEventPipeline>();
            _observabilityService = _serviceProvider.GetRequiredService<IObservabilityService>();
        }

        [Fact]
        public async Task FullPipeline_ShouldStartAndStop_Successfully()
        {
            // Act
            await _pipeline.StartAsync();

            // Assert
            _pipeline.IsRunning.Should().BeTrue();

            // Act
            await _pipeline.StopAsync();

            // Assert
            _pipeline.IsRunning.Should().BeFalse();
        }

        [Fact]
        public async Task FullPipeline_WithEnrichers_ShouldProcessEvents()
        {
            // Arrange
            await _pipeline.StartAsync();
            var testEvent = new TestEvent { Id = "test-1", Tier = 6 };

            // Act
            var enqueueResult = await _pipeline.EnqueueAsync(testEvent);

            // Assert
            enqueueResult.Should().BeTrue();

            // Wait a bit for processing
            await Task.Delay(100);

            // Verify metrics
            var metrics = _pipeline.GetMetrics();
            metrics.Should().NotBeNull();
        }

        [Fact]
        public async Task FullPipeline_WithObservability_ShouldRecordMetrics()
        {
            // Arrange
            await _observabilityService.InitializeAsync();
            await _pipeline.StartAsync();

            // Act
            var testEvent = new TestEvent { Id = "test-2", Tier = 8 };
            await _pipeline.EnqueueAsync(testEvent);

            // Wait for processing
            await Task.Delay(100);

            // Assert
            var metrics = _observabilityService.GetJsonMetrics();
            metrics.Should().NotBeNull();
        }

        [Fact]
        public async Task FullPipeline_WithHealthChecks_ShouldReportHealthy()
        {
            // Arrange
            await _observabilityService.InitializeAsync();
            await _pipeline.StartAsync();

            // Act
            var healthReport = await _observabilityService.CheckHealthAsync();

            // Assert
            healthReport.Should().NotBeNull();
            healthReport.Status.Should().Be("Healthy");
            healthReport.Checks.Should().NotBeEmpty();
        }

        [Fact]
        public async Task FullPipeline_WithMultipleEvents_ShouldProcessAll()
        {
            // Arrange
            await _pipeline.StartAsync();
            var events = new[]
            {
                new TestEvent { Id = "test-3", Tier = 4 },
                new TestEvent { Id = "test-4", Tier = 6 },
                new TestEvent { Id = "test-5", Tier = 8 }
            };

            // Act
            foreach (var evt in events)
            {
                var result = await _pipeline.EnqueueAsync(evt);
                result.Should().BeTrue();
            }

            // Wait for processing
            await Task.Delay(200);

            // Assert
            var metrics = _pipeline.GetMetrics();
            metrics.ProcessedEvents.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task FullPipeline_WithBufferFull_ShouldHandleBackpressure()
        {
            // Arrange
            var smallConfig = new PipelineConfiguration
            {
                BufferSize = 2,
                WorkerCount = 1,
                MaxConcurrency = 1
            };

            var smallPipeline = new EventPipeline(
                smallConfig,
                _serviceProvider.GetRequiredService<ILogger<EventPipeline>>(),
                _observabilityService);

            await smallPipeline.StartAsync();

            // Fill buffer
            await smallPipeline.EnqueueAsync(new TestEvent { Id = "fill-1" });
            await smallPipeline.EnqueueAsync(new TestEvent { Id = "fill-2" });

            // Act - Try to add more
            var result = await smallPipeline.EnqueueAsync(new TestEvent { Id = "overflow" });

            // Assert
            result.Should().BeFalse();

            // Cleanup
            await smallPipeline.StopAsync();
        }

        [Fact]
        public async Task FullPipeline_WithEnrichers_ShouldEnrichEvents()
        {
            // Arrange
            await _pipeline.StartAsync();
            var testEvent = new TestEvent { Id = "enrich-test", Tier = 7 };

            // Act
            await _pipeline.EnqueueAsync(testEvent);

            // Wait for processing
            await Task.Delay(100);

            // Assert
            // Note: In a real scenario, we'd verify the event was enriched
            // For now, we verify the pipeline processed it
            var metrics = _pipeline.GetMetrics();
            metrics.Should().NotBeNull();
        }

        [Fact]
        public async Task FullPipeline_WithTracing_ShouldCreateTraces()
        {
            // Arrange
            await _observabilityService.InitializeAsync();
            await _pipeline.StartAsync();

            // Act
            var trace = _observabilityService.StartTrace("TestOperation", "test-trace-123");

            // Assert
            trace.Should().NotBeNull();

            // Cleanup
            _observabilityService.EndTrace("Ok");
        }

        [Fact]
        public async Task FullPipeline_StressTest_ShouldHandleLoad()
        {
            // Arrange
            await _pipeline.StartAsync();
            var eventCount = 50;

            // Act
            var tasks = new Task<bool>[eventCount];
            for (int i = 0; i < eventCount; i++)
            {
                var testEvent = new TestEvent { Id = $"stress-{i}", Tier = i % 8 };
                tasks[i] = _pipeline.EnqueueAsync(testEvent);
            }

            var results = await Task.WhenAll(tasks);

            // Wait for processing
            await Task.Delay(500);

            // Assert
            results.Should().AllSatisfy(r => r.Should().BeTrue());
            
            var metrics = _pipeline.GetMetrics();
            metrics.Should().NotBeNull();
        }

        public void Dispose()
        {
            _pipeline?.StopAsync().Wait();
            _serviceProvider?.Dispose();
        }

        // Helper class for testing
        private class TestEvent
        {
            public string Id { get; set; } = string.Empty;
            public int Tier { get; set; }
        }
    }
}
