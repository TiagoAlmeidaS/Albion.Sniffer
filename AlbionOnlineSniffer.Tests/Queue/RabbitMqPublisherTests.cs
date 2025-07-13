using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Queue.Publishers;
using AlbionOnlineSniffer.Queue.Interfaces;
using Moq;
using RabbitMQ.Client;
using Xunit;

namespace AlbionOnlineSniffer.Tests.Queue
{
    public class RabbitMqPublisherTests
    {
        [Fact]
        public async Task PublishAsync_ShouldCallBasicPublish()
        {
            // Arrange
            var modelMock = new Mock<IModel>();
            var connectionMock = new Mock<IConnection>();
            connectionMock.Setup(c => c.CreateModel()).Returns(modelMock.Object);
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var publisher = new RabbitMqPublisherForTest(connectionMock.Object, "test-exchange");

            var message = new { Test = "Hello" };

            // Act
            await publisher.PublishAsync("test.topic", message);

            // Assert
            modelMock.Verify(m => m.BasicPublish(
                "test-exchange",
                "test.topic",
                null,
                It.IsAny<byte[]>()), Times.Once);
        }

        // Classe derivada para injetar mocks
        private class RabbitMqPublisherForTest : RabbitMqPublisher
        {
            public RabbitMqPublisherForTest(IConnection connection, string exchange)
                : base(connection, exchange) { }
        }
    }
} 