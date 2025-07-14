using System;
using System.Numerics;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Interfaces;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models;
using AlbionOnlineSniffer.Core.Handlers;
using Xunit;

namespace AlbionOnlineSniffer.Tests.Core
{
    public class NewDungeonEventHandlerTests
    {
        private class DungeonsHandlerMock : IDungeonsManager
        {
            public bool AddDungeonCalled { get; private set; }
            public void AddDungeon(int id, string type, Vector2 position, int charges) { AddDungeonCalled = true; }
            public void Remove(int id) { }
            public void Clear() { }
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