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
    public class NewMobEventHandlerTests
    {
        private class MobsHandlerMock : IMobsManager
        {
            public bool AddMobCalled { get; private set; }
            public void AddMob(int id, int typeId, Vector2 position, Health health, byte charge) { AddMobCalled = true; }
            public void UpdateMobPosition(int id, byte[] encryptedPosition, byte[] xorCode, float heading, DateTime timestamp) { }
            public void SyncMobsPositions() { }
            public void Remove(int id) { }
            public void Clear() { }
            public void UpdateMobCharge(int id, int charge) { }
            public void UpdateHealth(int id, int health) { }
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