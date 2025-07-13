using System;
using System.Numerics;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Handlers;
using AlbionOnlineSniffer.Core.Interfaces;
using AlbionOnlineSniffer.Core.Models.Events;
using Xunit;

namespace AlbionOnlineSniffer.Tests.Core
{
    public class WispGateOpenedEventHandlerTests
    {
        private class GatedWispsHandlerMock : IGatedWispsHandler
        {
            public bool RemoveCalled { get; private set; }
            public void AddWispInGate(string id, Vector2 position)
            {
                throw new NotImplementedException();
            }

            public void Remove(string id)
            {
                RemoveCalled = true;
            }
        }

        [Fact]
        public async Task HandleAsync_ShouldRaiseOnWispGateOpenedParsedEvent_AndCallRemove_WhenCollected()
        {
            // Arrange
            var gatedWispsHandler = new GatedWispsHandlerMock();
            var handler = new WispGateOpenedEventHandler(gatedWispsHandler);
            bool eventRaised = false;
            handler.OnWispGateOpenedParsed += data => { eventRaised = true; };

            var evt = new WispGateOpenedEvent
            {
                Id = "wg1",
                IsCollected = true
            };

            // Act
            await handler.HandleAsync(evt);

            // Assert
            Assert.True(eventRaised);
            Assert.True(gatedWispsHandler.RemoveCalled);
        }
    }
} 