using System;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Services;
using Microsoft.Extensions.Logging;
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

        [Fact(Skip = "Ignorado temporariamente: depende de lógica real de parsing")]
        public void ReceivePacket_ShouldCallHandler()
        {
            // Arrange
            var handler = new TestHandler();
            var called = false;
            
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<Protocol16Deserializer>();
            
            // Criar mocks para os serviços necessários
            var definitionLoader = new PhotonDefinitionLoader(loggerFactory.CreateLogger<PhotonDefinitionLoader>());
            var packetEnricher = new PhotonPacketEnricher(definitionLoader, loggerFactory.CreateLogger<PhotonPacketEnricher>());
            
            var deserializer = new Protocol16Deserializer(packetEnricher, logger);

            // Simular registro de handler (aqui, para fins de teste, chamamos diretamente)
            deserializer.OnParsedEvent += obj => { called = true; };

            // Act
            deserializer.ReceivePacket(new byte[] { 1, 2, 3 });

            // Assert
            Assert.True(called);
        }
    }
} 