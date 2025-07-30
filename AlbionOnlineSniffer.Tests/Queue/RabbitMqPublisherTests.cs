using System;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Queue.Publishers;
using Xunit;

namespace AlbionOnlineSniffer.Tests.Queue
{
    public class RabbitMqPublisherTests
    {
        [Fact]
        public void Constructor_WithValidConnectionString_ShouldNotThrow()
        {
            // Arrange & Act
            var exception = Record.Exception(() => 
                new RabbitMqPublisher("amqp://localhost", "test.exchange"));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public async Task PublishAsync_WithValidMessage_ShouldNotThrow()
        {
            // Arrange
            var publisher = new RabbitMqPublisher("amqp://localhost", "test.exchange");
            var message = new { Test = "data" };

            // Act & Assert
            var exception = await Record.ExceptionAsync(async () => 
                await publisher.PublishAsync("test.topic", message));

            Assert.Null(exception);
        }
    }
} 