using System;
using System.Numerics;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Handlers;
using Xunit;

namespace AlbionOnlineSniffer.Tests.Core
{
    public class NewCharacterEventHandlerTests
    {
        private class PlayersHandlerMock : IPlayersHandler
        {
            public object XorCode => null;
            public int[] Decrypt(object encryptedPosition) => new[] { 10, 20 };
            public bool AddPlayerCalled { get; private set; }
            public void AddPlayer(string id, string name, string guild, string alliance, Vector2 pos, int health, int faction, object equipments, object spells)
            {
                AddPlayerCalled = true;
            }
        }

        private class LocalPlayerHandlerMock : ILocalPlayerHandler { }
        private class ConfigHandlerMock : IConfigHandler { }

        [Fact]
        public async Task HandleAsync_ShouldRaiseOnCharacterParsedEvent()
        {
            // Arrange
            var playersHandler = new PlayersHandlerMock();
            var localPlayerHandler = new LocalPlayerHandlerMock();
            var configHandler = new ConfigHandlerMock();
            var handler = new NewCharacterEventHandler(playersHandler, localPlayerHandler, configHandler);
            bool eventRaised = false;
            handler.OnCharacterParsed += data => { eventRaised = true; };

            var evt = new NewCharacterEvent
            {
                Id = "char1",
                Name = "TestChar",
                Guild = "TestGuild",
                Alliance = "TestAlliance",
                Position = new Vector2(1, 2),
                Health = 100,
                Faction = 1,
                Equipments = null,
                Spells = null
            };

            // Act
            await handler.HandleAsync(evt);

            // Assert
            Assert.True(eventRaised);
            Assert.True(playersHandler.AddPlayerCalled);
        }
    }
} 