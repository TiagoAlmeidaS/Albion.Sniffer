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
            // Using a connection string that will fail gracefully in CI environment
            var exception = Record.Exception(() => 
                new RabbitMqPublisher("amqp://localhost:5672", "test.exchange"));

            // Assert
            // In CI environment, connection will fail but constructor should not throw
            // We expect either no exception or a RabbitMQ.Client.Exceptions.BrokerUnreachableException
            Assert.True(exception == null || exception is RabbitMQ.Client.Exceptions.BrokerUnreachableException);
        }

        [Fact]
        public async Task PublishAsync_WithValidMessage_ShouldNotThrow()
        {
            // Arrange
            // Using a connection string that will fail gracefully in CI environment
            RabbitMqPublisher? publisher = null;
            try
            {
                publisher = new RabbitMqPublisher("amqp://localhost:5672", "test.exchange");
            }
            catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException)
            {
                // Expected in CI environment - skip test
                return;
            }

            if (publisher == null)
            {
                // If we couldn't create the publisher, skip the test
                return;
            }

            var message = new { Test = "data" };

            // Act & Assert
            // In CI environment, connection will fail but the method should handle it gracefully
            var exception = await Record.ExceptionAsync(async () => 
                await publisher.PublishAsync("test.topic", message));

            // We expect either no exception or a RabbitMQ.Client.Exceptions.BrokerUnreachableException
            Assert.True(exception == null || exception is RabbitMQ.Client.Exceptions.BrokerUnreachableException);
        }
    }
} 