using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Pipeline;
using AlbionOnlineSniffer.Core.Observability;
using AlbionOnlineSniffer.Core.Observability.Metrics;
using AlbionOnlineSniffer.Core.Observability.HealthChecks;
using AlbionOnlineSniffer.Core.Observability.Tracing;
using AlbionOnlineSniffer.Core.Enrichers;
using AlbionOnlineSniffer.Core.Options.Profiles;
using AlbionOnlineSniffer.Core.Options;
using System.Threading.Tasks;

namespace AlbionOnlineSniffer.Tests
{
    /// <summary>
    /// Configuração centralizada para testes
    /// </summary>
    public static class TestConfiguration
    {
        /// <summary>
        /// Cria um container DI configurado para testes
        /// </summary>
        public static ServiceProvider CreateTestServiceProvider()
        {
            var services = new ServiceCollection();

            // Logging
            services.AddLogging(builder => 
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });

            // Options
            services.Configure<SnifferOptions>(options =>
            {
                options.ActiveProfile = "Default";
                options.LogLevel = LogLevel.Debug;
            });

            // Profile Management
            services.AddSingleton<IProfileManager, ProfileManager>();
            services.AddSingleton<ITierPaletteManager, TierPaletteManager>();

            // Observability
            services.AddSingleton<IObservabilityService, ObservabilityService>();
            services.AddSingleton<IMetricsCollector, PrometheusMetricsCollector>();
            services.AddSingleton<IHealthCheckService, HealthCheckService>();
            services.AddSingleton<ITracingService, OpenTelemetryTracingService>();

            // Pipeline
            services.AddSingleton<IEventPipeline>(provider =>
            {
                var config = new PipelineConfiguration
                {
                    BufferSize = 1000,
                    WorkerCount = 2,
                    MaxConcurrency = 5
                };
                return new EventPipeline(
                    config,
                    provider.GetRequiredService<ILogger<EventPipeline>>(),
                    provider.GetRequiredService<IObservabilityService>());
            });

            // Enrichers
            services.AddTransient<TierColorEnricher>();
            services.AddTransient<ProfileFilterEnricher>();
            services.AddTransient<ProximityAlertEnricher>();
            services.AddSingleton<CompositeEventEnricher>();

            return services.BuildServiceProvider();
        }

        /// <summary>
        /// Cria um container DI configurado para testes de performance
        /// </summary>
        public static ServiceProvider CreatePerformanceTestServiceProvider()
        {
            var services = new ServiceCollection();

            // Logging (minimal para performance)
            services.AddLogging(builder => builder.AddConsole());

            // Observability
            services.AddSingleton<IObservabilityService, ObservabilityService>();
            services.AddSingleton<IMetricsCollector, PrometheusMetricsCollector>();
            services.AddSingleton<IHealthCheckService, HealthCheckService>();
            services.AddSingleton<ITracingService, OpenTelemetryTracingService>();

            // Pipeline (otimizado para performance)
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

            return services.BuildServiceProvider();
        }

        /// <summary>
        /// Cria dados de teste padronizados
        /// </summary>
        public static class TestData
        {
            /// <summary>
            /// Cria um evento de teste básico
            /// </summary>
            public static TestEvent CreateBasicTestEvent(string id = "test-1", int tier = 5)
            {
                return new TestEvent
                {
                    Id = id,
                    Tier = tier,
                    Data = new byte[tier * 10]
                };
            }

            /// <summary>
            /// Cria múltiplos eventos de teste
            /// </summary>
            public static TestEvent[] CreateMultipleTestEvents(int count, string prefix = "test")
            {
                var events = new TestEvent[count];
                for (int i = 0; i < count; i++)
                {
                    events[i] = CreateBasicTestEvent($"{prefix}-{i}", i % 8);
                }
                return events;
            }

            /// <summary>
            /// Cria eventos de teste com diferentes tamanhos de dados
            /// </summary>
            public static TestEvent[] CreateVariableSizeTestEvents(int count, int minDataSize = 10, int maxDataSize = 1000)
            {
                var random = new Random(42); // Seed fixo para testes determinísticos
                var events = new TestEvent[count];
                
                for (int i = 0; i < count; i++)
                {
                    var dataSize = random.Next(minDataSize, maxDataSize);
                    events[i] = new TestEvent
                    {
                        Id = $"var-size-{i}",
                        Tier = random.Next(1, 9),
                        Data = new byte[dataSize]
                    };
                }
                
                return events;
            }
        }

        /// <summary>
        /// Utilitários para testes
        /// </summary>
        public static class TestUtilities
        {
            /// <summary>
            /// Aguarda até que uma condição seja verdadeira ou timeout seja atingido
            /// </summary>
            public static async Task<bool> WaitUntilAsync(Func<bool> condition, TimeSpan timeout, TimeSpan checkInterval)
            {
                var startTime = DateTime.UtcNow;
                
                while (DateTime.UtcNow - startTime < timeout)
                {
                    if (condition())
                        return true;
                    
                    await Task.Delay(checkInterval);
                }
                
                return false;
            }

            /// <summary>
            /// Aguarda até que uma condição assíncrona seja verdadeira ou timeout seja atingido
            /// </summary>
            public static async Task<bool> WaitUntilAsync(Func<Task<bool>> condition, TimeSpan timeout, TimeSpan checkInterval)
            {
                var startTime = DateTime.UtcNow;
                
                while (DateTime.UtcNow - startTime < timeout)
                {
                    if (await condition())
                        return true;
                    
                    await Task.Delay(checkInterval);
                }
                
                return false;
            }

            /// <summary>
            /// Executa uma ação com retry
            /// </summary>
            public static async Task<T> RetryAsync<T>(Func<Task<T>> action, int maxRetries = 3, TimeSpan delay = default)
            {
                if (delay == default)
                    delay = TimeSpan.FromMilliseconds(100);

                Exception? lastException = null;
                
                for (int i = 0; i <= maxRetries; i++)
                {
                    try
                    {
                        return await action();
                    }
                    catch (Exception ex) when (i < maxRetries)
                    {
                        lastException = ex;
                        await Task.Delay(delay * (i + 1)); // Exponential backoff
                    }
                }
                
                throw lastException ?? new InvalidOperationException("Retry failed");
            }

            /// <summary>
            /// Executa uma ação com retry
            /// </summary>
            public static async Task RetryAsync(Func<Task> action, int maxRetries = 3, TimeSpan delay = default)
            {
                await RetryAsync(async () =>
                {
                    await action();
                    return true;
                }, maxRetries, delay);
            }
        }

        /// <summary>
        /// Classe de evento de teste
        /// </summary>
        public class TestEvent
        {
            public string Id { get; set; } = string.Empty;
            public int Tier { get; set; }
            public byte[] Data { get; set; } = Array.Empty<byte>();
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        }
    }
}
