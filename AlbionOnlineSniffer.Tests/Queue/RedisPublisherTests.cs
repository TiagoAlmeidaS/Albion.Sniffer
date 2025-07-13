using System.Text.Json;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Queue.Publishers;
using AlbionOnlineSniffer.Queue.Interfaces;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace AlbionOnlineSniffer.Tests.Queue
{
    public class RedisPublisherTests
    {
        [Fact]
        public async Task PublishAsync_ShouldCallPublishAsync()
        {
            // Arrange
            var subscriberMock = new Mock<ISubscriber>();
            subscriberMock.Setup(s => s.PublishAsync(It.IsAny<RedisChannel>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(1);
            var multiplexerMock = new Mock<IConnectionMultiplexer>();
            multiplexerMock.Setup(m => m.GetSubscriber(null)).Returns(subscriberMock.Object);
            var publisher = new RedisPublisherForTest(multiplexerMock.Object);

            var message = new { Test = "Hello" };

            // Act
            await publisher.PublishAsync("test.topic", message);

            // Assert
            subscriberMock.Verify(s => s.PublishAsync(
                "test.topic",
                It.IsAny<RedisValue>(),
                It.IsAny<CommandFlags>()), Times.Once);
        }

        // Classe derivada para injetar mocks
        private class RedisPublisherForTest : RedisPublisher
        {
            public RedisPublisherForTest(IConnectionMultiplexer multiplexer)
                : base(multiplexer) { }
        }
    }
} 