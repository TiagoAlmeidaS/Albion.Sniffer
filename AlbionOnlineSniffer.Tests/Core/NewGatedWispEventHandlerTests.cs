using System;
using System.Numerics;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Handlers;
using AlbionOnlineSniffer.Core.Interfaces;
using AlbionOnlineSniffer.Core.Models.Events;
using Xunit;

namespace AlbionOnlineSniffer.Tests.Core
{
    public class NewGatedWispEventHandlerTests
    {
        private class GatedWispsHandlerMock : IGatedWispsHandler
        {
            public bool AddWispInGateCalled { get; private set; }
            public void AddWispInGate(string id, Vector2 position)
            {
                AddWispInGateCalled = true;
            }

            public void Remove(string id)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public async Task HandleAsync_ShouldRaiseOnGatedWispParsedEvent_AndCallAddWispInGate_WhenNotCollected()
        {
            // Arrange
            var gatedWispsHandler = new GatedWispsHandlerMock();
            var handler = new NewGatedWispEventHandler(gatedWispsHandler);
            bool eventRaised = false;
            handler.OnGatedWispParsed += data => { eventRaised = true; };

            var evt = new NewGatedWispEvent
            {
                Id = "gw1",
                Position = new Vector2(3, 4),
                IsCollected = false
            };

            // Act
            await handler.HandleAsync(evt);

            // Assert
            Assert.True(eventRaised);
            Assert.True(gatedWispsHandler.AddWispInGateCalled);
        }
    }
} 