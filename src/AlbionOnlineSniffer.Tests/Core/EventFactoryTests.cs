using System;
using System.Collections.Generic;
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
    public class EventFactoryTests
    {
        private readonly PacketOffsets _testPacketOffsets;
        private readonly IEventFactory _eventFactory;

        public EventFactoryTests()
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

            _eventFactory = new EventFactory(_testPacketOffsets);
        }

        [Fact]
        public void Constructor_WithNullPacketOffsets_ShouldThrowException()
        {
            // Act & Assert
            #pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type
            var exception = Assert.Throws<ArgumentNullException>(() => new EventFactory(null));
            #pragma warning restore CS8625
            Assert.Equal("packetOffsets", exception.ParamName);
        }

        [Fact]
        public void CreateEvent_Generic_WithValidParameters_ShouldCreateMoveEvent()
        {
            // Arrange
            var parameters = new Dictionary<byte, object>
            {
                { 0, 12345 },
                { 1, new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 } }
            };

            // Act
            var moveEvent = _eventFactory.CreateEvent<MoveEvent>(parameters);

            // Assert
            Assert.NotNull(moveEvent);
            Assert.Equal(12345, moveEvent.Id);
        }

        [Fact]
        public void CreateEvent_NonGeneric_WithValidParameters_ShouldCreateMoveEvent()
        {
            // Arrange
            var parameters = new Dictionary<byte, object>
            {
                { 0, 54321 },
                { 1, new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 } }
            };

            // Act
            var moveEvent = _eventFactory.CreateEvent(typeof(MoveEvent), parameters) as MoveEvent;

            // Assert
            Assert.NotNull(moveEvent);
            Assert.Equal(54321, moveEvent.Id);
        }

        [Fact]
        public void CreateEvent_WithChangeClusterEvent_ShouldCreateWithCorrectOffsets()
        {
            // Arrange
            var parameters = new Dictionary<byte, object>
            {
                { 0, "TestLocation" },
                { 1, "TestType" },
                { 2, new byte[] { 1, 2, 3, 4 } }
            };

            // Act
            var changeClusterEvent = _eventFactory.CreateEvent<ChangeClusterEvent>(parameters);

            // Assert
            Assert.NotNull(changeClusterEvent);
            Assert.Equal("TestLocation", changeClusterEvent.LocationId);
            Assert.Equal("TestType", changeClusterEvent.Type);
        }

        [Fact]
        public void CreateEvent_WithNewHarvestableEvent_ShouldCreateWithCorrectProperties()
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
            var harvestableEvent = _eventFactory.CreateEvent<NewHarvestableEvent>(parameters);

            // Assert
            Assert.NotNull(harvestableEvent);
            Assert.Equal(123, harvestableEvent.Id);
            Assert.Equal(456, harvestableEvent.TypeId);
            Assert.Equal(5, harvestableEvent.Tier);
            Assert.Equal(10, harvestableEvent.Charges);
        }

        [Fact]
        public void CreateEvent_WithHealthUpdateEvent_ShouldCreateWithCorrectProperties()
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
            var healthEvent = _eventFactory.CreateEvent<HealthUpdateEvent>(parameters);

            // Assert
            Assert.NotNull(healthEvent);
            Assert.Equal(789, healthEvent.Id);
            Assert.Equal(75.5f, healthEvent.Health);
            Assert.Equal(100.0f, healthEvent.MaxHealth);
            Assert.Equal(50.0f, healthEvent.Energy);
        }

        [Fact]
        public void CreateEvent_WithNullEventType_ShouldThrowException()
        {
            // Arrange
            var parameters = new Dictionary<byte, object>();

            // Act & Assert
            #pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type
            var exception = Assert.Throws<ArgumentNullException>(() => _eventFactory.CreateEvent(null, parameters));
            #pragma warning restore CS8625
            Assert.Equal("eventType", exception.ParamName);
        }

        [Fact]
        public void CreateEvent_WithNullParameters_ShouldThrowException()
        {
            // Act & Assert
            #pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type
            var exception = Assert.Throws<ArgumentNullException>(() => _eventFactory.CreateEvent<MoveEvent>(null));
            #pragma warning restore CS8625
            Assert.Equal("parameters", exception.ParamName);
        }

        [Fact]
        public void CreateEvent_WithInvalidEventType_ShouldThrowException()
        {
            // Arrange
            var parameters = new Dictionary<byte, object>();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _eventFactory.CreateEvent(typeof(string), parameters));
            Assert.Contains("construtor adequado", exception.Message);
        }

        [Fact]
        public void CreateEvent_WithLeaveEvent_ShouldCreateSimpleEvent()
        {
            // Arrange
            var parameters = new Dictionary<byte, object>
            {
                { 0, 999 }
            };

            // Act
            var leaveEvent = _eventFactory.CreateEvent<LeaveEvent>(parameters);

            // Assert
            Assert.NotNull(leaveEvent);
            Assert.Equal(999, leaveEvent.Id);
        }

        [Fact]
        public void CreateEvent_WithMobChangeStateEvent_ShouldCreateWithCorrectProperties()
        {
            // Arrange
            var parameters = new Dictionary<byte, object>
            {
                { 0, 555 },
                { 1, 3 }
            };

            // Act
            var mobChangeEvent = _eventFactory.CreateEvent<MobChangeStateEvent>(parameters);

            // Assert
            Assert.NotNull(mobChangeEvent);
            Assert.Equal(555, mobChangeEvent.Id);
            Assert.Equal(3, mobChangeEvent.Charge);
        }
    }
}
