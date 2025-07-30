using System;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Queue.Publishers;
using Xunit;

namespace AlbionOnlineSniffer.Tests.Queue
{
    public class RedisPublisherTests
    {
        [Fact]
        public void Constructor_WithValidConnectionString_ShouldNotThrow()
        {
            // Arrange & Act
            var exception = Record.Exception(() => 
                new RedisPublisher("localhost:6379"));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public async Task PublishAsync_WithValidMessage_ShouldNotThrow()
        {
            // Arrange
            var publisher = new RedisPublisher("localhost:6379");
            var message = new { Test = "data" };

            // Act & Assert
            var exception = await Record.ExceptionAsync(async () => 
                await publisher.PublishAsync("test.topic", message));

            Assert.Null(exception);
        }
    }
} 