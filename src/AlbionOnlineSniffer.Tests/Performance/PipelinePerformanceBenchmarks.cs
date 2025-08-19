using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Pipeline;
using AlbionOnlineSniffer.Core.Observability;
using AlbionOnlineSniffer.Core.Observability.Metrics;
using AlbionOnlineSniffer.Core.Observability.HealthChecks;
using AlbionOnlineSniffer.Core.Observability.Tracing;

namespace AlbionOnlineSniffer.Tests.Performance
{
    [MemoryDiagnoser]
    [SimpleJob]
    public class PipelinePerformanceBenchmarks
    {
        private ServiceProvider _serviceProvider = null!;
        private IEventPipeline _pipeline = null!;
        private IObservabilityService _observabilityService = null!;
        private TestEvent[] _testEvents = null!;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var services = new ServiceCollection();

            // Add logging
            services.AddLogging(builder => builder.AddConsole());

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
                    BufferSize = 10000,
                    WorkerCount = Environment.ProcessorCount,
                    MaxConcurrency = Environment.ProcessorCount * 2
                };
                return new EventPipeline(
                    config,
                    provider.GetRequiredService<ILogger<EventPipeline>>(),
                    provider.GetRequiredService<IObservabilityService>());
            });

            _serviceProvider = services.BuildServiceProvider();
            _pipeline = _serviceProvider.GetRequiredService<IEventPipeline>();
            _observabilityService = _serviceProvider.GetRequiredService<IObservabilityService>();

            // Initialize
            _observabilityService.InitializeAsync().Wait();
            _pipeline.StartAsync().Wait();

            // Prepare test events
            _testEvents = new TestEvent[1000];
            for (int i = 0; i < 1000; i++)
            {
                _testEvents[i] = new TestEvent
                {
                    Id = $"benchmark-{i}",
                    Tier = i % 8,
                    Data = new byte[i % 100]
                };
            }
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            _pipeline?.StopAsync().Wait();
            _serviceProvider?.Dispose();
        }

        [Benchmark]
        public async Task Pipeline_EnqueueSingleEvent()
        {
            var testEvent = new TestEvent { Id = "single", Tier = 5 };
            await _pipeline.EnqueueAsync(testEvent);
        }

        [Benchmark]
        public async Task Pipeline_EnqueueMultipleEvents()
        {
            for (int i = 0; i < 100; i++)
            {
                var testEvent = new TestEvent { Id = $"multi-{i}", Tier = i % 8 };
                await _pipeline.EnqueueAsync(testEvent);
            }
        }

        [Benchmark]
        public async Task Pipeline_EnqueueBatchEvents()
        {
            var tasks = new Task<bool>[100];
            for (int i = 0; i < 100; i++)
            {
                var testEvent = new TestEvent { Id = $"batch-{i}", Tier = i % 8 };
                tasks[i] = _pipeline.EnqueueAsync(testEvent);
            }
            await Task.WhenAll(tasks);
        }

        [Benchmark]
        public void Pipeline_GetMetrics()
        {
            var metrics = _pipeline.GetMetrics();
            _ = metrics.ProcessedEvents;
            _ = metrics.DroppedEvents;
            _ = metrics.ErrorCount;
            _ = metrics.ProcessingRate;
        }

        [Benchmark]
        public void Pipeline_GetBufferUsage()
        {
            var usage = _pipeline.GetBufferUsagePercentage();
            _ = usage;
        }

        [Benchmark]
        public async Task Observability_StartTrace()
        {
            var trace = _observabilityService.StartTrace("BenchmarkOperation", "benchmark-123");
            _observabilityService.EndTrace("Ok");
        }

        [Benchmark]
        public void Observability_RecordMetric()
        {
            _observabilityService.RecordMetric("benchmark.metric", 42.5);
        }

        [Benchmark]
        public void Observability_IncrementCounter()
        {
            _observabilityService.IncrementCounter("benchmark.counter");
        }

        [Benchmark]
        public async Task Observability_CheckHealth()
        {
            var health = await _observabilityService.CheckHealthAsync();
            _ = health.Status;
        }

        [Benchmark]
        public void Observability_GetMetrics()
        {
            var metrics = _observabilityService.GetJsonMetrics();
            _ = metrics;
        }

        [Benchmark]
        public async Task Pipeline_StressTest_1000Events()
        {
            var tasks = new Task<bool>[1000];
            for (int i = 0; i < 1000; i++)
            {
                tasks[i] = _pipeline.EnqueueAsync(_testEvents[i]);
            }
            await Task.WhenAll(tasks);
        }

        [Benchmark]
        public async Task Pipeline_ThroughputTest_10000Events()
        {
            var batchSize = 1000;
            var totalEvents = 10000;
            
            for (int batch = 0; batch < totalEvents / batchSize; batch++)
            {
                var tasks = new Task<bool>[batchSize];
                for (int i = 0; i < batchSize; i++)
                {
                    var eventIndex = (batch * batchSize + i) % _testEvents.Length;
                    tasks[i] = _pipeline.EnqueueAsync(_testEvents[eventIndex]);
                }
                await Task.WhenAll(tasks);
            }
        }

        // Helper class for testing
        private class TestEvent
        {
            public string Id { get; set; } = string.Empty;
            public int Tier { get; set; }
            public byte[] Data { get; set; } = Array.Empty<byte>();
        }
    }

    // Benchmark runner
    public class PipelineBenchmarkRunner
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<PipelinePerformanceBenchmarks>();
            Console.WriteLine(summary);
        }
    }
}
