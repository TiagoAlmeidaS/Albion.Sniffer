using System;
using System.Numerics;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Interfaces;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Handlers;
using Xunit;

namespace AlbionOnlineSniffer.Tests.Core
{
    public class NewFishingZoneEventHandlerTests
    {
        private class FishNodesHandlerMock : IFishNodesManager
        {
            public bool AddFishZoneCalled { get; private set; }
            public void AddFishZone(int id, Vector2 position, int size, int respawnCount) { AddFishZoneCalled = true; }
            public void Remove(int id) { }
            public void Clear() { }
        }

        [Fact]
        public async Task HandleAsync_ShouldRaiseOnFishingZoneParsedEvent()
        {
            // Arrange
            var fishNodesHandler = new FishNodesHandlerMock();
            var handler = new NewFishingZoneEventHandler(fishNodesHandler);
            bool eventRaised = false;
            handler.OnFishingZoneParsed += data => { eventRaised = true; };

            var evt = new NewFishingZoneEvent
            {
                Id = "fz1",
                Position = new Vector2(7, 8),
                Size = 2,
                RespawnCount = 6
            };

            // Act
            await handler.HandleAsync(evt);

            // Assert
            Assert.True(eventRaised);
            Assert.True(fishNodesHandler.AddFishZoneCalled);
        }
    }
} 