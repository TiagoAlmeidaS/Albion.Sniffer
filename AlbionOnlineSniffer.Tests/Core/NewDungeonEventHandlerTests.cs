using System;
using System.Numerics;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Handlers;
using AlbionOnlineSniffer.Core.Models.Events;
using Xunit;

namespace AlbionOnlineSniffer.Tests.Core
{
    public class NewDungeonEventHandlerTests
    {
        private class DungeonsHandlerMock : IDungeonsHandler
        {
            public bool AddDungeonCalled { get; private set; }
            public void AddDungeon(string id, int type, Vector2 position, int charges)
            {
                AddDungeonCalled = true;
            }
        }

        [Fact]
        public async Task HandleAsync_ShouldRaiseOnDungeonParsedEvent()
        {
            // Arrange
            var dungeonsHandler = new DungeonsHandlerMock();
            var handler = new NewDungeonEventHandler(dungeonsHandler);
            bool eventRaised = false;
            handler.OnDungeonParsed += data => { eventRaised = true; };

            var evt = new NewDungeonEvent
            {
                Id = "dungeon1",
                Type = 3,
                Position = new Vector2(11, 12),
                Charges = 4
            };

            // Act
            await handler.HandleAsync(evt);

            // Assert
            Assert.True(eventRaised);
            Assert.True(dungeonsHandler.AddDungeonCalled);
        }
    }
} 