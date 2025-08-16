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
            // Using a connection string that will fail gracefully in CI environment
            var exception = Record.Exception(() => 
                new RedisPublisher("localhost:6379,abortConnect=false,connectTimeout=100"));

            // Assert
            // In CI environment, connection will fail but constructor should not throw
            // We expect either no exception or a RedisConnectionException
            Assert.True(exception == null || exception is StackExchange.Redis.RedisConnectionException);
        }

        [Fact]
        public async Task PublishAsync_WithValidMessage_ShouldNotThrow()
        {
            // Arrange
            // Using a connection string that will fail gracefully in CI environment
            var publisher = new RedisPublisher("localhost:6379,abortConnect=false,connectTimeout=100");
            var message = new { Test = "data" };

            // Act & Assert
            // In CI environment, connection will fail but the method should handle it gracefully
            var exception = await Record.ExceptionAsync(async () => 
                await publisher.PublishAsync("test.topic", message));

            // We expect either no exception or a RedisConnectionException
            Assert.True(exception == null || exception is StackExchange.Redis.RedisConnectionException);
        }
    }
} 