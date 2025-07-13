using System;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Services;
using Xunit;

namespace AlbionOnlineSniffer.Tests.Core
{
    public class Protocol16DeserializerTests
    {
        private class TestHandler
        {
            public bool EventCalled { get; private set; }
            public void Handle(byte[] payload)
            {
                EventCalled = true;
            }
        }

        private class PhotonReceiverMock : Albion.Network.IPhotonReceiver
        {
            private readonly Action<byte[]> _onReceive;
            public PhotonReceiverMock(Action<byte[]> onReceive)
            {
                _onReceive = onReceive;
            }
            public void ReceivePacket(byte[] payload) => _onReceive(payload);
        }

        [Fact]
        public void ReceivePacket_ShouldCallHandler()
        {
            // Arrange
            var handler = new TestHandler();
            var called = false;
            var deserializer = new Protocol16Deserializer();

            // Simular registro de handler (aqui, para fins de teste, chamamos diretamente)
            deserializer.OnParsedEvent += obj => { called = true; };

            // Act
            deserializer.ReceivePacket(new byte[] { 1, 2, 3 });

            // Assert
            Assert.True(called);
        }
    }
} 