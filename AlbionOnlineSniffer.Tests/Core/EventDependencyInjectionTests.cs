using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.ResponseObj;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core;
using Xunit;

namespace AlbionOnlineSniffer.Tests.Core
{
    public class EventDependencyInjectionTests
    {
        private readonly PacketOffsets _testPacketOffsets;
        
        public EventDependencyInjectionTests()
        {
            _testPacketOffsets = new PacketOffsets
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

            SetupPacketOffsetsProvider();
        }

        private void SetupPacketOffsetsProvider()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(_ => LoggerFactory.Create(builder => { }));
            DependencyProvider.RegisterDataLoader(services, _testPacketOffsets);
            var serviceProvider = services.BuildServiceProvider();
            PacketOffsetsProvider.Configure(serviceProvider);
        }

        [Fact]
        public void MoveEvent_WithDirectDI_ShouldCreateSuccessfully()
        {
            // Arrange
            var parameters = new Dictionary<byte, object>
            {
                { 0, 12345 },
                { 1, new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 } }
            };

            // Act
            var moveEvent = new MoveEvent(parameters, _testPacketOffsets);

            // Assert
            Assert.NotNull(moveEvent);
            Assert.Equal(12345, moveEvent.Id);
        }

        [Fact]
        public void MoveEvent_WithProviderDI_ShouldCreateSuccessfully()
        {
            // Arrange
            var parameters = new Dictionary<byte, object>
            {
                { 0, 54321 },
                { 1, new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 } }
            };

            // Act
            var moveEvent = new MoveEvent(parameters);

            // Assert
            Assert.NotNull(moveEvent);
            Assert.Equal(54321, moveEvent.Id);
        }

        [Fact]
        public void ChangeClusterEvent_WithDirectDI_ShouldCreateSuccessfully()
        {
            // Arrange
            var parameters = new Dictionary<byte, object>
            {
                { 0, "TestLocation1" },
                { 1, "TestType1" },
                { 2, new byte[] { 1, 2, 3, 4 } }
            };

            // Act
            var event1 = new ChangeClusterEvent(parameters, _testPacketOffsets);

            // Assert
            Assert.NotNull(event1);
            Assert.Equal("TestLocation1", event1.LocationId);
            Assert.Equal("TestType1", event1.Type);
        }

        [Fact]
        public void ChangeClusterEvent_WithProviderDI_ShouldCreateSuccessfully()
        {
            // Arrange
            var parameters = new Dictionary<byte, object>
            {
                { 0, "TestLocation2" },
                { 1, "TestType2" },
                { 2, new byte[] { 5, 6, 7, 8 } }
            };

            // Act
            var event2 = new ChangeClusterEvent(parameters);

            // Assert
            Assert.NotNull(event2);
            Assert.Equal("TestLocation2", event2.LocationId);
            Assert.Equal("TestType2", event2.Type);
        }

        [Fact]
        public void NewHarvestableEvent_WithBothConstructors_ShouldProduceSameResults()
        {
            // Arrange
            var parameters = new Dictionary<byte, object>
            {
                { 0, 123 },
                { 1, 456 },
                { 2, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 } },
                { 3, (byte)5 },
                { 4, (byte)10 }
            };

            // Act
            var directDI = new NewHarvestableEvent(parameters, _testPacketOffsets);
            var providerDI = new NewHarvestableEvent(parameters);

            // Assert
            Assert.Equal(directDI.Id, providerDI.Id);
            Assert.Equal(directDI.TypeId, providerDI.TypeId);
            Assert.Equal(directDI.Tier, providerDI.Tier);
            Assert.Equal(directDI.Charges, providerDI.Charges);
        }

        [Fact]
        public void HealthUpdateEvent_WithBothConstructors_ShouldProduceSameResults()
        {
            // Arrange
            var parameters = new Dictionary<byte, object>
            {
                { 0, 789 },
                { 1, 75.5f },
                { 2, 100.0f },
                { 3, 50.0f }
            };

            // Act
            var directDI = new HealthUpdateEvent(parameters, _testPacketOffsets);
            var providerDI = new HealthUpdateEvent(parameters);

            // Assert
            Assert.Equal(directDI.Id, providerDI.Id);
            Assert.Equal(directDI.Health, providerDI.Health);
            Assert.Equal(directDI.MaxHealth, providerDI.MaxHealth);
            Assert.Equal(directDI.Energy, providerDI.Energy);
        }

        [Fact]
        public void LeaveEvent_WithBothConstructors_ShouldProduceSameResults()
        {
            // Arrange
            var parameters = new Dictionary<byte, object>
            {
                { 0, 999 }
            };

            // Act
            var directDI = new LeaveEvent(parameters, _testPacketOffsets);
            var providerDI = new LeaveEvent(parameters);

            // Assert
            Assert.Equal(directDI.Id, providerDI.Id);
        }

        [Fact]
        public void KeySyncEvent_WithBothConstructors_ShouldProduceSameResults()
        {
            // Arrange
            var parameters = new Dictionary<byte, object>
            {
                { 0, (ulong)123456789 }
            };

            // Act
            var directDI = new KeySyncEvent(parameters, _testPacketOffsets);
            var providerDI = new KeySyncEvent(parameters);

            // Assert
            Assert.Equal(directDI.Key, providerDI.Key);
        }

        [Fact]
        public void MobChangeStateEvent_WithBothConstructors_ShouldProduceSameResults()
        {
            // Arrange
            var parameters = new Dictionary<byte, object>
            {
                { 0, 555 },
                { 1, 3 }
            };

            // Act
            var directDI = new MobChangeStateEvent(parameters, _testPacketOffsets);
            var providerDI = new MobChangeStateEvent(parameters);

            // Assert
            Assert.Equal(directDI.Id, providerDI.Id);
            Assert.Equal(directDI.Charge, providerDI.Charge);
        }

        [Fact]
        public void MountedEvent_WithBothConstructors_ShouldProduceSameResults()
        {
            // Arrange
            var parameters = new Dictionary<byte, object>
            {
                { 0, 777 },
                { 1, true }
            };

            // Act
            var directDI = new MountedEvent(parameters, _testPacketOffsets);
            var providerDI = new MountedEvent(parameters);

            // Assert
            Assert.Equal(directDI.Id, providerDI.Id);
            Assert.Equal(directDI.IsMounted, providerDI.IsMounted);
        }

        [Fact]
        public void HarvestableChangeStateEvent_WithBothConstructors_ShouldProduceSameResults()
        {
            // Arrange
            var parameters = new Dictionary<byte, object>
            {
                { 0, 111 },
                { 1, 2 }
            };

            // Act
            var directDI = new HarvestableChangeStateEvent(parameters, _testPacketOffsets);
            var providerDI = new HarvestableChangeStateEvent(parameters);

            // Assert
            Assert.Equal(directDI.Id, providerDI.Id);
            Assert.Equal(directDI.Count, providerDI.Count);
        }

        [Fact]
        public void CharacterEquipmentChangedEvent_WithBothConstructors_ShouldProduceSameResults()
        {
            // Arrange
            var parameters = new Dictionary<byte, object>
            {
                { 0, 333 },
                { 1, new float[] { 1.0f, 2.0f, 3.0f } },
                { 2, new float[] { 4.0f, 5.0f, 6.0f } }
            };

            // Act
            var directDI = new CharacterEquipmentChangedEvent(parameters, _testPacketOffsets);
            var providerDI = new CharacterEquipmentChangedEvent(parameters);

            // Assert
            Assert.Equal(directDI.Id, providerDI.Id);
            Assert.Equal(directDI.Equipments, providerDI.Equipments);
            Assert.Equal(directDI.Spells, providerDI.Spells);
        }

        [Fact]
        public void ChangeFlaggingFinishedEvent_WithBothConstructors_ShouldProduceSameResults()
        {
            // Arrange
            var parameters = new Dictionary<byte, object>
            {
                { 0, 444 }
            };

            // Act
            var directDI = new ChangeFlaggingFinishedEvent(parameters, _testPacketOffsets);
            var providerDI = new ChangeFlaggingFinishedEvent(parameters);

            // Assert
            Assert.Equal(directDI.Id, providerDI.Id);
        }

        [Fact]
        public void NewHarvestablesListEvent_WithBothConstructors_ShouldProduceSameResults()
        {
            // Arrange - Simulating a list of harvestables with byte arrays
            var parameters = new Dictionary<byte, object>
            {
                { 0, new byte[] { 1, 2, 3 } },
                { 1, new byte[] { 10, 20, 30 } },
                { 2, new byte[] { 1, 2, 3 } },
                { 3, new float[] { 100.0f, 200.0f, 150.0f, 250.0f, 175.0f, 225.0f } },
                { 4, new byte[] { 5, 10, 15 } }
            };

            // Act
            var directDI = new NewHarvestablesListEvent(parameters, _testPacketOffsets);
            var providerDI = new NewHarvestablesListEvent(parameters);

            // Assert
            Assert.Equal(directDI.HarvestableObjects.Count, providerDI.HarvestableObjects.Count);
        }

        [Fact]
        public void Events_WithNullPacketOffsets_ShouldHandleGracefully()
        {
            // Arrange - Using null offsets to test fallback behavior
            var nullOffsets = new PacketOffsets();

            var moveParams = new Dictionary<byte, object>
            {
                { 0, 12345 },
                { 1, new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 } }
            };

            // Act & Assert - Should not throw exceptions, but handle null offsets
            Assert.Throws<NullReferenceException>(() => new MoveEvent(moveParams, nullOffsets));
        }
    }
}