using System;
using System.Numerics;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Interfaces;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models;
using Xunit;
using AlbionOnlineSniffer.Core.Handlers;

namespace AlbionOnlineSniffer.Tests.Core
{
    public class NewCharacterEventHandlerTests
    {
        private class PlayersHandlerMock : IPlayersManager
        {
            public byte[] XorCode { get; set; } = null;
            public bool AddPlayerCalled { get; private set; }
            public void AddPlayer(int id, string name, string guild, string alliance, Vector2 pos, Health health, Faction faction, int[] equipments, int[] spells) { AddPlayerCalled = true; }
            public void Remove(int id) { }
            public void Clear() { }
            public void Mounted(int id, bool mounted) { }
            public void UpdateHealth(int id, int health) { }
            public void SetFaction(int id, Faction faction) { }
            public void RegenerateHealth() { }
            public void UpdateItems(int id, int[] equipments, int[] spells) { }
            public void SetRegeneration(int id, Health health) { }
            public void SyncPlayersPosition() { }
            public void UpdatePlayerPosition(int id, byte[] encryptedPosition, byte[] xorCode, float heading, DateTime timestamp) { }
            public float[] Decrypt(byte[] coordinates, int offset = 0) => new[] { 10f, 20f };
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