using System;
using System.Numerics;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Handlers;
using AlbionOnlineSniffer.Core.Models.Events;
using Xunit;

namespace AlbionOnlineSniffer.Tests.Core
{
    public class NewHarvestableEventHandlerTests
    {
        private class HarvestablesHandlerMock : IHarvestablesHandler
        {
            public bool AddHarvestableCalled { get; private set; }
            public void AddHarvestable(string id, int type, int tier, Vector2 position, int count, int charge)
            {
                AddHarvestableCalled = true;
            }
        }

        [Fact]
        public async Task HandleAsync_ShouldRaiseOnHarvestableParsedEvent()
        {
            // Arrange
            var harvestablesHandler = new HarvestablesHandlerMock();
            var handler = new NewHarvestableEventHandler(harvestablesHandler);
            bool eventRaised = false;
            handler.OnHarvestableParsed += data => { eventRaised = true; };

            var evt = new NewHarvestableEvent
            {
                Id = "harv1",
                Type = 7,
                Tier = 3,
                Position = new Vector2(2, 4),
                Count = 5,
                Charge = 1
            };

            // Act
            await handler.HandleAsync(evt);

            // Assert
            Assert.True(eventRaised);
            Assert.True(harvestablesHandler.AddHarvestableCalled);
        }
    }
} 