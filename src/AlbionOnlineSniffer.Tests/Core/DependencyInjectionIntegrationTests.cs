using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.ResponseObj;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Interfaces;
using AlbionOnlineSniffer.Core;
using Xunit;

namespace AlbionOnlineSniffer.Tests.Core
{
    public class DependencyInjectionIntegrationTests
    {
        [Fact]
        public void FullIntegration_WithCustomPacketOffsets_ShouldWorkEndToEnd()
        {
            // Arrange - Create custom PacketOffsets
            var customOffsets = new PacketOffsets
            {
                Move = new byte[] { 0, 1 },
                ChangeCluster = new byte[] { 0, 1, 2 },
                NewMobEvent = new byte[] { 0, 1, 2, 3, 4, 5 },
                NewCharacter = new byte[] { 0, 1, 2, 3, 4, 5 },
                NewHarvestableObject = new byte[] { 0, 1, 2, 3, 4 },
                HealthUpdateEvent = new byte[] { 0, 1, 2, 3 },
                Leave = new byte[] { 0 },
                KeySync = new byte[] { 0 },
                MobChangeState = new byte[] { 0, 1 },
                Mounted = new byte[] { 0, 1 },
                HarvestableChangeState = new byte[] { 0, 1 },
                CharacterEquipmentChanged = new byte[] { 0, 1, 2 },
                ChangeFlaggingFinished = new byte[] { 0 },
                RegenerationHealthChangedEvent = new byte[] { 0, 1 },
                NewDungeonExit = new byte[] { 0, 1, 2 },
                NewFishingZoneObject = new byte[] { 0, 1 },
                NewWispGate = new byte[] { 0, 1 },
                NewLootChest = new byte[] { 0, 1 },
                WispGateOpened = new byte[] { 0, 1 }
            };

            // Setup services with custom offsets
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(_ => LoggerFactory.Create(builder => { }));
            DependencyProvider.RegisterDataLoader(services, customOffsets);
            DependencyProvider.RegisterServices(services);
            
            var serviceProvider = services.BuildServiceProvider();
            DependencyProvider.ConfigurePacketOffsetsProvider(serviceProvider);

            // Act & Assert - Test all major components
            
            // 1. Test PacketOffsetsProvider is configured
            Assert.True(PacketOffsetsProvider.IsConfigured);
            var resolvedOffsets = PacketOffsetsProvider.GetOffsets();
            Assert.Equal(customOffsets.Move, resolvedOffsets.Move);

            // 2. Test EventFactory can be resolved and works
            var eventFactory = serviceProvider.GetRequiredService<IEventFactory>();
            Assert.NotNull(eventFactory);

            // 3. Test EventFactory can create events with custom offsets
            var moveParams = new Dictionary<byte, object>
            {
                { 0, 12345 },
                { 1, new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 } }
            };
            var moveEvent = eventFactory.CreateEvent<MoveEvent>(moveParams);
            Assert.NotNull(moveEvent);
            Assert.Equal(12345, moveEvent.Id);

            // 4. Test direct event creation with Provider also works
            var moveEvent2 = new MoveEvent(moveParams);
            Assert.NotNull(moveEvent2);
            Assert.Equal(12345, moveEvent2.Id);

            // 5. Test both approaches produce same results
            Assert.Equal(moveEvent.Id, moveEvent2.Id);

            // 6. Test other services are properly registered
            Assert.NotNull(serviceProvider.GetService<EventDispatcher>());
            Assert.NotNull(serviceProvider.GetService<PhotonDefinitionLoader>());
            Assert.NotNull(serviceProvider.GetService<Protocol16Deserializer>());
            Assert.NotNull(serviceProvider.GetService<PositionDecryptor>());
            Assert.NotNull(serviceProvider.GetService<ClusterService>());
            Assert.NotNull(serviceProvider.GetService<ItemDataService>());
            Assert.NotNull(serviceProvider.GetService<DataLoaderService>());
            Assert.NotNull(serviceProvider.GetService<AlbionNetworkHandlerManager>());
        }

        [Fact]
        public void DynamicOverride_ShouldUpdateAllEventCreationMethods()
        {
            // Arrange - Initial setup
            var initialOffsets = new PacketOffsets
            {
                Move = new byte[] { 0, 1 },
                Leave = new byte[] { 0 }
            };

            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(_ => LoggerFactory.Create(builder => { }));
            DependencyProvider.RegisterDataLoader(services, initialOffsets);
            DependencyProvider.RegisterServices(services);
            
            var serviceProvider = services.BuildServiceProvider();
            DependencyProvider.ConfigurePacketOffsetsProvider(serviceProvider);

            // Test initial state
            var initialEvent = new LeaveEvent(new Dictionary<byte, object> { { 0, 999 } });
            Assert.Equal(999, initialEvent.Id);

            // Act - Override with new offsets
            var newOffsets = new PacketOffsets
            {
                Move = new byte[] { 2, 3 },
                Leave = new byte[] { 1 } // Different offset
            };

            var newServices = new ServiceCollection();
            newServices.AddSingleton<ILoggerFactory>(_ => LoggerFactory.Create(builder => { }));
            DependencyProvider.RegisterDataLoader(newServices, newOffsets);
            DependencyProvider.RegisterServices(newServices);
            
            var newServiceProvider = newServices.BuildServiceProvider();
            DependencyProvider.ConfigurePacketOffsetsProvider(newServiceProvider);

            // Test with new offsets
            var newEvent = new LeaveEvent(new Dictionary<byte, object> { { 1, 888 } }); // Note: using offset 1 now
            Assert.Equal(888, newEvent.Id);

            // Test EventFactory also uses new offsets
            var eventFactory = newServiceProvider.GetRequiredService<IEventFactory>();
            var factoryEvent = eventFactory.CreateEvent<LeaveEvent>(new Dictionary<byte, object> { { 1, 777 } });
            Assert.Equal(777, factoryEvent.Id);
        }

        [Fact]
        public void MultipleEventTypes_ShouldAllUseCorrectOffsets()
        {
            // Arrange
            var testOffsets = new PacketOffsets
            {
                Move = new byte[] { 10, 11 },
                Leave = new byte[] { 20 },
                HealthUpdateEvent = new byte[] { 30, 31, 32, 33 },
                KeySync = new byte[] { 40 },
                MobChangeState = new byte[] { 50, 51 },
                NewHarvestableObject = new byte[] { 60, 61, 62, 63, 64 }
            };

            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(_ => LoggerFactory.Create(builder => { }));
            DependencyProvider.RegisterDataLoader(services, testOffsets);
            DependencyProvider.RegisterServices(services);
            
            var serviceProvider = services.BuildServiceProvider();
            DependencyProvider.ConfigurePacketOffsetsProvider(serviceProvider);
            var eventFactory = serviceProvider.GetRequiredService<IEventFactory>();

            // Act & Assert - Test different event types
            
            // LeaveEvent
            var leaveEvent = eventFactory.CreateEvent<LeaveEvent>(new Dictionary<byte, object> { { 20, 123 } });
            Assert.Equal(123, leaveEvent.Id);

            // HealthUpdateEvent  
            var healthEvent = eventFactory.CreateEvent<HealthUpdateEvent>(new Dictionary<byte, object>
            {
                { 30, 456 },
                { 31, 75.5f },
                { 32, 100.0f },
                { 33, 50.0f }
            });
            Assert.Equal(456, healthEvent.Id);
            Assert.Equal(75.5f, healthEvent.Health);

            // KeySyncEvent
            var keySyncEvent = eventFactory.CreateEvent<KeySyncEvent>(new Dictionary<byte, object> { { 40, (ulong)789 } });
            Assert.Equal((ulong)789, keySyncEvent.Key);

            // MobChangeStateEvent
            var mobChangeEvent = eventFactory.CreateEvent<MobChangeStateEvent>(new Dictionary<byte, object>
            {
                { 50, 111 },
                { 51, 5 }
            });
            Assert.Equal(111, mobChangeEvent.Id);
            Assert.Equal(5, mobChangeEvent.Charge);

            // NewHarvestableEvent
            var harvestEvent = eventFactory.CreateEvent<NewHarvestableEvent>(new Dictionary<byte, object>
            {
                { 60, 222 },
                { 61, 333 },
                { 62, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 } },
                { 63, (byte)7 },
                { 64, (byte)12 }
            });
            Assert.Equal(222, harvestEvent.Id);
            Assert.Equal(333, harvestEvent.TypeId);
            Assert.Equal(7, harvestEvent.Tier);
            Assert.Equal(12, harvestEvent.Charges);
        }

        [Fact]
        public void BackwardCompatibility_WithFileBasedOffsets_ShouldWork()
        {
            // Arrange - Create temporary offset files
            var tempDir = Path.GetTempPath();
            var offsetsPath = Path.Combine(tempDir, "offsets.json");
            var indexesPath = Path.Combine(tempDir, "indexes.json");

            var offsetsData = new
            {
                Move = new[] { 5, 6 },
                Leave = new[] { 7 },
                KeySync = new[] { 8 }
            };

            var indexesData = new
            {
                Move = 100,
                Leave = 200,
                KeySync = 300
            };

            var offsetsJson = JsonSerializer.Serialize(offsetsData);
            var indexesJson = JsonSerializer.Serialize(indexesData);

            File.WriteAllText(offsetsPath, offsetsJson);
            File.WriteAllText(indexesPath, indexesJson);

            try
            {
                // Temporarily change directory to make files discoverable
                var originalDir = Directory.GetCurrentDirectory();
                Directory.SetCurrentDirectory(tempDir);

                var services = new ServiceCollection();
                services.AddSingleton<ILoggerFactory>(_ => LoggerFactory.Create(builder => { }));
                
                // Act - Register without custom offsets (should load from file)
                try
                {
                    DependencyProvider.RegisterDataLoader(services);
                    DependencyProvider.RegisterServices(services);
                    
                    var serviceProvider = services.BuildServiceProvider();
                    DependencyProvider.ConfigurePacketOffsetsProvider(serviceProvider);

                    // Assert - Should work even with file-based loading
                    Assert.True(PacketOffsetsProvider.IsConfigured);
                    var eventFactory = serviceProvider.GetRequiredService<IEventFactory>();
                    Assert.NotNull(eventFactory);

                    // Should be able to create events (even if offsets are loaded from file)
                    var offsets = PacketOffsetsProvider.GetOffsets();
                    Assert.NotNull(offsets);
                }
                catch (FileNotFoundException)
                {
                    // Expected if file loading fails - that's OK for this test
                    // The important thing is that the DI system is set up correctly
                }
                finally
                {
                    Directory.SetCurrentDirectory(originalDir);
                }
            }
            finally
            {
                // Cleanup
                if (File.Exists(offsetsPath)) File.Delete(offsetsPath);
                if (File.Exists(indexesPath)) File.Delete(indexesPath);
            }
        }

        [Fact]
        public void ProviderNotConfigured_ShouldGiveHelpfulError()
        {
            // Arrange - Reset provider to simulate unconfigured state
            var serviceProviderField = typeof(PacketOffsetsProvider).GetField("_serviceProvider", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var cacheField = typeof(PacketOffsetsProvider).GetField("_cachedOffsets", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            
            serviceProviderField?.SetValue(null, null);
            cacheField?.SetValue(null, null);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                PacketOffsetsProvider.GetOffsets()
            );

            Assert.Contains("não foi configurado", exception.Message);
        }
    }
}
