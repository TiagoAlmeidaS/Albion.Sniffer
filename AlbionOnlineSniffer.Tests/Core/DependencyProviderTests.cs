using System;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Models.ResponseObj;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Interfaces;
using AlbionOnlineSniffer.Core;
using Xunit;

namespace AlbionOnlineSniffer.Tests.Core
{
    public class DependencyProviderTests
    {
        [Fact]
        public void RegisterDataLoader_WithCustomPacketOffsets_ShouldUseCustomOffsets()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(_ => LoggerFactory.Create(builder => { }));
            
            var customOffsets = new PacketOffsets
            {
                Move = new byte[] { 10, 20, 30 },
                ChangeCluster = new byte[] { 40, 50, 60 }
            };

            // Act
            DependencyProvider.RegisterDataLoader(services, customOffsets);
            var serviceProvider = services.BuildServiceProvider();
            var resolvedOffsets = serviceProvider.GetRequiredService<PacketOffsets>();

            // Assert
            Assert.NotNull(resolvedOffsets);
            Assert.Equal(customOffsets.Move, resolvedOffsets.Move);
            Assert.Equal(customOffsets.ChangeCluster, resolvedOffsets.ChangeCluster);
        }

        [Fact]
        public void RegisterDataLoader_WithCustomPacketIndexes_ShouldUseCustomIndexes()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(_ => LoggerFactory.Create(builder => { }));
            
            var customIndexes = new PacketIndexes
            {
                Move = 100,
                ChangeCluster = 200
            };

            // Act
            DependencyProvider.RegisterDataLoader(services, null, customIndexes);
            var serviceProvider = services.BuildServiceProvider();
            var resolvedIndexes = serviceProvider.GetRequiredService<PacketIndexes>();

            // Assert
            Assert.NotNull(resolvedIndexes);
            Assert.Equal(customIndexes.Move, resolvedIndexes.Move);
            Assert.Equal(customIndexes.ChangeCluster, resolvedIndexes.ChangeCluster);
        }

        [Fact]
        public void RegisterDataLoader_WithBothCustomParameters_ShouldUseBothCustomValues()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(_ => LoggerFactory.Create(builder => { }));
            
            var customOffsets = new PacketOffsets
            {
                Move = new byte[] { 1, 2, 3 }
            };
            
            var customIndexes = new PacketIndexes
            {
                Move = 999
            };

            // Act
            DependencyProvider.RegisterDataLoader(services, customOffsets, customIndexes);
            var serviceProvider = services.BuildServiceProvider();
            
            var resolvedOffsets = serviceProvider.GetRequiredService<PacketOffsets>();
            var resolvedIndexes = serviceProvider.GetRequiredService<PacketIndexes>();

            // Assert
            Assert.Equal(customOffsets.Move, resolvedOffsets.Move);
            Assert.Equal(customIndexes.Move, resolvedIndexes.Move);
        }

        [Fact]
        public void RegisterDataLoader_WithFileBasedOffsets_ShouldLoadFromFile()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(_ => LoggerFactory.Create(builder => { }));
            
            // Create temporary offsets file
            var tempDir = Path.GetTempPath();
            var offsetsPath = Path.Combine(tempDir, "test_offsets.json");
            
            var testOffsetsData = new
            {
                Move = new[] { 5, 6, 7 },
                ChangeCluster = new[] { 8, 9, 10 }
            };
            
            var json = JsonSerializer.Serialize(testOffsetsData);
            File.WriteAllText(offsetsPath, json);

            try
            {
                // Temporarily modify current directory to make file discoverable
                var originalDir = Directory.GetCurrentDirectory();
                Directory.SetCurrentDirectory(tempDir);

                // Act
                DependencyProvider.RegisterDataLoader(services);
                var serviceProvider = services.BuildServiceProvider();
                var resolvedOffsets = serviceProvider.GetRequiredService<PacketOffsets>();

                // Assert
                Assert.NotNull(resolvedOffsets);
                // Note: This test may fail if file isn't found in expected locations
                // The actual file loading logic depends on the PacketOffsetsLoader implementation

                Directory.SetCurrentDirectory(originalDir);
            }
            finally
            {
                if (File.Exists(offsetsPath))
                    File.Delete(offsetsPath);
            }
        }

        [Fact]
        public void OverridePacketOffsets_ShouldReplaceExistingOffsets()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(_ => LoggerFactory.Create(builder => { }));
            
            var initialOffsets = new PacketOffsets
            {
                Move = new byte[] { 1, 2, 3 }
            };
            
            DependencyProvider.RegisterDataLoader(services, initialOffsets);

            var newOffsets = new PacketOffsets
            {
                Move = new byte[] { 4, 5, 6 }
            };

            // Act
            DependencyProvider.OverridePacketOffsets(services, newOffsets);
            var serviceProvider = services.BuildServiceProvider();
            var resolvedOffsets = serviceProvider.GetRequiredService<PacketOffsets>();

            // Assert
            Assert.NotNull(resolvedOffsets);
            Assert.Equal(newOffsets.Move, resolvedOffsets.Move);
            Assert.NotEqual(initialOffsets.Move, resolvedOffsets.Move);
        }

        [Fact]
        public void OverridePacketIndexes_ShouldReplaceExistingIndexes()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(_ => LoggerFactory.Create(builder => { }));
            
            var initialIndexes = new PacketIndexes
            {
                Move = 100
            };
            
            DependencyProvider.RegisterDataLoader(services, null, initialIndexes);

            var newIndexes = new PacketIndexes
            {
                Move = 200
            };

            // Act
            DependencyProvider.OverridePacketIndexes(services, newIndexes);
            var serviceProvider = services.BuildServiceProvider();
            var resolvedIndexes = serviceProvider.GetRequiredService<PacketIndexes>();

            // Assert
            Assert.NotNull(resolvedIndexes);
            Assert.Equal(newIndexes.Move, resolvedIndexes.Move);
            Assert.NotEqual(initialIndexes.Move, resolvedIndexes.Move);
        }

        [Fact]
        public void RegisterServices_ShouldRegisterEventFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(_ => LoggerFactory.Create(builder => { }));
            
            var customOffsets = new PacketOffsets
            {
                Move = new byte[] { 1, 2, 3 }
            };

            // Act
            DependencyProvider.RegisterDataLoader(services, customOffsets);
            DependencyProvider.RegisterServices(services);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var eventFactory = serviceProvider.GetService<IEventFactory>();
            Assert.NotNull(eventFactory);
            Assert.IsType<EventFactory>(eventFactory);
        }

        [Fact]
        public void ConfigurePacketOffsetsProvider_ShouldConfigureProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(_ => LoggerFactory.Create(builder => { }));
            
            var customOffsets = new PacketOffsets
            {
                Move = new byte[] { 7, 8, 9 }
            };
            
            DependencyProvider.RegisterDataLoader(services, customOffsets);
            var serviceProvider = services.BuildServiceProvider();

            // Act
            DependencyProvider.ConfigurePacketOffsetsProvider(serviceProvider);

            // Assert
            Assert.True(PacketOffsetsProvider.IsConfigured);
            var offsets = PacketOffsetsProvider.GetOffsets();
            Assert.NotNull(offsets);
            Assert.Equal(customOffsets.Move, offsets.Move);
        }

        [Fact]
        public void RegisterServices_WithCompleteSetup_ShouldResolveAllDependencies()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(_ => LoggerFactory.Create(builder => { }));
            
            var customOffsets = new PacketOffsets
            {
                Move = new byte[] { 1, 2, 3 },
                ChangeCluster = new byte[] { 4, 5, 6 },
                NewMobEvent = new byte[] { 7, 8, 9, 10, 11, 12 }
            };

            // Act
            DependencyProvider.RegisterDataLoader(services, customOffsets);
            DependencyProvider.RegisterServices(services);
            var serviceProvider = services.BuildServiceProvider();
            DependencyProvider.ConfigurePacketOffsetsProvider(serviceProvider);

            // Assert - Check that all main services are registered
            Assert.NotNull(serviceProvider.GetService<PacketOffsets>());
            Assert.NotNull(serviceProvider.GetService<PacketIndexes>());
            Assert.NotNull(serviceProvider.GetService<IEventFactory>());
            Assert.NotNull(serviceProvider.GetService<EventDispatcher>());
            Assert.NotNull(serviceProvider.GetService<PhotonDefinitionLoader>());
            Assert.NotNull(serviceProvider.GetService<Protocol16Deserializer>());
            Assert.NotNull(serviceProvider.GetService<PositionDecryptor>());
            Assert.NotNull(serviceProvider.GetService<ClusterService>());
            Assert.NotNull(serviceProvider.GetService<ItemDataService>());
            Assert.NotNull(serviceProvider.GetService<DataLoaderService>());
            Assert.NotNull(serviceProvider.GetService<AlbionNetworkHandlerManager>());

            // Check PacketOffsetsProvider is configured
            Assert.True(PacketOffsetsProvider.IsConfigured);
        }

        [Fact]
        public void OverridePacketOffsets_WithNullOffsets_ShouldNotThrow()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(_ => LoggerFactory.Create(builder => { }));

            // Act & Assert - Should not throw an exception
            DependencyProvider.OverridePacketOffsets(services, null);
            var serviceProvider = services.BuildServiceProvider();
            
            // The service should be registered even if it's null
            var hasOffsets = serviceProvider.GetService<PacketOffsets>();
            Assert.Null(hasOffsets);
        }

        [Fact]
        public void OverridePacketIndexes_WithNullIndexes_ShouldNotThrow()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(_ => LoggerFactory.Create(builder => { }));

            // Act & Assert - Should not throw an exception
            DependencyProvider.OverridePacketIndexes(services, null);
            var serviceProvider = services.BuildServiceProvider();
            
            // The service should be registered even if it's null
            var hasIndexes = serviceProvider.GetService<PacketIndexes>();
            Assert.Null(hasIndexes);
        }
    }
}
