using System;
using System.Numerics;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Handlers;
using AlbionOnlineSniffer.Core.Models.Events;
using Xunit;

namespace AlbionOnlineSniffer.Tests.Core
{
    public class NewMobEventHandlerTests
    {
        private class MobsHandlerMock : IMobsHandler
        {
            public bool AddMobCalled { get; private set; }
            public void AddMob(string id, int typeId, Vector2 position, int health, int charge)
            {
                AddMobCalled = true;
            }
        }

        [Fact]
        public async Task HandleAsync_ShouldRaiseOnMobParsedEvent()
        {
            // Arrange
            var mobsHandler = new MobsHandlerMock();
            var handler = new NewMobEventHandler(mobsHandler);
            bool eventRaised = false;
            handler.OnMobParsed += data => { eventRaised = true; };

            var evt = new NewMobEvent
            {
                Id = "mob1",
                TypeId = 42,
                Position = new Vector2(5, 10),
                Health = 200,
                Charge = 3
            };

            // Act
            await handler.HandleAsync(evt);

            // Assert
            Assert.True(eventRaised);
            Assert.True(mobsHandler.AddMobCalled);
        }
    }
} 