using System;
using System.Numerics;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Handlers;
using Xunit;

namespace AlbionOnlineSniffer.Tests.Core
{
    public class NewLootChestEventHandlerTests
    {
        private class LootChestsHandlerMock : ILootChestsHandler
        {
            public bool AddWorldChestCalled { get; private set; }
            public void AddWorldChest(string id, Vector2 position, string name, int enchLvl)
            {
                AddWorldChestCalled = true;
            }
        }

        [Fact]
        public async Task HandleAsync_ShouldRaiseOnLootChestParsedEvent()
        {
            // Arrange
            var lootChestsHandler = new LootChestsHandlerMock();
            var handler = new NewLootChestEventHandler(lootChestsHandler);
            bool eventRaised = false;
            handler.OnLootChestParsed += data => { eventRaised = true; };

            var evt = new NewLootChestEvent
            {
                Id = "chest1",
                Position = new Vector2(8, 9),
                Name = "Golden Chest",
                EnchLvl = 2
            };

            // Act
            await handler.HandleAsync(evt);

            // Assert
            Assert.True(eventRaised);
            Assert.True(lootChestsHandler.AddWorldChestCalled);
        }
    }
} 