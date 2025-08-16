using System;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Capture;
using Xunit;

namespace AlbionOnlineSniffer.Tests.Capture
{
    public class PacketCaptureServiceTests
    {
        [Fact(Skip = "Ignorado temporariamente: depende de evento interno não testável")]
        public void OnUdpPayloadCaptured_ShouldBeRaised_WhenPacketArrives()
        {
            // Arrange
            var service = new PacketCaptureService(udpPort: 5050);
            bool eventRaised = false;
            byte[] testPayload = new byte[] { 0x01, 0x02, 0x03 };
            service.OnUdpPayloadCaptured += payload =>
            {
                eventRaised = true;
                Assert.Equal(testPayload, payload);
            };

            // Simular chegada de pacote (chamando o evento manualmente)
            // Como o método Device_OnPacketArrival é privado, simulamos o disparo do evento diretamente
            // Em um cenário real, refatore para permitir injeção/testabilidade
            var eventInfo = service.GetType().GetEvent("OnUdpPayloadCaptured");
            if (eventInfo != null && eventInfo.RaiseMethod != null)
            {
                eventInfo.RaiseMethod.Invoke(service, new object[] { testPayload });
            }

            // Assert
            Assert.True(eventRaised);
        }
    }
}